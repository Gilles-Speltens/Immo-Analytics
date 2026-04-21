using Common;
using Message_Parser.Infrastructure;
using System.Text.RegularExpressions;

namespace Message_Parser.Services
{
    /// <summary>
    /// Service responsable du traitement des fichiers NDJSON contenant des logs.
    /// 
    /// Fonctionnalités principales :
    /// - Lecture et désérialisation des fichiers de logs
    /// - Séparation des logs valides et invalides
    /// - Écriture des logs invalides dans un répertoire dédié
    /// - Archivage ou déplacement des fichiers traités
    /// - Suppression des fichiers après traitement si nécessaire
    /// </summary>
    public class FileProcessingService
    {
        private readonly string _archiveDirectory;
        private readonly string _invalidDirectory;
        private readonly string _workDirectory;
        private readonly LogFileDeserializer _reader;
        private readonly LogWriter _writer;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="FileProcessingService"/>.
        /// </summary>
        /// <param name="archiveDirectory">Répertoire de destination pour les fichiers archivés.</param>
        /// <param name="invalidDirectory">Répertoire contenant les logs invalides.</param>
        /// <param name="workDirectory">Répertoire de travail pour les fichiers en cours de traitement.</param>
        /// <param name="maxSizeKb">Taille maximale des fichiers de logs invalides.</param>
        public FileProcessingService(string archiveDirectory, string invalidDirectory, string workDirectory, long maxSizeKb)
        {
            _archiveDirectory = archiveDirectory;
            _invalidDirectory = invalidDirectory;
            _workDirectory = workDirectory;
            _reader = new LogFileDeserializer();
            _writer = new LogWriter(invalidDirectory, maxSizeKb);
        }

        /// <summary>
        /// Traite un fichier NDJSON :
        /// - Désérialise les logs
        /// - Sépare logs valides et invalides
        /// - Écrit les logs invalides
        /// - Déplace ou supprime le fichier selon le cas
        /// </summary>
        /// <param name="file">Chemin du fichier à traiter.</param>
        /// <returns>Liste des logs valides extraits du fichier.</returns>
        public async Task<(List<RequestLogDto> validLogs, int invalidLogs)> ProcessFileAsync(string file)
        {
            var validLogs = new List<RequestLogDto>();
            List<string> invalidLogs = new List<string>();
            if (File.Exists(file))
            {
                (validLogs, invalidLogs) = await _reader.DeserializeFileAsync(file);
            }

            if (invalidLogs.Any())
            {
                await _writer.WriteInvalidLogsAsync(invalidLogs, _invalidDirectory);
                await _writer.WriteProcessingLogAsync(file, _workDirectory, validLogs);
                File.Delete(file);
            } else
            {
                MoveFile(file, _workDirectory, "processing-");
            }

            return (validLogs, invalidLogs.Count);
        }

        /// <summary>
        /// Déplace tous les fichiers du répertoire de travail vers le répertoire d’archive.
        /// </summary>
        public void MoveToArchive()
        {
            var files = Directory.GetFiles(_workDirectory);
            foreach (var file in files)
            {
                MoveFile(file, _archiveDirectory, "archive-");
            }
        }

        private void MoveFile(string oldFile, string path, string name)
        {
            string oldFileName = Path.GetFileName(oldFile);
            string date = Regex.Match(oldFileName, @"\d+").Value;

            string newFileName = String.Concat(name, date, ".log");
            string newFile = Path.Combine(path,newFileName);

            File.Move(oldFile, newFile);
        }
    }
}

