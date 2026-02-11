using System.Text;
using System.Text.Json;

var client  = new HttpClient();

var uri = new Uri("https://localhost:7042/TrackingDatas");

var tasks = new List<Task>();

//Requêtes evoyées les une à la suite des autres
/*for (int i = 0; i < 100000; i++)
{
    var log = new
    {
        UserId = i.ToString(),
        Url = "/test/1",
        UrlReferrer = "/test",
        Action = "test",
        LanguageBrowser = "fr",
        SessionId = "4321",
        UserAgent = "Google"
    };

    var json = JsonSerializer.Serialize(log);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    tasks.Add(client.PostAsync(uri, content));
}*/

//Requêtes envoyée en même temps
await Parallel.ForEachAsync(
    Enumerable.Range(0, 1000000),
    async (i, _) =>
    {
        var log = new
        {
            UserId = i.ToString(),
            Url = "/test/1",
            UrlReferrer = "/test",
            Action = "test",
            LanguageBrowser = "fr",
            SessionId = "4321",
            UserAgent = "Google"
        };

        var json = JsonSerializer.Serialize(log);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await client.PostAsync(uri, content);
    });

await Task.WhenAll(tasks);

Console.WriteLine("Toutes les requêtes envoyées !");
