using POC_Analyse_1.Models.DTO;
using System.Text;
using System.Text.Json;

namespace POC_Analyse_1.Models
{
    public class LogManager
    {
        private readonly string directoryLogs;
        private string? currentFilePath;
        public LogManager(string path) {
            if(path == null) throw new ArgumentNullException("Path can't be null");
            if (!Directory.Exists(path)) throw new Exception("Path must be a directory");
            this.directoryLogs = path;
            this.currentFilePath = null;
        }

        public void addLogs(RequestLogDto log)
        {
            string jsonLine = JsonSerializer.Serialize(log);


            if(String.IsNullOrEmpty(currentFilePath))
            {
                this.currentFilePath = string.Concat(directoryLogs, $"Logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
            }

            //Ecire une ligne de log
            using (var writer = new StreamWriter(this.currentFilePath, append: true, Encoding.UTF8))
            {

                string Escape(string value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    if (value.Contains('"') || value.Contains(",") || value.Contains('\n'))
                    {
                        value = value.Replace("\"", "\"\"");
                        return $"\"{value}\"";
                    }
                    return value;
                }

                string line = string.Join(",",
                    Escape(log.UserAgent),
                    log.Date.ToString("o"),
                    Escape(log.CurrentPath),
                    Escape(log.UrlReferrer),
                    Escape(log.SessionId),
                    Escape(log.Action)
                );

                writer.WriteLine(line);
            }
        }
    }
}
