using Dapper;
using Message_Parser.Model;
using MySqlConnector;

//using var db = new MySqlConnection("server=localhost;user=root;password=1234;database=AnalyticsDB;");
//db.Open();

//Console.WriteLine(db.ToString());

var json = new NDJSONDeserializer("C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs");

var logs = await json.DeserializeAll();
