using Common;
using Message_Parser.Data;
using Message_Parser.Infrastructure;
using Message_Parser.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser
{
    internal class MessageParserApp
    {
        private string _archiveDir;
        private string _invalidDir;
        private string _workingDir;

        private string[] _files;

        private UnitOfWork _unitOfWork;
        private FileProcessingService _processor;

        public MessageParserApp(string trackingDir, string archiveDir, string invalidDir, string workingDir, int sessionTime, string connection)
        {
            _archiveDir = archiveDir;
            _invalidDir = invalidDir;
            _workingDir = workingDir;

            _files = Directory.GetFiles(trackingDir);

            _unitOfWork = new UnitOfWork(sessionTime, connection);
            _processor = new FileProcessingService(_archiveDir, _invalidDir, _workingDir, 100);
        }

        public async Task InsertLogs()
        {
            for (int i = 0;  i < _files.Length-1; i++)
            {
                List<RequestLogDto> logs = await _processor.ProcessFileAsync(_files[i]);

                var sucess = await _unitOfWork.bulkInsertLogs(logs);
                
                if(sucess)
                {
                    _processor.MoveToArchive();
                } else
                {
                    Console.WriteLine("Error");
                    //Log error;
                    break;
                }
            }
        }
    }
}
