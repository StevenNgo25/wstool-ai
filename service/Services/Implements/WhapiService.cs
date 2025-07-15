using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace WhatsAppCampaignManager.Services.Implements
{
    public class WhapiService : IWhapiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhapiService> _logger;
        private const string DefaultBaseUrl = "https://gate.whapi.cloud";

        public WhapiService(HttpClient httpClient, ILogger<WhapiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(List<WhapiGroup> items, int total)> GetGroupsAsync(string token, string? baseUrl = null, int count = 20, int offset = 0)
        {
            try
            {
                var url = $"{baseUrl ?? DefaultBaseUrl}/groups?count={count}&offset={offset}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                        
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get groups. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return (new List<WhapiGroup>(), 0);
                }

                var content = await response.Content.ReadAsStringAsync();
                var groups = JsonConvert.DeserializeObject<JObject>(content);
                
                var result = groups?["groups"]?.Select(g => new WhapiGroup
                {
                    Id = g["id"]?.ToString() ?? string.Empty,
                    Name = g["name"]?.ToString() ?? string.Empty,
                    ParticipantCount = g["participants"]?.Count() ?? 0,
                    RawData = JsonConvert.SerializeObject(g)
                }).ToList() ?? new List<WhapiGroup>();

                var total = int.TryParse(groups?["total"]?.ToString(), out var totalCount) ? totalCount : 0;
                
                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups from WHAPI");
                return (new List<WhapiGroup>(), 0);
            }
        }

        public async Task<List<WhapiGroupMember>> GetGroupMembersAsync(string token, string groupId, string? baseUrl = null)
        {
            try
            {
                var url = $"{baseUrl ?? DefaultBaseUrl}/groups/{groupId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                        
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get group members. Status: {StatusCode}", response.StatusCode);
                    return new List<WhapiGroupMember>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<JObject>(content);
                
                var result = group?["participants"]?.Select(p => new WhapiGroupMember
                {
                    Id = p["id"]?.ToString() ?? string.Empty,
                    Name = p["id"]?.ToString() ?? string.Empty,
                    IsAdmin = p["rank"]?.ToString().ToLower() == "admin",
                }).ToList() ?? new List<WhapiGroupMember>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members from WHAPI");
                return new List<WhapiGroupMember>();
            }
        }

        public async Task<WhapiMessageResponse> SendMessageAsync(string token, string groupId, string message, string? imageBase64 = null, string? baseUrl = null)
        {
            try
            {
                var url = $"{baseUrl ?? DefaultBaseUrl}/messages/" +
                    (!string.IsNullOrEmpty(imageBase64) ? "image" : "text");

                var payload = new
                {
                    to = groupId,
                    body = !string.IsNullOrEmpty(imageBase64) ? "" : message,
                    caption = !string.IsNullOrEmpty(imageBase64) ? message : "",
                    media = !string.IsNullOrEmpty(imageBase64) ? imageBase64 : null
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var messageRes = JsonConvert.DeserializeObject<JObject>(responseContent);
                    var result = new WhapiMessageResponse
                    {
                        Id = messageRes?["message"]?["id"]?.ToString() ?? string.Empty,
                        Sent = messageRes?["sent"]?.ToString().ToLower() == "true",
                        Error = messageRes?["error"]?["message"]?.ToString()
                    };
                    return result;
                }
                else
                {
                    _logger.LogError("Failed to send message to group. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, responseContent);
                    return new WhapiMessageResponse { Sent = false, Error = responseContent };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to group via WHAPI");
                return new WhapiMessageResponse { Sent = false, Error = ex.Message };
            }
        }

        public async Task<WhapiMessageStatus> GetMessageStatusAsync(string token, string messageId, string? baseUrl = null)
        {
            try
            {
                var url = $"{baseUrl ?? DefaultBaseUrl}/messages/{messageId}/status";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                        
                if (!response.IsSuccessStatusCode)
                {
                    return new WhapiMessageStatus { Id = messageId, Ack = "unknown" };
                }

                var content = await response.Content.ReadAsStringAsync();
                var status = JsonConvert.DeserializeObject<WhapiMessageStatus>(content);
                return status ?? new WhapiMessageStatus { Id = messageId, Ack = "unknown" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message status from WHAPI");
                return new WhapiMessageStatus { Id = messageId, Ack = "error" };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token, string? baseUrl = null)
        {
            try
            {
                var url = $"{baseUrl ?? DefaultBaseUrl}/settings";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token with WHAPI");
                return false;
            }
        }
    }
}
