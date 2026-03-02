using Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Interface_Gestion_API.Models
{
    public class RequestApiService
    {
        private string _apiPath;
        private HttpClient _client;

        public RequestApiService(string apiPath, IHttpClientFactory factory)
        {
            _apiPath = apiPath;
            _client = factory.CreateClient();
        }

        //public async Task<(HttpStatusCode StatusCode, WhiteListViewModel? WhiteList)> GetWhiteListAsync()
        //{
        //    var ipsTask = _client.GetAsync($"{_apiPath}/Admin/Ips");
        //    var domainsTask = _client.GetAsync($"{_apiPath}/Admin/Domains");

        //    await Task.WhenAll(ipsTask, domainsTask);

        //    var responseIps = await ipsTask;
        //    var responseDomains = await domainsTask;

        //    if (responseIps.StatusCode != HttpStatusCode.OK)
        //        return (responseIps.StatusCode, null);

        //    if (responseDomains.StatusCode != HttpStatusCode.OK)
        //        return (responseDomains.StatusCode, null);

        //    var whiteList = await CreateWhiteList(responseDomains, responseIps);

        //    return (HttpStatusCode.OK, whiteList);
        //}

        public async Task<(HttpStatusCode StatusCode, List<string> list)> GetList(string path)
        {
            var response = await _client.GetAsync(String.Concat($"{_apiPath}", path));

            return (response.StatusCode, await CreateList(response));
        }

        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_apiPath}/Admin/Health");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<(HttpStatusCode, List<string>)> LaunchPostAsync(string value, string path)
        {
            var fullPath = String.Concat(_apiPath, path);

            var json = JsonSerializer.Serialize(value);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(fullPath, content);

            return (response.StatusCode, await CreateList(response));
        }

        

        private async Task<List<string>> CreateList(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var stream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<List<string>>(stream);

            return result ?? new List<string>();
        }
    }
}
