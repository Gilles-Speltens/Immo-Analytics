using Message_Parser;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;

var start = DateTime.Now;

var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var nlogConfigPath = config["NLogConfigPath"];
LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);

Logger logger = LogManager.GetCurrentClassLogger();


logger.Warn("Application démarrée");

var trackingDir = config["trackingDirectory"];
var archiveDir = config["archiveDirectory"];
var invalidDir = config["invalidDirectory"];
var workingDir = config["workingDirectory"];
int sessionTime = int.Parse(config["sessionDuration"]);
var connection = config["DBConnection"];

MessageParserApp app = new MessageParserApp(trackingDir, archiveDir, invalidDir, workingDir, sessionTime, connection, logger);

await app.InsertLogs();

logger.Warn("Application terminée");

var end = DateTime.Now;
Console.WriteLine(end - start);