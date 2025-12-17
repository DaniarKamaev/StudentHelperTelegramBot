using Microsoft.Extensions.Options;
using StudentHelperTelegramBot.Configuration;
using StudentHelperTelegramBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudentHelperTelegramBot.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly MessageHandler _messageHandler;
        private readonly BotConfiguration _botConfig;
        private readonly UserStateService _stateService;
        private CancellationTokenSource _cts = new();

        public BotService(
            ITelegramBotClient botClient,
            MessageHandler messageHandler,
            IOptions<BotConfiguration> botConfig,
            UserStateService stateService)
        {
            _botClient = botClient;
            _messageHandler = messageHandler;
            _botConfig = botConfig.Value;
            _stateService = stateService;
        }

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(_botConfig.Token))
            {
                Console.WriteLine("Telegram bot token is not configured!");
                return;
            }

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Bot started: @{me.Username}");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _cts.Token
            );

            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), _cts.Token);
                        _stateService.CleanupInactiveSessions(TimeSpan.FromHours(2));
                    }
                    catch (TaskCanceledException)
                    {
                        //TODU later...
                    }
                }
            }, _cts.Token);
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            await Task.Delay(1000);
        }

        private async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            await _messageHandler.HandleUpdateAsync(update, cancellationToken);
        }

        private Task HandlePollingErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Polling error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}