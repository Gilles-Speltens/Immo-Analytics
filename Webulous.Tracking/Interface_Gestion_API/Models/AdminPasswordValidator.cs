using System.Security.Cryptography;
using System.Text;

namespace Interface_Gestion_API.Models
{
    /// <summary>
    /// Classe responsable de la validation du mot de passe administrateur.
    /// <para>
    /// Cette classe permet de stocker de manière sécurisée un mot de passe administrateur
    /// sous forme de hash SHA-256 et de vérifier la correspondance avec un mot de passe fourni.
    /// </para>
    /// <para>
    /// Destiné à un accès interne simple, cette classe ne gère pas d'utilisateurs multiples ni de rôles.
    /// </para>
    /// </summary>
    public class AdminPasswordValidator
    {
        private readonly string _passwordHash;

        /// <summary>
        /// Initialise un nouvel objet <see cref="AdminPasswordValidator"/> avec un mot de passe administrateur.
        /// </summary>
        /// <param name="password">Mot de passe administrateur en clair.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="password"/> est null.</exception>
        public AdminPasswordValidator(string password)
        {
            if (password == null) throw new ArgumentNullException("Le mot de passe est null");

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            _passwordHash = Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        /// <summary>
        /// Vérifie si le mot de passe fourni correspond au mot de passe administrateur.
        /// </summary>
        /// <param name="psw">Mot de passe à valider.</param>
        /// <returns>
        /// <c>true</c> si le mot de passe fourni correspond au mot de passe administrateur, 
        /// <c>false</c> si le mot de passe est incorrect, vide ou null.
        /// </returns>
        public bool ValidPassword(string psw)
        {
            if (string.IsNullOrEmpty(psw))
                return false;

            using var sha = SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(psw)));
            return _passwordHash == hash;

        }
    }
}
