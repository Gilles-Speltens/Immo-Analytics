using Message_Parser.Data;
using Message_Parser.Data.UserActions;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace Message_Parser.Infrastructure
{
    /// <summary>
    /// Classe responsable de la lecture d’un fichier de logs et de la
    /// désérialisation de chaque ligne en objet RequestLogDto.
    /// Les logs valides et invalides sont séparés.
    /// </summary>
    public class LogFileDeserializer
    {
        /// <summary>
        /// Lit un fichier ligne par ligne de manière asynchrone et tente
        /// de désérialiser chaque ligne en <see cref="RequestLogDto"/>.
        /// </summary>
        /// <param name="filePath">Chemin du fichier de logs.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - La liste des logs valides désérialisés
        /// - La liste des lignes invalides (non désérialisables)
        /// </returns>
        public async Task<(List<RequestLogDto> validLogs, List<string> invalidLogs)> DeserializeFileAsync(string filePath)
        {
            var validLogs = new List<RequestLogDto>();
            var invalidLogs = new List<string>();

            using var sr = new StreamReader(filePath);
            string? line;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                var log = Deserialize(line);
                if (log != null)
                    validLogs.Add(log);
                else
                    invalidLogs.Add(line);
            }

            return (validLogs, invalidLogs);
        }

        private RequestLogDto? Deserialize(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            try
            {
                var dto = new RequestLogDto();
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(line));

                var seenProps = new HashSet<string>();

                while (reader.Read())
                {
                    //Only if property
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propName = reader.GetString()!;

                        //If already in the HashSet
                        if (!seenProps.Add(propName)) return null;

                        //Move to value
                        reader.Read();
                        
                        switch (propName)
                        {
                            case "Date":
                                dto.Date = reader.GetDateTime();
                                break;

                            case "UserId":
                                dto.UserId = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                                break;

                            case "UserIp":
                                dto.UserIp = reader.GetString()!;
                                break;

                            case "SessionId":
                                dto.SessionId = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                                break;

                            case "Url":
                                dto.Url = reader.GetString()!;
                                break;

                            case "UrlReferrer":
                                dto.UrlReferrer = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                                break;

                            case "ActionType":
                                if (reader.TokenType == JsonTokenType.Null)
                                    return null;

                                int actionValue = reader.GetInt32();

                                // Check the Enum
                                if (!Enum.IsDefined(typeof(ActionsType), actionValue))
                                    return null;

                                dto.ActionType = (ActionsType)actionValue;
                                break;

                            case "ActionParameters":
                                if (reader.TokenType == JsonTokenType.Null)
                                {
                                    dto.ActionParameters = null;
                                    break;
                                }

                                using (var document = JsonDocument.ParseValue(ref reader))
                                {
                                    var rawJson = document.RootElement.GetRawText();

                                    dto.ActionParameters = dto.ActionType switch
                                    {
                                        ActionsType.CLIENT_ACTION =>
                                            JsonConvert.DeserializeObject<ClientActions>(rawJson),

                                        ActionsType.CONTACT_REQUEST =>
                                            JsonConvert.DeserializeObject<ContactRequest>(rawJson),

                                        ActionsType.ESTATE_SEARCH =>
                                            JsonConvert.DeserializeObject<EstateSearchs>(rawJson),

                                        _ => null
                                    };
                                }
                                break;

                            //case "ActionParameters":
                            //    if (reader.TokenType != JsonTokenType.Null)
                            //    {
                            //        using var doc = JsonDocument.ParseValue(ref reader);
                            //        string rawJson = doc.RootElement.GetRawText();
                            //        dto.ActionParameters = JsonConvert.DeserializeObject<UserActionsBase>(
                            //            rawJson,
                            //            new JsonSerializerSettings //Pour convertir automatiquement le UserActionBase en une de ses sous classe.
                            //            {
                            //                TypeNameHandling = TypeNameHandling.Auto
                            //            });
                            //    }
                            //    break;

                            case "LanguageBrowser":
                                dto.LanguageBrowser = reader.GetString()!;
                                break;

                            case "UserAgent":
                                dto.UserAgent = reader.GetString()!;
                                break;

                            default:
                                return null;
                        }
                    }
                }

                if (dto.Date == default || dto.UserIp == null || dto.Url == null || dto.LanguageBrowser == null || dto.UserAgent == null)
                    return null;

                return dto;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }
    }
}
