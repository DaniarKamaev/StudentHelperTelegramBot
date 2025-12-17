using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;

namespace StudentHelperTelegramBot.Services
{
    public class MessageCleanupService
    {
        private readonly ITelegramBotClient _botClient;

        public MessageCleanupService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SafeDeleteMessageAsync(long chatId, int messageId)
        {
            try
            {
                await _botClient.DeleteMessageAsync(chatId, messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось удалить сообщение {messageId}: {ex.Message}");
            }
        }

        public async Task DeleteAuthMessagesAsync(long chatId, List<int> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                await SafeDeleteMessageAsync(chatId, messageId);
            }
        }
    }
}