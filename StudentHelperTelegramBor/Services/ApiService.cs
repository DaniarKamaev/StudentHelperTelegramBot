using Microsoft.Extensions.Options;
using StudentHelperTelegramBot.Configuration;
using StudentHelperTelegramBot.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace StudentHelperTelegramBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _apiConfig;
        private string _jwtToken = string.Empty;

        public ApiService(IOptions<ApiConfiguration> apiConfig)
        {
            _apiConfig = apiConfig.Value;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiConfig.BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public void ClearToken()
        {
            _jwtToken = string.Empty;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var request = new { email, password };
                var content = CreateJsonContent(request);

                var response = await _httpClient.PostAsync("helper/auth", content);
                return await HandleResponse<AuthResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new AuthResponse(null, false, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<AddLectureResponse> AddLectureAsync(
            string title, string description, string externalUrl, string subject)
        {
            try
            {
                var request = new
                {
                    title,
                    description,
                    external_url = externalUrl,
                    subject
                };

                var content = CreateJsonContent(request);
                var response = await _httpClient.PostAsync("helper/admin/lecture/add", content);
                return await HandleResponse<AddLectureResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Add lecture error: {ex.Message}");
                return new AddLectureResponse(null, false, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<RegistrationResponse> RegisterAsync(
            string email, string password, string firstName,
            string lastName, string? groupId = null)
        {
            try
            {
                string finalGroupId;
                if (groupId == null)
                    finalGroupId = "08de3bb7-23ba-4bc0-8623-0e0ea69477bb";
                else
                    finalGroupId =  groupId;

                    var request = new
                    {
                        email,
                        password,
                        firstNamem = firstName,
                        lastName,
                        role = "student",
                        GrupId = finalGroupId
                    };

                var content = CreateJsonContent(request);
                var response = await _httpClient.PostAsync("helper/reg", content);
                return await HandleResponse<RegistrationResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex.Message}");
                return new RegistrationResponse(null, false, $"Ошибка: {ex.Message}", null);
            }
        }

        public async Task<AddPublicationResponse> CreatePublicationAsync(
            string title, string content, string publicationType)
        {
            try
            {
                var request = new
                {
                    title,
                    content,
                    publication_type = publicationType
                };

                var contentData = CreateJsonContent(request);
                var response = await _httpClient.PostAsync("helper/publications", contentData);
                return await HandleResponse<AddPublicationResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create publication error: {ex.Message}");
                return new AddPublicationResponse(null, false, $"Ошибка: {ex.Message}");
            }
        }
        public async Task<UserInfoResponse?> GetUserInfoAsync()
        {
            try
            {
                if (!IsAuthenticated)
                {
                    Console.WriteLine("GetUserInfoAsync: User is not authenticated");
                    return null;
                }

                Console.WriteLine("GetUserInfoAsync: User is authenticated, parsing JWT token...");

                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        if (handler.CanReadToken(_jwtToken))
                        {
                            var token = handler.ReadJwtToken(_jwtToken);

                            var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier ||
                                                                           c.Type == "UserId")?.Value;
                            var userEmail = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                            var userFirstName = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                            var userLastName = "";
                            var userRole = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role ||
                                                                             c.Type == "UserRole" ||
                                                                             c.Type == "role")?.Value;

                            Console.WriteLine($"Parsed from JWT: UserId={userId}, Email={userEmail}, Name={userFirstName}, Role={userRole}");

                            if (!string.IsNullOrEmpty(userId))
                            {
                                return new UserInfoResponse(
                                    userId,
                                    userEmail ?? "unknown@email.com",
                                    userFirstName ?? "Unknown",
                                    userLastName,
                                    userRole ?? "student"
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"JWT parsing error: {ex.Message}");
                    }
                }

                Console.WriteLine("GetUserInfoAsync: Could not get user info from JWT");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserInfoAsync general error: {ex.Message}");
                return null;
            }
        }
        public async Task<List<Lecture>> GetLecturesAsync(string subject)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"helper/lectures/{Uri.EscapeDataString(subject)}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Lecture>>(json,
                        GetJsonOptions()) ?? new List<Lecture>();
                }
                return new List<Lecture>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get lectures error: {ex.Message}");
                return new List<Lecture>();
            }
        }

        public async Task<AddGroupResponse> CreateGroupAsync(string name)
        {
            try
            {
                var request = new { Name = name };
                var content = CreateJsonContent(request);

                var response = await _httpClient.PostAsync("helper/admin/group/add", content);
                return await HandleResponse<AddGroupResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create group error: {ex.Message}");
                return new AddGroupResponse(null, false, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<Result<SendMessageResponse>> SendAIMessageAsync(
            string message, string contextType)
        {
            try
            {
                var request = new { message, contextType };
                var content = CreateJsonContent(request);

                var response = await _httpClient.PostAsync("helper/ai/chat", content);

                if (response.IsSuccessStatusCode)
                {
                    var sendResponse = await HandleResponse<SendMessageResponse>(response);
                    return new Result<SendMessageResponse>(true, sendResponse, null);
                }

                return new Result<SendMessageResponse>(false, null,
                    $"Ошибка HTTP: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI error: {ex.Message}");
                return new Result<SendMessageResponse>(false, null, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<ReadPublicationsResponse> GetAllPublicationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("helper/publications");
                return await HandleResponse<ReadPublicationsResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get publications error: {ex.Message}");
                return new ReadPublicationsResponse(false, $"Ошибка: {ex.Message}", null);
            }
        }

        public async Task<GetPublicationResponse> GetPublicationAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"helper/publications/{id}");
                return await HandleResponse<GetPublicationResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get publication error: {ex.Message}");
                return new GetPublicationResponse(false, $"Ошибка: {ex.Message}", null);
            }
        }

        private static async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API response: {response.StatusCode} - {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, GetJsonOptions());
                return result ?? throw new InvalidOperationException("Ошибка десериализации");
            }

            var instance = Activator.CreateInstance<T>();
            return instance;
        }

        private static StringContent CreateJsonContent(object data)
        {
            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static JsonSerializerOptions GetJsonOptions() => new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}