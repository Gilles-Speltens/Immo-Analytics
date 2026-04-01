using Common;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Message_Parser.Infrastructure
{
    /// <summary>
    /// Classe responsable de l’écriture des logs :
    /// - logs valides dans des fichiers d’archive
    /// - logs invalides dans des fichiers séparés avec gestion de taille
    /// </summary>
    public class LogWriter
    {
        private readonly long _maxSizeKb;
        private int _currentId;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="archiveDirectory">Répertoire de stockage des logs archivés.</param>
        /// <param name="invalidDirectory">Répertoire de stockage des logs invalides.</param>
        /// <param name="maxSizeKb">Taille maximale d’un fichier de logs invalides (en Ko).</param>
        public LogWriter(string invalidDirectory, long maxSizeKb)
        {
            _maxSizeKb = maxSizeKb;
            _currentId = GetMaxExistingId(invalidDirectory);
        }

        /// <summary>
        /// Écrit les logs invalides dans un fichier.
        /// Si la taille maximale est atteinte, un nouveau fichier est créé.
        /// </summary>
        /// <param name="invalidLogs">Liste des lignes invalides.</param>
        public async Task WriteInvalidLogsAsync(List<string> invalidLogs, string invalidDirectory)
        {
            if (!invalidLogs.Any()) return;

            var path = Path.Combine(invalidDirectory, $"invalid-{_currentId}.log");

            if (File.Exists(path) && new FileInfo(path).Length / 1024 >= _maxSizeKb)
            {
                _currentId++;
                path = Path.Combine(invalidDirectory, $"invalid-{_currentId}.log");
            }

            await File.AppendAllLinesAsync(path, invalidLogs);
        }

        public async Task WriteProcessingLogAsync(string oldFileName, string workingDirectory, List<RequestLogDto> logs)
        {
            if (!logs.Any()) return;

            var fileName = Path.GetFileNameWithoutExtension(oldFileName);
            var datePart = Regex.Match(fileName, @"\d+").Value;
            var workingPath = Path.Combine(workingDirectory, $"processing-{datePart}.log");

            List<string> stringLogs = ConvertLogs(logs);

            await File.WriteAllLinesAsync(workingPath, stringLogs);
        }

        private List<string> ConvertLogs(List<RequestLogDto> logs)
        {
            var stringLogs = new List<string>();

            foreach ( var log in logs )
            {
                //stringLogs.Add($"[{log.Date}] {log.UserIp} | {log.Action.ToString()} | {log.Url} | {log.UrlReferrer ?? "-"} | {log.UserId ?? "-"} | {log.SessionId} | {log.ActionParameters ?? "-"} | {log.LanguageBrowser} | {log.UserAgent}");
                var serialized = JsonSerializer.Serialize(log);
                stringLogs.Add(serialized);
            }

            return stringLogs;
        }

        private int GetMaxExistingId(string invalidDirectory)
        {
            var files = Directory.GetFiles(invalidDirectory, "invalid-*.log");
            int maxId = 0;

            foreach (var file in files)
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"\d+");
                if (int.TryParse(match.Value, out int id))
                    maxId = Math.Max(maxId, id);
            }

            return maxId == 0 ? 1 : maxId;
        }
    }
}
