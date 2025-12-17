using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentHelperTelegramBot.Configuration;
using StudentHelperTelegramBot.Handlers;
using StudentHelperTelegramBot.Services;
using Telegram.Bot;

namespace StudentHelperTelegramBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                var botService = services.GetRequiredService<BotService>();
                await botService.StartAsync();

                Console.WriteLine("Bot started. Press Ctrl+C to exit...");
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<BotConfiguration>(
                        context.Configuration.GetSection("TelegramBot"));
                    services.Configure<ApiConfiguration>(
                        context.Configuration.GetSection("ApiSettings"));

                    services.AddSingleton<ApiService>();
                    services.AddSingleton<UserStateService>();
                    services.AddSingleton<MessageCleanupService>();
                    services.AddSingleton<CommandHandler>();

                    services.AddSingleton<MessageHandler>();

                    var botConfig = context.Configuration.GetSection("TelegramBot").Get<BotConfiguration>();
                    services.AddSingleton<ITelegramBotClient>(provider =>
                        new TelegramBotClient(botConfig!.Token));

                    services.AddSingleton<BotService>();
                });
    }
}