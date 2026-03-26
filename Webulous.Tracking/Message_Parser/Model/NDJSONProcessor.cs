using Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Message_Parser.Model
{
    internal class NDJSONProcessor
    {

        private readonly LogFileDeserializer _reader;
        private readonly LogWriter _writer;

        public NDJSONProcessor(LogFileDeserializer reader, LogWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

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

