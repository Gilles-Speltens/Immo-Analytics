using Common;
using Message_Parser.Infrastructure;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Message_Parser.Services
{
    /// <summary>
    /// Classe responsable du traitement des fichiers NDJSON.
    /// Elle lit les fichiers, désérialise les logs, archive les logs valides,
    /// écrit les logs invalides et supprime les fichiers traités.
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
        /// <param name="reader">Lecteur permettant de désérialiser les fichiers NDJSON.</param>
        /// <param name="writer">Writer permettant d’écrire les logs archivés et invalides.</param>
        public FileProcessingService(string archiveDirectory, string invalidDirectory, string workDirectory, long maxSizeKb)
        {
            _archiveDirectory = archiveDirectory;
            _invalidDirectory = invalidDirectory;
            _workDirectory = workDirectory;
            _reader = new LogFileDeserializer();
            _writer = new LogWriter(invalidDirectory, maxSizeKb);
        }

        public async Task<List<RequestLogDto>> ProcessAllFilesAsync(string[] files)
        {
            var allLogs = new List<RequestLogDto>();

            for (int i = 0; i < files.Length - 1; i++)
            {
                allLogs.AddRange(await ProcessFileAsync(files[i]));
            }

            return allLogs;
        }

        public async Task<List<RequestLogDto>> ProcessFileAsync(string file)
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

            return validLogs;
        }

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

