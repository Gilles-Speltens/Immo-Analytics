using Common;
using Message_Parser.Data;
using Message_Parser.Services;
using NLog;

namespace Message_Parser
{
    public class MessageParserApp
    {
        private string _archiveDir;
        private string _invalidDir;
        private string _workingDir;

        private string[] _files;

        private UnitOfWork _unitOfWork;
        private FileProcessingService _processor;

        private Logger _logger;

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
                    
                    List<RequestLogDto> logs = await _processor.ProcessFileAsync(_files[i]);

                    var error = await _unitOfWork.bulkInsertLogs(logs);
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
