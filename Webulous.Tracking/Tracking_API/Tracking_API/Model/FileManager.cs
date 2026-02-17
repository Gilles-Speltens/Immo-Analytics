using System.Collections;
using System.Collections.Concurrent;

namespace Tracking_API.Model
{

    /// <summary>
    /// Fournit des méthodes pour lire et écrire des fichiers texte.
    /// </summary>
    public class FileManager
    {
        private string _path;

        /// <summary>
        /// Initialise le FileManager avec un chemin de fichier.
        /// </summary>
        /// <param name="path">Chemin du fichier à gérer</param>
        public FileManager(string path)
        {
            _path = path; 
        }

        /// <summary>
        /// Change le chemin du fichier à gérer.
        /// </summary>
        /// <param name="newPath">Nouveau chemin du fichier</param>
        public void changePath(string newPath)
        {
            if (newPath != null)
            {
                _path = newPath;
            }
        }

        /// <summary>
        /// Ajoute les lignes d'une ConcurrentQueue dans le fichier de manière asynchrone.
        /// Les lignes sont écrites dans l'ordre du dequeuing.
        /// </summary>
        /// <param name="queue">Queue contenant les lignes à ajouter</param>
        /// <returns>Task représentant l'opération asynchrone</returns>
        public async Task AppendFromQueue(ConcurrentQueue<string> queue)
        {
            await using (var fs = File.AppendText(_path))
            {
                while (queue.TryDequeue(out var line))
                {
                    await fs.WriteLineAsync(line);
                }
            }
        }

        /// <summary>
        /// Écrase le contenu du fichier avec les lignes fournies.
        /// Cette opération est synchrone.
        /// </summary>
        /// <param name="list">Liste de lignes à écrire dans le fichier</param>
        public void OverwriteFromList(List<string> list)
        {
            using (var fs = new StreamWriter(_path, false)) // false = overwrite
            {
                foreach (var line in list)
                {
                    fs.WriteLineAsync(line);
                }
            }
        }

        /// <summary>
        /// Lit toutes les lignes du fichier et les retourne sous forme de tableau de chaînes.
        /// </summary>
        /// <returns>Tableau de chaînes contenant toutes les lignes du fichier</returns>
        public string[] ReadFile()
        {
            return File.ReadAllLines(_path);
        }
    }
}
