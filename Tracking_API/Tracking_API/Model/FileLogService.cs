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
        public FileLogService(int RotationInterval) 
        {
            if(RotationInterval <= 0)
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
        }

        public void addEntry(RequestLogDto log)
        {
            string stringLog = format(log);

            DateTime time = DateTime.Now;

            if(isExpired(time))
            {
                LogFileTimestamp = time;
                CompletePath = String.Concat(Path, filterCharacters(LogFileTimestamp.ToString()));

                fs.Close();
                fs = File.OpenWrite(this.CompletePath);
            }

            Byte[] info = new UTF8Encoding(true).GetBytes(stringLog);
            fs.Write(info, 0, info.Length);
            fs.Flush();
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
            return $"{log.Date} - {log.Url} - {log.UrlReferrer} - {log.Action} - {log.SessionId} - {log.UserAgent}\n";
        }
    }
}
