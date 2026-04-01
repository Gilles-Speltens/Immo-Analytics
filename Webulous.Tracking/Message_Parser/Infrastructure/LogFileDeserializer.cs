using Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Message_Parser.Infrastructure
{
    /// <summary>
    /// Classe responsable de la lecture d’un fichier de logs et de la
    /// désérialisation de chaque ligne en objet RequestLogDto.
    /// Les logs valides et invalides sont séparés.
    /// </summary>
    public class LogFileDeserializer
    {
        /// <summary>
        /// Lit un fichier ligne par ligne de manière asynchrone et tente
        /// de désérialiser chaque ligne en <see cref="RequestLogDto"/>.
        /// </summary>
        /// <param name="filePath">Chemin du fichier de logs.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - La liste des logs valides désérialisés
        /// - La liste des lignes invalides (non désérialisables)
        /// </returns>
        public async Task<(List<RequestLogDto> validLogs, List<string> invalidLogs)> DeserializeFileAsync(string filePath)
        {
            var validLogs = new List<RequestLogDto>();
            var invalidLogs = new List<string>();

            using var sr = new StreamReader(filePath);
            string? line;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                var log = Deserialize(line);
                if (log != null)
                    validLogs.Add(log);
                else
                    invalidLogs.Add(line);
            }

            return (validLogs, invalidLogs);
        }

        private RequestLogDto? Deserialize(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            try
            {
                return JsonSerializer.Deserialize<RequestLogDto>(line);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
