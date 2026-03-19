using Dapper;
using Message_Parser.Entities;
using Message_Parser.Model;
using Message_Parser.Model.Reposiroties;

var db = DBConnection.Instance;
var sessionRepo = new SessionsRepository(db);
var hitpageRepo = new HitPageRepository(db);
var userActions = new UserActionsRepository(db);
var siteRepo = new SiteRepository(db);

var start = DateTime.Now;
var list = new List<Session>();

for (int i = 0; i < 10000; i++)
{
    list.Add(new Session { SessionId = string.Concat("session:", i), SiteId = 1, UserId = string.Concat("User:", i), UserIp = i, LanguageBrowser = "fr", UserAgent = "azroieroaiuzerioua", SessionEnd = DateTime.Now });
}
await sessionRepo.BulkUpsert(list, null);

var end = DateTime.Now;
Console.WriteLine(end - start);