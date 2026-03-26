using Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Message_Parser.Model
{
    internal class LogFileDeserializer
    {
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
