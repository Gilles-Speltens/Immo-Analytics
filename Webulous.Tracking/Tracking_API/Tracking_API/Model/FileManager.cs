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
        private byte[] _newLine = Encoding.UTF8.GetBytes("\n");

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

            _path = newPath;
        }

        /// <summary>
        /// Vide la ConcurrentQueue et écrit son contenu à la fin du fichier courant.
        /// 
        /// - Chaque élément de la queue est un tableau de bytes représentant une ligne JSON.
        /// - Les lignes sont écrites en UTF8 telles quelles (aucune conversion string).
        /// - Une nouvelle ligne (\n) est ajoutée après chaque entrée.
        /// - Le fichier est ouvert uniquement le temps du flush puis refermé.
        /// </summary>
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
                await fs.WriteAsync(line);
                await fs.WriteAsync(_newLine);
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
        /// <returns>Tableau de chaînes contenant toutes les lignes du fichier ou un tableau vide si le fichier n'existe pas</returns>
        public string[] ReadFile()
        {
            if(File.Exists(_path))
            {
                return File.ReadAllLines(_path);
            } else
            {
                return new string[0];
            }
        }
    }
}
