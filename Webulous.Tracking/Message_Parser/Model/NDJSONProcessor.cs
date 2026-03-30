using Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Message_Parser.Model
{
    /// <summary>
    /// Classe responsable du traitement des fichiers NDJSON.
    /// Elle lit les fichiers, désérialise les logs, archive les logs valides,
    /// écrit les logs invalides et supprime les fichiers traités.
    /// </summary>
    internal class NDJSONProcessor
    {

        private readonly LogFileDeserializer _reader;
        private readonly LogWriter _writer;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="NDJSONProcessor"/>.
        /// </summary>
        /// <param name="reader">Lecteur permettant de désérialiser les fichiers NDJSON.</param>
        /// <param name="writer">Writer permettant d’écrire les logs archivés et invalides.</param>
        public NDJSONProcessor(LogFileDeserializer reader, LogWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        /// <summary>
        /// Traite tous les fichiers NDJSON fournis.
        /// Pour chaque fichier :
        /// - Désérialise les logs
        /// - Archive les logs valides
        /// - Écrit les logs invalides
        /// - Supprime le fichier traité
        /// </summary>
        /// <param name="files">Liste des chemins des fichiers à traiter.</param>
        /// <returns>Liste de tous les logs valides provenant de tous les fichiers.</returns>
        public async Task<List<RequestLogDto>> ProcessAllFilesAsync(string[] files)
        {
            var allLogs = new List<RequestLogDto>();

            for (int i = 0; i < files.Length - 1; i++)
            {
                if(File.Exists(files[i]))
                {
                    var (validLogs, invalidLogs) = await _reader.DeserializeFileAsync(files[i]);
                    allLogs.AddRange(validLogs);

                    await _writer.WriteArchiveLogAsync(files[i], validLogs);
                    await _writer.WriteInvalidLogsAsync(invalidLogs);

                    //File.Delete(files[i]);
                }
            }

            return allLogs;
        }

        
    }
}

