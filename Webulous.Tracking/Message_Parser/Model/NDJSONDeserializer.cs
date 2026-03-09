using Common;
using System.Text.Json;

namespace Message_Parser.Model
{
    public class NDJSONDeserializer
    {
        private readonly string _path;

        public NDJSONDeserializer(string path)
        {
            _path = path;
        }

        public async Task<List<RequestLogDto>> DeserializeAll()
        {
            var files = Directory.GetFiles(_path);
            var logs = new List<RequestLogDto>();

            for (int i = 0; i < files.Length-1; i++)
            {
                var file = files[i];

                logs.Concat(await DeserializeFile(file));     
            }
            return logs;
        }

        public async Task<List<RequestLogDto>> DeserializeFile(string path)
        {
            var logs = new List<RequestLogDto>();
            using (StreamReader sr = new StreamReader(path))
            {
                var line = await sr.ReadLineAsync();
                while (line != null && !line.Equals(""))
                {
                    var log = JsonSerializer.Deserialize<RequestLogDto>(line);
                    if (log != null)
                    {
                        logs.Add(log);
                    }
                    line = await sr.ReadLineAsync();
                }
            }
            return logs;
        }
    }
}

