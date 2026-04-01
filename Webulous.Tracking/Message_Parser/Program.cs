using Message_Parser;
using Microsoft.Extensions.Configuration;
using NLog;

var start = DateTime.Now;

var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

LogManager.Setup().LoadConfigurationFromFile(config["NLogConfigPath"]);
var logger = LogManager.GetCurrentClassLogger();


// Charger le fichier NLog.config
LogManager.Setup().LoadConfigurationFromFile(config["NLogConfigPath"]);

var trackingDir = config["trackingDirectory"];
var archiveDir = config["archiveDirectory"];
var invalidDir = config["invalidDirectory"];
var workingDir = config["workingDirectory"];
int sessionTime = int.Parse(config["sessionDuration"]);
var connection = config["DBConnection"];


MessageParserApp app = new MessageParserApp(trackingDir, archiveDir, invalidDir, workingDir, sessionTime, connection);

await app.InsertLogs();

logger.Warn("test");

var end = DateTime.Now;
Console.WriteLine(end - start);