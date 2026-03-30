using Common;
using Dapper;
using Message_Parser.Entities;
using Message_Parser.Model;
using Message_Parser.Model.Reposiroties;

var start = DateTime.Now;

var trackingDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs";
var archiveDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs\\Test";
var invalidDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs\\Test";
var processor = new NDJSONProcessor(new LogFileDeserializer(), new LogWriter(archiveDir, invalidDir, 100));

var files = Directory.GetFiles(trackingDir);
List<RequestLogDto> logs = await processor.ProcessAllFilesAsync(files);

var uow = new UnitOfWork(20, "server=localhost;user=root;password=1234;database=AnalyticsDB;");

var result = await uow.bulkInsertLogs(logs);
Console.WriteLine(result);

var end = DateTime.Now;
Console.WriteLine(end - start);