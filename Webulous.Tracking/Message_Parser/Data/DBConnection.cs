using MySqlConnector;

namespace Message_Parser.Data
{
    /// <summary>
    /// Fournit une méthode centralisée pour la création de connexions à la base de données MySQL.
    /// </summary>
    /// <remarks>
    /// La classe <see cref="DBConnection"/> encapsule la chaîne de connexion et permet
    /// de créer des instances de <see cref="MySqlConnection"/> à la demande.
    /// Elle est généralement utilisée dans le cadre du pattern <c>Unit of Work</c>
    /// ou par les différents repositories pour garantir une gestion cohérente
    /// des connexions à la base de données.
    /// </remarks>
    public class DBConnection
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="DBConnection"/>.
        /// </summary>
        /// <param name="connectionString">
        /// Chaîne de connexion à la base de données MySQL.
        /// Elle doit contenir toutes les informations nécessaires telles que
        /// le serveur, la base de données, l'utilisateur et le mot de passe.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Levée lorsque la chaîne de connexion est <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Levée lorsque la chaîne de connexion est vide ou composée uniquement d'espaces.
        /// </exception>
        public DBConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Crée une nouvelle instance de <see cref="MySqlConnection"/> en utilisant
        /// la chaîne de connexion fournie lors de l'initialisation.
        /// </summary>
        /// <returns>
        /// Une instance de <see cref="MySqlConnection"/> prête à être ouverte.
        /// </returns>
        /// <remarks>
        /// La connexion retournée n'est pas ouverte automatiquement. Il est de la
        /// responsabilité de l'appelant d'appeler <see cref="MySqlConnection.Open"/>
        /// ou <see cref="MySqlConnection.OpenAsync"/> avant son utilisation.
        /// </remarks>
        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
