using Microsoft.Extensions.Options;
using StudentHelperAPI.Core.Abstractions;
using StudentHelperAPI.Core.Common;
using System.Text;
using System.Text.Json;

namespace StudentHelperAPI.Infrastructure.Services
{
    public class GigaChatService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GigaChatSettings _settings;
        private readonly ILogger<GigaChatService> _logger;
        private string? _accessToken;
        private DateTime _tokenExpiresAt;
        private readonly object _tokenLock = new object();

        public GigaChatService(
            IOptions<GigaChatSettings> settings,
            ILogger<GigaChatService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _logger.LogInformation("GigaChat Service initialized");
        }

        public async Task<Result<string>> GetResponseAsync(string message, string contextType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting GigaChat response for message: {MessageLength} chars", message.Length);

                var tokenResult = await GetAccessTokenAsync(cancellationToken);
                if (!tokenResult.IsSuccess)
                {
                    return Result.Failure<string>(tokenResult.Error);
                }

                var systemPrompt = GetSystemPrompt(contextType);

                var request = new
                {
                    model = "GigaChat",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = message }
                    },
                    max_tokens = 1000,
                    temperature = 0.7,
                    stream = false
                };

                _logger.LogDebug("Sending request to GigaChat chat/completions");

                using var chatRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://gigachat.devices.sberbank.ru/api/v1/chat/completions");

                chatRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                chatRequest.Headers.Add("Accept", "application/json");

                chatRequest.Content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(chatRequest, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug("Chat response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GigaChat API error: {StatusCode} - {Content}",
                        response.StatusCode, responseContent);
                    return Result.Failure<string>($"AI service error: {response.StatusCode}");
                }

                var gigaChatResponse = JsonSerializer.Deserialize<GigaChatResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var aiResponse = gigaChatResponse?.Choices?[0]?.Message?.Content ?? "No response from AI";

                _logger.LogInformation("GigaChat response received successfully");

                return Result.Success(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GigaChatService.GetResponseAsync");
                return Result.Failure<string>($"AI service error: {ex.Message}");
            }
        }

        private async Task<Result> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt)
                {
                    _logger.LogDebug("Using cached token");
                    return Result.Success();
                }
            }

            try
            {
                _logger.LogInformation("Requesting new access token from GigaChat");

                var tokenUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

                var authString = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

                using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
                tokenRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
                tokenRequest.Headers.Add("Accept", "application/json");
                tokenRequest.Headers.Add("RqUID", Guid.NewGuid().ToString());

                var formData = new Dictionary<string, string>
                {
                    ["scope"] = _settings.Scope
                };

                tokenRequest.Content = new FormUrlEncodedContent(formData);

                _logger.LogDebug("Sending token request to: {TokenUrl}", tokenUrl);

                var response = await _httpClient.SendAsync(tokenRequest, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogDebug("Token response status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Token response content: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get GigaChat token: {StatusCode} - {Content}",
                        response.StatusCode, responseContent);
                    return Result.Failure($"Failed to get access token: {response.StatusCode}");
                }

                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("access_token", out var accessTokenElement) ||
                    string.IsNullOrEmpty(accessTokenElement.GetString()))
                {
                    _logger.LogError("No access_token in response: {Response}", responseContent);
                    return Result.Failure("No access token received from GigaChat");
                }

                var accessToken = accessTokenElement.GetString()!;

                long expiresAtMs = 0;
                if (root.TryGetProperty("expires_at", out var expiresAtElement))
                {
                    if (expiresAtElement.ValueKind == JsonValueKind.Number)
                    {
                        expiresAtMs = expiresAtElement.GetInt64();
                    }
                }

                lock (_tokenLock)
                {
                    _accessToken = accessToken;

                    if (expiresAtMs > 0)
                    {
                        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        _tokenExpiresAt = epoch.AddMilliseconds(expiresAtMs).AddMinutes(-5);
                    }
                    else
                    {
                        _tokenExpiresAt = DateTime.UtcNow.AddMinutes(25);
                    }

                    _logger.LogInformation("GigaChat token received. Expires at: {ExpiresAt}", _tokenExpiresAt);
                    _logger.LogDebug("Token: {TokenStart}...",
                        _accessToken.Length > 50 ? _accessToken.Substring(0, 50) : _accessToken);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GigaChat access token");
                return Result.Failure($"Failed to get access token: {ex.Message}");
            }
        }

        private string GetSystemPrompt(string contextType)
        {
            return contextType switch
            {
                "math" => "Ты помощник по математике для студентов колледжа. Решай примеры, объясняй шаги решения, помогай с теорией. Будь понятным и терпеливым.",
                "programming" => "Ты помощник по программированию для студентов. Объясняй код простыми словами, помогай с отладкой, предлагай решения. Пиши примеры кода на C#.",
                "lectures" => "Ты помощник для конспектирования лекций. Структурируй информацию, выделяй главное, объясняй сложные термины простыми словами.",
                _ => "Ты полезный AI-ассистент GigaChat для студентов. Помогай с учебными задачами, объясняй сложные темы, будь дружелюбным и поддерживающим."
            };
        }

        private class GigaChatResponse
        {
            public List<Choice>? Choices { get; set; }

            public class Choice
            {
                public Message? Message { get; set; }
            }

            public class Message
            {
                public string? Content { get; set; }
            }
        }
    }

    public class GigaChatSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = "GIGACHAT_API_PERS";
    }
}