using Common;
using Message_Parser.Data;
using Message_Parser.Services;
using Message_Parser.Infrastructure;
using Message_Parser;

var start = DateTime.Now;

var trackingDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs";
var archiveDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs\\Archive";
var invalidDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs\\Invalid";
var workingDir = "C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs\\Work";

MessageParserApp app = new MessageParserApp(trackingDir, archiveDir, invalidDir, workingDir, 20, "server=localhost;user=root;password=1234;database=AnalyticsDB;");

await app.InsertLogs();

var end = DateTime.Now;
Console.WriteLine(end - start);