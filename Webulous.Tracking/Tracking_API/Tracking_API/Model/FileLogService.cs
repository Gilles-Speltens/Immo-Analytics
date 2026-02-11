using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tracking_API.Model.Dto;

namespace Tracking_API.Model
{
    /// <summary>
    /// Service responsable de la gestion des logs dans un fichier.
    /// Implémente un pattern Producer / Consumer :
    /// - Les requêtes HTTP ajoutent des logs dans une ConcurrentQueue.
    /// - Une tâche en arrière-plan vide périodiquement la queue et écrit dans un fichier.
    /// Gère également la rotation des fichiers selon un intervalle configuré.
    /// </summary>
    public class FileLogService
    {
        private string _path;

        private DateTime _logFileTimeStamp;

        private string _completePath;

        // En minutes
        private int _logFileRotationIntervalMinutes;

        //Une année max
        private int _maxMinutes = 525600;

        ConcurrentQueue<string> _logMessages = new ConcurrentQueue<string>();

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private PerformanceAnalyser _analyser;

        /// <summary>
        /// Initialise le service de log.
        /// Récupère la configuration et démarre la tâche background.
        /// </summary>
        /// <param name="configuration">Configuration de l'application.</param>
        /// <exception cref="ArgumentException">Si l'intervalle de rotation est invalide.</exception>
        public FileLogService(IConfiguration configuration) 
        {
            int rotationInterval = configuration.GetValue<int>("LogFileRotationIntervalMinutes");

            if (rotationInterval <= 0)
            {
                throw new ArgumentException("rotationInterval must be postif.");
            } else if (rotationInterval > _maxMinutes)
            {
                throw new ArgumentException("rotationInterval can't be greater than one year.");
            }
                
            this._logFileRotationIntervalMinutes = rotationInterval;
            this._path = "Logs/tracking-";
            this._logFileTimeStamp = DateTime.Now;
            this._completePath = String.Concat(_path, FilterCharacters(_logFileTimeStamp.ToString()));

            this._analyser = new PerformanceAnalyser();

            StartBackgroundWriter();
        }

        /// <summary>
        /// Ajoute un log dans la queue.
        /// Méthode appelée par les requêtes HTTP (Producteur).
        /// </summary>
        /// <param name="log">DTO contenant les informations de la requête.</param>
        public void AddEntryToQueue(RequestLogDto log)
        {
            string stringLog = Format(log);

            _logMessages.Enqueue(stringLog);
        }

        /// <summary>
        /// Lance une tâche (Consumer) en arrière-plan qui :
        /// - Vérifie la rotation du fichier
        /// - Vide la queue
        /// - Attend 1 seconde entre chaque cycle
        /// </summary>
        private void StartBackgroundWriter()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        DateTime time = DateTime.Now;
                        if (IsExpired(time)) RotateFile(time);
                        WriteLogsFromQueue();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    // 1 seconde de sleep.
                    await Task.Delay(1000, _cts.Token);
                }
            });
        }

        /// <summary>
        /// Vide la queue et écrit tous les logs dans le fichier courant.
        /// Mesure également le temps d'exécution pour analyse.
        /// </summary>
        private void WriteLogsFromQueue()
        {
            // Début Analyse
            int queueSize = _logMessages.Count;
            TimeOnly start = TimeOnly.FromDateTime(DateTime.Now);
            // Fin Analyse

            var _fs = File.AppendText(_completePath);
            while (_logMessages.TryDequeue(out var log))
            {
                _fs.Write(log);

                _analyser.IncreaseLogs();
            }

            _fs.Flush();
            _fs.Close();

            // Début Analyse
            TimeOnly end = TimeOnly.FromDateTime(DateTime.Now);
            TimeSpan elapsed = end - start;
            _analyser.AddQueue(queueSize, elapsed);
            // Fin Analyse
        }

        /// <summary>
        /// Crée un nouveau fichier de log lorsque l'intervalle est dépassé.
        /// </summary>
        /// <param name="time">Timestamp courant.</param>
        private void RotateFile(DateTime time)
        {
            _logFileTimeStamp = time;
            _completePath = String.Concat(_path, FilterCharacters(_logFileTimeStamp.ToString()));

            _analyser.NewFile();
        }

        private bool IsExpired(DateTime time)
        {
            return (time - _logFileTimeStamp).TotalMinutes >= _logFileRotationIntervalMinutes;
        }

        private string FilterCharacters(string date)
        {
            string d = Regex.Replace(date, @"\D", "");
            return d.Remove(d.Length - 2, 2);
        }

        private string Format(RequestLogDto log)
        {
            return $"{DateTime.UtcNow} - {log.UserId} - {log.Url} - {log.UrlReferrer} - {log.Action} - {log.LanguageBrowser} - {log.SessionId} - {log.UserAgent}\n";
        }
    }
}
