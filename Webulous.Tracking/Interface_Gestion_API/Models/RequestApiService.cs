using System.Net;
using System.Text;
using System.Text.Json;

namespace Interface_Gestion_API.Models
{
    /// <summary>
    /// Service responsable des appels HTTP vers l'API distante.
    /// 
    /// Permet :
    /// - De récupérer des listes via requêtes GET
    /// - D'envoyer des données via POST
    /// - De vérifier la disponibilité de l'API
    /// 
    /// Utilise IHttpClientFactory pour la gestion du HttpClient.
    /// </summary>
    public class RequestApiService
    {
        private string _apiPath;
        private HttpClient _client;

        /// <summary>
        /// Initialise une nouvelle instance du service d'appel API.
        /// </summary>
        /// <param name="apiPath">URL de base de l'API.</param>
        /// <param name="factory">Factory permettant de créer un HttpClient.</param>

        public RequestApiService(string apiPath, IHttpClientFactory factory)
        {
            _apiPath = apiPath;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Effectue une requête GET vers un endpoint spécifique et retourne
        /// le code HTTP ainsi qu'une liste de chaînes désérialisée.
        /// </summary>
        /// <param name="path">Chemin relatif de l'endpoint.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - StatusCode : le code HTTP retourné par l'API
        /// - list : la liste désérialisée depuis la réponse JSON
        /// </returns>
        public async Task<(HttpStatusCode StatusCode, List<string> list)> GetList(string path)
        {
            var response = await _client.GetAsync(String.Concat($"{_apiPath}", path));

            return (response.StatusCode, await CreateList(response));
        }

        /// <summary>
        /// Vérifie si l'API est disponible via un endpoint de santé.
        /// </summary>
        /// <returns>
        /// <c>true</c> si l'API répond avec un code succès ; sinon <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Effectue une requête POST vers un endpoint spécifique avec un corps JSON.
        /// </summary>
        /// <param name="value">Valeur à sérialiser et envoyer dans le corps de la requête.</param>
        /// <param name="path">Chemin relatif de l'endpoint.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - Le code HTTP retourné par l'API
        /// - La liste de chaînes désérialisée depuis la réponse JSON
        /// </returns>
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
