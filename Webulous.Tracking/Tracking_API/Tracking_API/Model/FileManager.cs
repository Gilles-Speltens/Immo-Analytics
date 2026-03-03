using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace Tracking_API.Model
{

    /// <summary>
    /// Fournit des méthodes pour lire et écrire des fichiers texte.
    /// </summary>
    public class FileManager : IFileManager
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
        public void ChangePath(string newPath)
        {
            if (string.IsNullOrEmpty(newPath))
                return;

            using (var fs = new StreamWriter(_path, append: true, encoding: Encoding.UTF8))
            {
                fs.Write("]");
            }

            _path = newPath;

            using (var fs = new StreamWriter(_path, append: false, encoding: Encoding.UTF8))
            {
                fs.Write("[\n");
            }
        }

        /// <summary>
        /// Ajoute les lignes d'une ConcurrentQueue dans le fichier de manière asynchrone.
        /// Les lignes sont écrites dans l'ordre du dequeuing.
        /// </summary>
        /// <param name="queue">Queue contenant les lignes à ajouter</param>
        /// <returns>Task représentant l'opération asynchrone</returns>
        public async Task AppendFromQueue(ConcurrentQueue<byte[]> queue)
        {
            if (queue.IsEmpty)
                return;

            await using var fs = new FileStream(
                _path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous);

            while (queue.TryDequeue(out var line))
            {
                await fs.WriteAsync("   "u8.ToArray());
                await fs.WriteAsync(line);
                await fs.WriteAsync(",\n"u8.ToArray());
            }

            await fs.FlushAsync();
        }

        /// <summary>
        /// Écrase le contenu du fichier avec les lignes fournies.
        /// Cette opération est synchrone.
        /// </summary>
        /// <param name="list">Liste de lignes à écrire dans le fichier</param>
        public void OverwriteFromList(List<string>? list)
        {
            using (var fs = new StreamWriter(_path, false)) // false = overwrite
            {
                foreach (var line in list)
                {
                    fs.WriteLineAsync(line);
                }
            }
        }

        public void AppendText(string text)
        {
            File.AppendText(text + Environment.NewLine);
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
