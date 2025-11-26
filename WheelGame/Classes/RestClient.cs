using WheelGame.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Security.Policy;
using HuntpointApp.Models;

namespace WheelGame.Classes
{
    public class RestClient
    {
        private HttpClient httpClient;
        private HttpMessageHandler httpMessageHandler;

        public static string baseUri = $"{App.TimerSocketScheme}://{App.TimerSocketAddress}/";                
        public RestClient()
        {
            httpMessageHandler = new HttpClientHandler()
            {
                CookieContainer = new System.Net.CookieContainer(),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }

            };

            httpClient = new HttpClient(httpMessageHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<RestResponse<IEnumerable<WheelAppPlayer>>> GetAvailablePlayersAsync(string playerId)
            => await GetFromAPI<IEnumerable<WheelAppPlayer>>($"wheel/players/available/{playerId}");

        public async Task<RestResponse<IEnumerable<WheelAppPlayer>>> GetAllPlayersAsync(string playerId)
            => await GetFromAPI<IEnumerable<WheelAppPlayer>>($"wheel/players/all/{playerId}");


        public async Task<RestResponse<WheelAppPlayer>> GetPlayerInfoAsync(string playerId)
            => await GetFromAPI<WheelAppPlayer>($"wheel/players/info/{playerId}");
        
        public async Task<RestResponse<RoomConnectionInfo>> GetRoomInfo(string roomId)
            => await GetFromAPI<RoomConnectionInfo>($"wheel/room/info/{roomId}");
        
        public async Task<RestResponse<Room>> ConnectToRoom(ConnectRoomModel model)
        {
            try
            {
                var url = $"wheel/room/connect";
                var json = JsonConvert.SerializeObject(model);
                var response = await PostAsync(url, json, CancellationToken.None, false);
                if (response?.IsSuccessStatusCode == true)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content == "notfound")
                    {
                        return new RestResponse<Room>() { ErrorMessage = "Not Found", IsSuccess = false };
                    }
                    else if (content == "wrongpassword")
                    {
                        return new RestResponse<Room>() { ErrorMessage = "Wrong Password", IsSuccess = false };
                    }
                    {
                        var res = new RestResponse<Room>()
                        {
                            Data = JsonConvert.DeserializeObject<Room>(content),
                            IsSuccess = true
                        };

                        return res;
                    }
                }
                return new RestResponse<Room>() { IsSuccess = false, ErrorMessage = response.ReasonPhrase };
            }
            catch (Exception ex)
            {
                return new RestResponse<Room>() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<RestResponse<string>> UpdateUserInfoAsync(WheelAppPlayer player)
        {
            var json = JsonConvert.SerializeObject(player);
            var response = await PostAsync($"wheel/players/signup", json, CancellationToken.None, false);
            if (response?.IsSuccessStatusCode == true)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "success")
                {
                    return new RestResponse<string>() { IsSuccess = true };
                }

                return new RestResponse<string>() { IsSuccess = false, ErrorMessage = content };
            }
            return new RestResponse<string>() { IsSuccess = false, ErrorMessage = response?.ReasonPhrase ?? "No connection to server" };
        }

        public async Task<RestResponse<string>> Ping()
        {
            var playerId = WheelGame.Properties.Settings.Default.PlayerId ?? "";
            var json = JsonConvert.SerializeObject(new { playerId = playerId });
            var response = await PostAsync($"wheel/ping/{playerId}", json, CancellationToken.None, false);
            if (response?.IsSuccessStatusCode == true)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "pong")
                {
                    return new RestResponse<string>() { IsSuccess = true };
                }

                return new RestResponse<string>() { IsSuccess = false, ErrorMessage = content };
            }
            return new RestResponse<string>() { IsSuccess = false, ErrorMessage = response?.ReasonPhrase ?? "No connection to server" };
        }

        public async Task<RestResponse<Room>> CreateRoomAsync(NewRoomModel newBoard)
        {
            var json = JsonConvert.SerializeObject(newBoard);
            var response = await PostAsync($"wheel/room/create", json, CancellationToken.None, false);
            if (response?.IsSuccessStatusCode == true)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "error")
                {
                    return new RestResponse<Room>() { IsSuccess = false, ErrorMessage = "Error with room creation!" };
                }
                var newroom = JsonConvert.DeserializeObject<Room>(content);
                if (newroom != null)
                {
                    return new RestResponse<Room>() { IsSuccess = true, Data = newroom };
                }
                else
                    return new RestResponse<Room>() { IsSuccess = false, ErrorMessage = "Error with room creation!" };
            }
            return new RestResponse<Room>() { IsSuccess = false, ErrorMessage = response?.ReasonPhrase ?? "No connection to server" };

        }

        #region PrivateMethods

        private async Task<HttpResponseMessage?> PostAsync(string uri, CancellationToken _token, bool isAuthenticated = true)
        {
            try
            {
                if (isAuthenticated == false)
                {
                    return await httpClient.PostAsync(baseUri + uri,
                        new StringContent("", Encoding.UTF8, "application/json"), _token);
                }
                else
                {
                    var message = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri(baseUri + uri) };
                    try
                    {
                        var response = await httpClient.SendAsync(message, _token);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                    return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.NotAcceptable };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<HttpResponseMessage?> PostAsync(string uri, JObject jobj, CancellationToken _token, bool isAuthenticated = true)
        {
            try
            {
                if (isAuthenticated == false)
                {
                    return await httpClient.PostAsync(baseUri + uri,
                        new StringContent(jobj?.ToString() ?? "", Encoding.UTF8, "application/json"), _token);
                }
                else
                {
                    var message = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri(baseUri + uri) };
                    if (jobj != null)
                    {
                        message.Content = new StringContent(jobj?.ToString(), Encoding.UTF8, "application/json");
                    }
                    return await httpClient.SendAsync(message, _token);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<HttpResponseMessage?> PostAsync(string uri, string json, CancellationToken _token, bool isAuthenticated = true)
        {
            try
            {
                if (isAuthenticated == false)
                {
                    var _uri = baseUri + uri;
                    return await httpClient.PostAsync(_uri,
                        new StringContent(json, Encoding.UTF8, "application/json"), _token);
                }
                else
                {
                    var message = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri(baseUri + uri) };
                    if (!String.IsNullOrEmpty(json))
                    {
                        message.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                    return await httpClient.SendAsync(message, _token);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<HttpResponseMessage> PutAsync(string uri, int id, string json, CancellationToken _token)
        {

            var message = new HttpRequestMessage() { Method = HttpMethod.Put, RequestUri = new Uri(baseUri + uri + $"?id={id}") };
            if (!String.IsNullOrEmpty(json))
            {
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return await httpClient.SendAsync(message, _token);
        }

        private async Task<HttpResponseMessage> DeleteAsync(string uri, int id, CancellationToken _token)
        {

            var message = new HttpRequestMessage() { Method = HttpMethod.Delete, RequestUri = new Uri(baseUri + uri + $"/{id}") };
            return await httpClient.SendAsync(message, _token);
        }

        private async Task<HttpResponseMessage> GetAsync(string uri, bool IsAuthenticated = true)
        {
            var message = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri(baseUri + uri) };

            return await httpClient.SendAsync(message);
        }

        private async Task<RestResponse<T>> GetFromAPI<T>(string url, bool isAuthenticated = true)
        {
            try
            {
                var response = await GetAsync(url, isAuthenticated);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content == "notfound")
                    {
                        return new RestResponse<T>() { ErrorMessage = "Not Found", IsSuccess = false };
                    }
                    else
                    {
                        var res = new RestResponse<T>()
                        {
                            Data = JsonConvert.DeserializeObject<T>(content),
                            IsSuccess = true
                        };

                        return res;
                    }
                }
                return new RestResponse<T>() { IsSuccess = false, ErrorMessage = response.ReasonPhrase };
            }
            catch (Exception ex)
            {
                return new RestResponse<T>() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        #endregion
    }

    public class RestResponse<T>
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class VerifyCodeResponse
    {
        public string Result { get; set; }
        public string UserId { get; set; }
        public string SecurityToken { get; set; }
    }
}
