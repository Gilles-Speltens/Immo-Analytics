using Message_Parser.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Message_Parser.Infrastructure
{
    /// <summary>
    /// Fournit des fonctionnalités pour l'écriture et la gestion des fichiers de logs.
    /// </summary>
    /// <remarks>
    /// La classe <see cref="LogWriter"/> permet :
    /// <list type="bullet">
    /// <item>
    /// <description>D'écrire les logs invalides dans des fichiers avec un mécanisme de rotation basé sur la taille.</description>
    /// </item>
    /// <item>
    /// <description>D'écrire les logs de traitement dans des fichiers dédiés, au format JSON.</description>
    /// </item>
    /// </list>
    /// Les fichiers de logs invalides sont nommés selon le format <c>invalid-{id}.log</c>,
    /// où <c>id</c> est incrémenté lorsque la taille maximale du fichier est atteinte.
    /// Les fichiers de logs de traitement sont nommés selon le format
    /// <c>processing-{date}.log</c>, où <c>date</c> est extraite du nom du fichier source.
    /// </remarks>
    public class LogWriter
    {
        private readonly long _maxSizeKb;
        private int _currentId;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="invalidDirectory">Répertoire de stockage des logs invalides.</param>
        /// <param name="maxSizeKb">Taille maximale d’un fichier de logs invalides (en Ko).</param>
        public LogWriter(string invalidDirectory, long maxSizeKb)
        {
            _maxSizeKb = maxSizeKb;
            _currentId = GetMaxExistingId(invalidDirectory);
        }

        /// <summary>
        /// Écrit une collection de logs invalides dans un fichier.
        /// Si la taille maximale du fichier courant est atteinte, un nouveau fichier est créé.
        /// </summary>
        /// <param name="invalidLogs">Liste des lignes de logs invalides à écrire.</param>
        /// <param name="invalidDirectory">Répertoire de stockage des fichiers de logs invalides.</param>
        /// <returns>Une tâche représentant l'opération asynchrone.</returns>
        /// <remarks>
        /// Les fichiers sont nommés selon le format <c>invalid-{id}.log</c>.
        /// L'écriture est effectuée en mode ajout afin de préserver les données existantes.
        /// </remarks>
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

        /// <summary>
        /// Écrit les logs de traitement dans un fichier dédié au format JSON.
        /// </summary>
        /// <param name="oldFileName">
        /// Nom du fichier source à partir duquel la date est extraite pour nommer le fichier de sortie.
        /// </param>
        /// <param name="workingDirectory">
        /// Répertoire de stockage des fichiers de logs de traitement.
        /// </param>
        /// <param name="logs">
        /// Liste des objets <see cref="RequestLogDto"/> représentant les logs de traitement.
        /// </param>
        /// <returns>Une tâche représentant l'opération asynchrone.</returns>
        /// <remarks>
        /// Le fichier est nommé selon le format <c>processing-{date}.log</c>, où la date est
        /// extraite du nom du fichier source à l'aide d'une expression régulière.
        /// Chaque entrée est sérialisée au format JSON pour faciliter l'analyse ultérieure.
        /// </remarks>
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
