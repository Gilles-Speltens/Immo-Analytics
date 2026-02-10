using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Tracking_API.Model.Dto;

namespace Tracking_API.Model
{
    public class FileLogService
    {
        private string Path;

        private DateTime LogFileTimestamp;

        private string CompletePath;

        // En minutes
        private int LogFileRotationIntervalMinutes;

        //Une année max
        private int MaxMinutes = 525600;

        private FileStream fs;

        ConcurrentQueue<string> _logMessages = new ConcurrentQueue<string>();

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly UTF8Encoding _utf8 = new UTF8Encoding(true);


        public FileLogService(IConfiguration configuration) 
        {
            int RotationInterval = configuration.GetValue<int>("LogFileRotationIntervalMinutes");

            if (RotationInterval <= 0)
            {
                throw new ArgumentException("RotationInterval must be postif.");
            } else if (RotationInterval > MaxMinutes)
            {
                throw new ArgumentException("RotationInterval can't be greater than one year.");
            }
                
            this.LogFileRotationIntervalMinutes = RotationInterval;
            this.Path = "Logs/tracking-";
            this.LogFileTimestamp = DateTime.Now;
            this.CompletePath = String.Concat(Path, filterCharacters(LogFileTimestamp.ToString()));
            this.fs = File.OpenWrite(this.CompletePath);

            StartBackgroundWriter();
        }

        public void addEntryToQueue(RequestLogDto log)
        {
            string stringLog = format(log);

            _logMessages.Enqueue(stringLog);
        }

        private void StartBackgroundWriter()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        DateTime time = DateTime.Now;
                        if (isExpired(time)) RotateFile(time);
                        writeLogsFromQueue();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    // 1 seconde de sleep.
                    await Task.Delay(1000);
                }
            });
        }

        private void writeLogsFromQueue()
        {
            while (_logMessages.TryDequeue(out var log))
            {
                var bytes = _utf8.GetBytes(log);
                fs.Write(bytes, 0, bytes.Length);
            }

            fs.Flush();
        }

        private void RotateFile(DateTime time)
        {
            LogFileTimestamp = time;
            CompletePath = String.Concat(Path, filterCharacters(LogFileTimestamp.ToString()));

            fs.Close();
            fs = File.OpenWrite(this.CompletePath);
        }

        private bool isExpired(DateTime time)
        {
            int NewTime = convertToMinutes(time);
            int OldTime = convertToMinutes(LogFileTimestamp);

            if (NewTime < OldTime)
            {
                if ((MaxMinutes - OldTime) + NewTime >= LogFileRotationIntervalMinutes)
                {
                    return true;
                }
            } 
            else if (NewTime - OldTime >= LogFileRotationIntervalMinutes)
            {
                return true;
            }
            
            return false;
        }

        private int convertToMinutes(DateTime time)
        {
            int minutes = time.Minute;
            minutes = minutes + time.Hour * 60;
            minutes = minutes + time.Day * 1440;

            return minutes;
        }

        private string filterCharacters(string date)
        {
            string d = Regex.Replace(date, @"\D", "");
            return d.Remove(d.Length - 2, 2);
        }

        private string format(RequestLogDto log)
        {
            return $"{DateTime.UtcNow} - {log.UserId} - {log.Url} - {log.UrlReferrer} - {log.Action} - {log.LanguageBrowser} - {log.SessionId} - {log.UserAgent}\n";
        }
    }
}
