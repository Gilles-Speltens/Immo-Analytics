using Common;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Message_Parser.Model
{
    /// <summary>
    /// Classe responsable de l’écriture des logs :
    /// - logs valides dans des fichiers d’archive
    /// - logs invalides dans des fichiers séparés avec gestion de taille
    /// </summary>
    internal class LogWriter
    {
        private readonly string _archiveDirectory;
        private readonly string _invalidDirectory;
        private readonly long _maxSizeKb;
        private int _currentId;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="archiveDirectory">Répertoire de stockage des logs archivés.</param>
        /// <param name="invalidDirectory">Répertoire de stockage des logs invalides.</param>
        /// <param name="maxSizeKb">Taille maximale d’un fichier de logs invalides (en Ko).</param>
        public LogWriter(string archiveDirectory, string invalidDirectory, long maxSizeKb)
        {
            _invalidDirectory = invalidDirectory;
            _maxSizeKb = maxSizeKb;
            _currentId = GetMaxExistingId();
            _archiveDirectory = archiveDirectory;
        }

        /// <summary>
        /// Écrit les logs valides dans un fichier d’archive.
        /// Le nom du fichier est basé sur la date extraite du nom du fichier source.
        /// </summary>
        /// <param name="oldFileName">Nom du fichier source.</param>
        /// <param name="logs">Liste des logs valides.</param>
        public async Task WriteArchiveLogAsync(string oldFileName, List<RequestLogDto> logs)
        {
            if (!logs.Any()) return;

            var fileName = Path.GetFileNameWithoutExtension(oldFileName);
            var datePart = Regex.Match(fileName, @"\d+").Value;
            var archivePath = Path.Combine(_archiveDirectory, $"archive-{datePart}.log");

            List<string> stringLogs = ConvertLogs(logs);

            await File.WriteAllLinesAsync(archivePath, stringLogs);
        }

        /// <summary>
        /// Écrit les logs invalides dans un fichier.
        /// Si la taille maximale est atteinte, un nouveau fichier est créé.
        /// </summary>
        /// <param name="invalidLogs">Liste des lignes invalides.</param>
        public async Task WriteInvalidLogsAsync(List<string> invalidLogs)
        {
            if (!invalidLogs.Any()) return;

            var path = Path.Combine(_invalidDirectory, $"invalid-{_currentId}.log");

            if (File.Exists(path) && new FileInfo(path).Length / 1024 >= _maxSizeKb)
            {
                _currentId++;
                path = Path.Combine(_invalidDirectory, $"invalid-{_currentId}.log");
            }

            await File.AppendAllLinesAsync(path, invalidLogs);
        }

        private List<string> ConvertLogs(List<RequestLogDto> logs)
        {
            var stringLogs = new List<string>();

            foreach ( var log in logs )
            {
                stringLogs.Add($"[{log.Date}] {log.UserIp} | {log.Action.ToString()} | {log.Url} | {log.UrlReferrer ?? "-"} | {log.UserId ?? "-"} | {log.SessionId} | {log.ActionParameters ?? "-"} | {log.LanguageBrowser} | {log.UserAgent}");
            }

            return stringLogs;
        }

        private int GetMaxExistingId()
        {
            var files = Directory.GetFiles(_invalidDirectory, "invalid-*.log");
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
