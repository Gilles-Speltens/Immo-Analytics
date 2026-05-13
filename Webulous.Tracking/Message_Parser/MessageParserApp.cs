using Message_Parser.Data;
using Message_Parser.Entities;
using Message_Parser.Services;
using NLog;

namespace Message_Parser
{
    /// <summary>
    /// Point d’entrée applicatif pour le traitement des fichiers de logs.
    ///
    /// Responsabilités :
    /// - Récupérer les fichiers à traiter
    /// - Orchestrer leur traitement via FileProcessingService
    /// - Insérer les logs en base de données via UnitOfWork
    /// - Gérer les erreurs et le logging
    /// - Déclencher l’archivage des fichiers traités
    /// </summary>
    public class MessageParserApp
    {
        private string _archiveDir;
        private string _invalidDir;
        private string _workingDir;

        private string[] _files;

        private UnitOfWork _unitOfWork;
        private FileProcessingService _processor;

        private Logger _logger;

        /// <summary>
        /// Initialise l'application de parsing des logs.
        /// </summary>
        /// <param name="trackingDir">Répertoire contenant les fichiers bruts à traiter.</param>
        /// <param name="archiveDir">Répertoire de destination pour les fichiers archivés.</param>
        /// <param name="invalidDir">Répertoire pour les logs invalides.</param>
        /// <param name="workingDir">Répertoire temporaire de traitement.</param>
        /// <param name="sessionTime">Durée de session utilisée côté base de données.</param>
        /// <param name="connection">Chaîne de connexion à la base de données.</param>
        /// <param name="logger">Logger utilisé pour tracer les événements et erreurs.</param>
        public MessageParserApp(string trackingDir, string archiveDir, string invalidDir, string workingDir, int sessionTime, string connection, Logger logger)
        {
            _archiveDir = archiveDir;
            _invalidDir = invalidDir;
            _workingDir = workingDir;

            _files = Directory.GetFiles(trackingDir);

            _unitOfWork = new UnitOfWork(sessionTime, connection);
            _processor = new FileProcessingService(_archiveDir, _invalidDir, _workingDir, 100);

            _logger = logger;
        }

        /// <summary>
        /// Lance le traitement et l’insertion des logs en base.
        ///
        /// Workflow :
        /// 1. Vérifie si le dossier de travail est vide
        /// 2. Traite chaque fichier (sauf le dernier, potentiellement en cours d’écriture)
        /// 3. Insère les logs en base
        /// 4. Archive les fichiers si succès
        /// 5. Stoppe le traitement en cas d’erreur
        /// </summary>

        public async Task InsertLogs()
        {
            if (Directory.GetFiles(_workingDir).Any())
            {
                _logger.Warn("Le dossier de travail '{workingDir}' n'est pas vide au démarrage. Des fichiers n'ont probablement pas été traités lors d'une exécution précédente.", _workingDir);
            }
            else
            {
                for (int i = 0; i < _files.Length - 1; i++)
                {
                    string filePath = _files[i];
                    string fileName = Path.GetFileName(filePath);

                    // Get current UTC time
                    DateTime timeUtc = DateTime.UtcNow;

                    // Find the target time zone for Belgium / CEST (UTC + 02:00 in summer)
                    TimeZoneInfo cestZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

                    // Convert UTC to the local CEST time
                    var date = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cestZone);

                    var monitoring = new FileMonitoring
                    {
                        FileName = fileName,
                        TreatementDate = date,
                        Speed = null,
                        Status = FileStatus.IN_PROCESS,
                    };
                    _unitOfWork.InsertIntoMonitoring(monitoring);

                    var result = await _processor.ProcessFileAsync(_files[i]);
                    List<RequestLogDto> logs = result.validLogs;
                    int nbInvalidLogs = result.invalidLogs;

                    var error = await _unitOfWork.bulkInsertLogs(logs, monitoring, nbInvalidLogs);
                    if (error == null)
                    {
                        _processor.MoveToArchive();
                    } else
                    {
                        _logger.Error("Erreur lors du traitement du fichier \"" + _files[i] + "\" | " + error);
                        break;
                    }
                }
            }
        }
    }
}
