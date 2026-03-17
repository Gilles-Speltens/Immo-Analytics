using Dapper;
using Message_Parser.Entities;
using Message_Parser.Model;
using Message_Parser.Model.Reposiroties;

var db = DBConnection.Instance;
var sessionRepo = new SessionsRepository();
var userRepo = new UsersRepository();
var hitpageRepo = new HitPageRepository();
var userActions = new UserActionsRepository();
var websiteRepo = new WebsiteRepository();

var list = new List<Website>();

for (int i = 0; i < 10000; i++)
{
    list.Add(new Website { Domain = i.ToString(), DateWhenAdded = DateTime.UtcNow, Certify = true});
}

//await userActions.BulkInsert(list, null);

//db.Execute("delete from Hit_Page;");

//var json = new NDJSONDeserializer("C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs");

//var logs = await json.DeserializeAll();