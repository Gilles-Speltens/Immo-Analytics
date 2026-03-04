using Common;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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

        private FileManager _fileManager;

        // En minutes
        private int _logFileRotationIntervalMinutes;

        //Une année max
        private int _maxMinutes = 525600;

        ConcurrentQueue<byte[]> _logMessages = new ConcurrentQueue<byte[]>();

        private CancellationTokenSource _cts = new CancellationTokenSource();

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
            this._path = string.Concat(configuration["PathToLogsDirectory"], "/ tracking-");
            this._logFileTimeStamp = DateTime.Now;
            this._fileManager = new FileManager(String.Concat(_path, FilterCharacters(_logFileTimeStamp.ToString("yyyyMMddHHmmss")), ".log"));

            StartBackgroundWriter();
        }

        /// <summary>
        /// Ajoute un log dans la queue.
        /// Méthode appelée par les requêtes HTTP (Producteur).
        /// </summary>
        /// <param name="log">DTO contenant les informations de la requête.</param>
        public async Task AddEntryToQueue(Stream body)
        {
            using var ms = new MemoryStream();
            await body.CopyToAsync(ms);

            _logMessages.Enqueue(ms.ToArray());
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
                    DateTime time = DateTime.Now;
                    if (IsExpired(time)) RotateFile(time);
                    if (!_logMessages.IsEmpty) await _fileManager.AppendFromQueue(_logMessages);

                    // 1 seconde de sleep.
                    await Task.Delay(1000, _cts.Token);
                }
            });
        }

        /// <summary>
        /// Crée un nouveau fichier de log lorsque l'intervalle est dépassé.
        /// </summary>
        /// <param name="time">Timestamp courant.</param>
        private void RotateFile(DateTime time)
        {
            _logFileTimeStamp = time;

            _fileManager.ChangePath(String.Concat(_path, FilterCharacters(_logFileTimeStamp.ToString("yyyyMMddHHmmss")), ".log"));
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
    }
}
