using Dapper;
using Message_Parser.Entities;
using Message_Parser.Model;
using Message_Parser.Model.Reposiroties;
using MySqlConnector;

var db = DBConnection.Instance;
var sessionRepo = new SessionsRepository();
var userRepo = new UsersRepository();

var list = new List<Session>();

for (int i = 0; i < 10000; i++)
{
    list.Add(new Session { Id = i.ToString(), UserId = "a", Duration = DateTime.UtcNow });
}
 

//userRepo.Insert("a");
await sessionRepo.BulkInsert(list, null);

db.Execute("delete from sessions;");

//var json = new NDJSONDeserializer("C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs");

//var logs = await json.DeserializeAll();