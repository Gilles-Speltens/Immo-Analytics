using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Message_Parser.Reposiroties
{
    /// <summary>
    /// Repository responsable de l’accès aux données de la table Site.
    /// Permet l’insertion simple, l’insertion en batch et la récupération des domaines.
    /// </summary>
    public class SiteRepository : BaseRepository
    {
        /// <summary>
        /// Insère un site en base de données.
        /// </summary>
        /// <param name="site">Site à insérer.</param>
        /// <returns>True si l’insertion a réussi.</returns>
        public bool Insert(Site site, MySqlConnection connection)
        {
            int rows = connection.Execute(
                "INSERT INTO Site (Domain, Date_When_Added, Certify) VALUES (@Domain, @DateWhenAdded, @Certify)",
                site);

            return rows == 1;
        }

        /// <summary>
        /// Insère une liste de sites en base de données en utilisant des batchs.
        /// </summary>
        /// <param name="sites">Liste des sites.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre total de lignes insérées.</returns>
        public Task<int> BulkInsert(List<Site> sites, IDbTransaction? transaction, MySqlConnection connection)
        {
            return BulkInsertInternal(sites, BatchInsert, transaction, connection);
        }

        /// <summary>
        /// Vérifie si un domaine existe déjà en base de données.
        /// </summary>
        /// <param name="domain">Nom de domaine.</param>
        /// <returns>True si le domaine existe.</returns>
        public async Task<bool> Contains(string domain, MySqlConnection connection)
        {
            return connection.Execute("SELECT 1 FROM Site WHERE domain = (@Domain)", new { Domain = domain }) == 1;
        }

        /// <summary>
        /// Récupère la liste de tous les domaines enregistrés.
        /// </summary>
        /// <returns>Liste des domaines.</returns>
        public List<string> GetAllDomain(MySqlConnection connection)
        {
            return connection.Query<string>("SELECT Domain FROM Site").ToList();
        }

        /// <summary>
        /// Méthode interne permettant d’insérer un batch de sites
        /// en utilisant une requête SQL multi-values.
        /// Utilise INSERT IGNORE pour éviter les erreurs en cas de doublons.
        /// </summary>
        /// <param name="batch">Batch de sites.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre de lignes insérées.</returns>
        private async Task<int> BatchInsert(List<Site> batch, IDbTransaction? transaction, MySqlConnection connection)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Domain{i}, @DateWhenAdded{i}, @Certify{i}),");

                parameters.Add($"Domain{i}", batch[i].Domain);
                parameters.Add($"DateWhenAdded{i}", batch[i].DateWhenAdded);
                parameters.Add($"Certify{i}", batch[i].Certify);
            }

            sqlValues.Length--;

            var sql = $"""
                INSERT IGNORE INTO Site (Domain, Date_When_Added, Certify) 
                VALUES {sqlValues}
                """;

            return await connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
