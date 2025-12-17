namespace StudentHelperTelegramBot.Configuration
{
    public class BotConfiguration
    {
        public string Token { get; set; } = string.Empty;
    }

    public class ApiConfiguration
    {
        public string BaseUrl { get; set; } = "https://localhost:5001/api";
    }
}