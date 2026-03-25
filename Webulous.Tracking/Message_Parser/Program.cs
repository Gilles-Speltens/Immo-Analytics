using Common;
using Dapper;
using Message_Parser.Entities;
using Message_Parser.Model;
using Message_Parser.Model.Reposiroties;

var deserilaizer = new NDJSONDeserializer("C:\\Users\\gille\\Desktop\\Stage Webulous\\Immo-Analytics\\Webulous.Tracking\\Logs");
List<RequestLogDto> logs = await deserilaizer.DeserializeAll();

//List<RequestLogDto> logs = new List<RequestLogDto>();

//for (int i = 0; i < 1000; i++)
//{
//    logs.Add(new RequestLogDto { Date =  DateTime.Now.AddMinutes(i),
//        UserId = "User"+i%10, 
//        UserIp = "192.168.1."+i%10, 
//        SessionId = "Session" + i%30, 
//        Url = "https://page/" + i%5, 
//        UrlReferrer = "https://page/" + (i-1)%5, 
//        Action = i%3 == 3 ? ActionsType.BUTTON_CLICK : ActionsType.HITPAGE, 
//        ActionParameters = i%3 != 3 ? null : "buttonParam" + i%3, 
//        LanguageBrowser = "fr", 
//        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:148.0) Gecko/20100101 Firefox/148.0"
//    });
//}

var start = DateTime.Now;

var uow = new UnitOfWork(20);

var result = await uow.bulkInsertLogs(logs);
Console.WriteLine(result);

var end = DateTime.Now;
Console.WriteLine(end - start);