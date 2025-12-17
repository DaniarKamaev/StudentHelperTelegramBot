
using StudentHelperTelegramBot.Models;
using StudentHelperTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StudentHelperTelegramBot.Handlers
{
    public class CommandHandler
    {
        private readonly ApiService _apiService;
        private readonly UserStateService _stateService;

        public CommandHandler(ApiService apiService, UserStateService stateService)
        {
            _apiService = apiService;
            _stateService = stateService;
        }

        public async Task<bool> HandleCommand(
            ITelegramBotClient botClient,
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            // Главное меню
            if (text == "/start")
            {
                if (_apiService.IsAuthenticated)
                {
                    var userInfo = await _apiService.GetUserInfoAsync();
                    var userRole = "student";

                    if (userInfo != null && !string.IsNullOrEmpty(userInfo.Role))
                    {
                        userRole = userInfo.Role.ToLower();
                    }

                    await ShowRoleBasedMenu(botClient, chatId, userRole, cancellationToken);
                }
                else
                {
                    await ShowUnauthenticatedMenu(botClient, chatId, cancellationToken);
                }
                return true;
            }

            // Для неавторизованных пользователей
            if (!_apiService.IsAuthenticated)
            {
                switch (text)
                {
                    case "🔐 Авторизация":
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "📧 Введите email для входа:",
                            cancellationToken: cancellationToken);
                        _stateService.UpdateState(chatId, UserState.WaitingForEmail);
                        return true;

                    case "📝 Регистрация":
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "👤 Введите email для регистрации:",
                            cancellationToken: cancellationToken);
                        _stateService.UpdateState(chatId, UserState.WaitingForRegEmail);
                        return true;

                    default:
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "❌ Сначала авторизуйтесь или зарегистрируйтесь!",
                            cancellationToken: cancellationToken);
                        await ShowUnauthenticatedMenu(botClient, chatId, cancellationToken);
                        return true;
                }
            }

            // Для авторизованных пользователей
            return await HandleAuthenticatedCommands(botClient, chatId, text, cancellationToken);
        }

        private async Task<bool> HandleAuthenticatedCommands(
            ITelegramBotClient botClient,
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            var userInfo = await _apiService.GetUserInfoAsync();
            var userRole = "student";

            if (userInfo != null && !string.IsNullOrEmpty(userInfo.Role))
            {
                userRole = userInfo.Role.ToLower();
            }
            switch (text)
            {
                case "/menu":
                case "🏠 Главное меню":
                    await ShowRoleBasedMenu(botClient, chatId, userRole, cancellationToken);
                    return true;

                case "📚 Публикации":
                    await ShowPublicationsMenu(botClient, chatId, cancellationToken);
                    return true;

                case "➕ Создать публикацию":
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "📝 Введите заголовок публикации:",
                        cancellationToken: cancellationToken);
                    _stateService.UpdateState(chatId, UserState.WaitingForPublicationTitle);
                    return true;

                case "📋 Мои публикации":
                    await GetAllPublications(botClient, chatId, cancellationToken);
                    return true;

                case "🤖 ИИ помощник":
                    await ShowAICategories(botClient, chatId, cancellationToken);
                    return true;

                case "📖 Лекции":
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "📚 Введите предмет для поиска лекций:\n(Пример: Физика, Математика)",
                        cancellationToken: cancellationToken);
                    _stateService.UpdateState(chatId, UserState.WaitingForLectureSearchSubject);
                    return true;

                // Админские команды
                case "🏫 Создать группу":
                    if (userRole != "admin")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "❌ Эта функция доступна только администраторам!",
                            cancellationToken: cancellationToken);
                        return true;
                    }
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "🏫 Введите название группы:",
                        cancellationToken: cancellationToken);
                    _stateService.UpdateState(chatId, UserState.WaitingForGroupName);
                    return true;

                case "➕ Добавить лекцию":
                    if (userRole != "admin")
                    {
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "❌ Эта функция доступна только администраторам!",
                            cancellationToken: cancellationToken);
                        return true;
                    }

                    await botClient.SendTextMessageAsync(
                        chatId,
                        "📖 Давайте добавим новую лекцию!\n\n" +
                        "Введите заголовок лекции:",
                        cancellationToken: cancellationToken);
                    _stateService.UpdateState(chatId, UserState.WaitingForLectureTitle);
                    return true;

                case "👨‍🏫 Общая информация":
                    await GetGeneralInfo(botClient, chatId, cancellationToken);
                    return true;

                case "🔙 Назад":
                    await ShowRoleBasedMenu(botClient, chatId, userRole, cancellationToken);
                    return true;

                case "/status":
                    var status = _apiService.IsAuthenticated ? "✅ Авторизован" : "❌ Не авторизован";
                    var roleInfo = userRole == "admin" ? "👑 Администратор" : "👨‍🎓 Студент";

                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"📊 Статус бота:\n• Авторизация: {status}\n• Роль: {roleInfo}",
                        cancellationToken: cancellationToken);
                    return true;

                case "/logout":
                    _apiService.ClearToken();
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "✅ Вы вышли из системы",
                        cancellationToken: cancellationToken);
                    await ShowUnauthenticatedMenu(botClient, chatId, cancellationToken);
                    return true;

                default:
                    if (text.StartsWith("/register "))
                    {
                        await HandleRegisterCommand(botClient, chatId, text, cancellationToken);
                        return true;
                    }
                    else if (text.StartsWith("/publication "))
                    {
                        await HandleGetPublicationCommand(botClient, chatId, text, cancellationToken);
                        return true;
                    }
                    break;
            }

            return false;
        }


        public async Task ShowUnauthenticatedMenu(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            var keyboard = new[]
            {
                new[] { new KeyboardButton("🔐 Авторизация") },
                new[] { new KeyboardButton("📝 Регистрация") }
            };

            var replyMarkup = new ReplyKeyboardMarkup(keyboard)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await botClient.SendTextMessageAsync(
                chatId,
                "👋 Добро пожаловать в Student Helper Bot!\n\n" +
                "❌ Вы не авторизованы.\n" +
                "Пожалуйста, войдите или зарегистрируйтесь:",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }

        public async Task ShowRoleBasedMenu(
            ITelegramBotClient botClient,
            long chatId,
            string userRole,
            CancellationToken cancellationToken)
        {
            List<KeyboardButton[]> keyboardRows = new();

            if (userRole == "admin")
            {
                // Меню для администратора
                keyboardRows.Add(new[] { new KeyboardButton("📚 Публикации"), new KeyboardButton("🤖 ИИ помощник") });
                keyboardRows.Add(new[] { new KeyboardButton("📖 Лекции"), new KeyboardButton("➕ Добавить лекцию") });
                keyboardRows.Add(new[] { new KeyboardButton("🏫 Создать группу"), new KeyboardButton("👨‍🏫 Общая информация") });
                keyboardRows.Add(new[] { new KeyboardButton("/status"), new KeyboardButton("/logout") });
                keyboardRows.Add(new[] { new KeyboardButton("🏠 Главное меню") });
            }
            else
            {
                // Меню для студента
                keyboardRows.Add(new[] { new KeyboardButton("📚 Публикации"), new KeyboardButton("🤖 ИИ помощник") });
                keyboardRows.Add(new[] { new KeyboardButton("📖 Лекции"), new KeyboardButton("👨‍🏫 Общая информация") });
                keyboardRows.Add(new[] { new KeyboardButton("/status"), new KeyboardButton("/logout") });
                keyboardRows.Add(new[] { new KeyboardButton("🏠 Главное меню") });
            }

            var replyMarkup = new ReplyKeyboardMarkup(keyboardRows)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            var roleText = userRole == "admin" ? "👑 Администратор" : "👨‍🎓 Студент";

            await botClient.SendTextMessageAsync(
                chatId,
                $"✅ Вы авторизованы!\n" +
                $"Роль: {roleText}\n\n" +
                "Выберите действие из меню:",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }

        private async Task ShowPublicationsMenu(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            var keyboard = new[]
            {
                new[] { new KeyboardButton("📋 Мои публикации"), new KeyboardButton("➕ Создать публикации") },
                new[] { new KeyboardButton("🏠 Главное меню") }
            };

            var replyMarkup = new ReplyKeyboardMarkup(keyboard)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await botClient.SendTextMessageAsync(
                chatId,
                "📚 Управление публикациями:\n\n" +
                "• 📋 Мои публикации - просмотр всех публикаций\n" +
                "• ➕ Создать публикации - создать новую публикацию\n\n" +
                "Для просмотра конкретной публикации используйте:\n" +
                "/publication {id}",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }

        private async Task ShowAICategories(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            var keyboard = new[]
            {
                new[] { new KeyboardButton("math"), new KeyboardButton("programming") },
                new[] { new KeyboardButton("lectures"), new KeyboardButton("general") },
                new[] { new KeyboardButton("🔙 Назад"), new KeyboardButton("🏠 Главное меню") }
            };

            var replyMarkup = new ReplyKeyboardMarkup(keyboard)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await botClient.SendTextMessageAsync(
                chatId,
                "🤖 Выберите категорию для ИИ-помощника:\n\n" +
                "• math - математика\n" +
                "• programming - программирование\n" +
                "• lectures - лекции\n" +
                "• general - общие вопросы",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);

            _stateService.UpdateState(chatId, UserState.WaitingForAICategory);
        }

        private async Task HandleRegisterCommand(
            ITelegramBotClient botClient,
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            try
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "❌ Неверный формат. Используйте:\n" +
                        "/register email password firstName lastName [groupId]\n\n" +
                        "Примеры:\n" +
                        "/register student@example.com 12345 Иван Иванов\n" +
                        "/register student@example.com 12345 Иван Иванов GroupName",
                        cancellationToken: cancellationToken);
                    return;
                }

                var email = parts[1];
                var password = parts[2];
                var firstName = parts[3];
                var lastName = parts[4];
                var groupId = parts.Length > 5 ? parts[5] : null;
                var role = "student";

                var response = await _apiService.RegisterAsync(email, password, firstName, lastName, groupId);

                if (response.Success && !string.IsNullOrEmpty(response.Token))
                {
                    _apiService.SetToken(response.Token);
                    var userInfo = await _apiService.GetUserInfoAsync();
                    var userRole = userInfo?.Role ?? "student";

                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"✅ Регистрация успешна!\n\n" +
                        $"ID пользователя: {response.Id}\n" +
                        $"Email: {email}\n" +
                        $"Имя: {firstName} {lastName}\n" +
                        $"Роль: {(userRole == "admin" ? "👑 Администратор" : "👨‍🎓 Студент")}\n\n" +
                        $"Вы автоматически вошли в систему!",
                        cancellationToken: cancellationToken);

                    await ShowRoleBasedMenu(botClient, chatId, userRole, cancellationToken);
                }
                else if (response.Success)
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"✅ Регистрация успешна!\n\n" +
                        $"ID пользователя: {response.Id}\n" +
                        $"Email: {email}\n" +
                        $"Имя: {firstName} {lastName}\n" +
                        $"Роль: Студент (автоматически)",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"❌ Ошибка регистрации: {response.Message}",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка: {ex.Message}",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleGetPublicationCommand(
            ITelegramBotClient botClient,
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            try
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "❌ Неверный формат. Используйте:\n" +
                        "/publication {id}",
                        cancellationToken: cancellationToken);
                    return;
                }

                var publicationId = parts[1];
                var response = await _apiService.GetPublicationAsync(publicationId);

                if (response.Success && response.Publication != null)
                {
                    var publication = response.Publication;
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"📄 Публикация:\n\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Тип: {publication.PublicationType}\n" +
                        $"Автор ID: {publication.AuthorId}\n" +
                        $"Группа ID: {publication.GroupId}\n" +
                        $"Опубликована: {(publication.IsPublished ? "Да" : "Нет")}\n" +
                        $"Создана: {publication.CreatedAt:dd.MM.yyyy HH:mm}\n" +
                        $"Обновлена: {publication.UpdatedAt:dd.MM.yyyy HH:mm}\n\n" +
                        $"Содержание:\n{publication.Content}",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"❌ Публикация не найдена: {response.Message}",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка: {ex.Message}",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task GetAllPublications(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            try
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    "🔍 Загружаю публикации...",
                    cancellationToken: cancellationToken);

                var response = await _apiService.GetAllPublicationsAsync();

                if (response.Success && response.Publications != null && response.Publications.Any())
                {
                    var messageText = "📚 Все публикации:\n\n";
                    foreach (var publication in response.Publications.Take(5))
                    {
                        messageText += $"📄 {publication.Title}\n";
                        messageText += $"   ID: {publication.Id}\n";
                        messageText += $"   Тип: {publication.PublicationType}\n";
                        messageText += $"   Автор: {publication.AuthorId}\n";
                        messageText += $"   Дата: {publication.CreatedAt:dd.MM.yyyy}\n\n";

                        if (publication.Content.Length > 100)
                        {
                            messageText += $"   {publication.Content.Substring(0, 100)}...\n\n";
                        }
                        else
                        {
                            messageText += $"   {publication.Content}\n\n";
                        }
                    }

                    if (response.Publications.Count > 5)
                    {
                        messageText += $"... и ещё {response.Publications.Count - 5} публикаций\n\n";
                    }

                    messageText += "Для просмотра полной публикации используйте:\n";
                    messageText += "/publication {id}";

                    await botClient.SendTextMessageAsync(chatId, messageText, cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "📭 Публикаций пока нет.", cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"❌ Ошибка: {ex.Message}", cancellationToken: cancellationToken);
            }
        }

        private async Task GetGeneralInfo(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            var messageText = "📊 Student Helper Bot\n\n" +
                         "🤖 Функции:\n" +
                         "• ИИ-помощник по предметам (math, programming, lectures, general)\n" +
                         "• 📚 Публикации (homework, solution, material)\n" +
                         "• 📖 Поиск лекций по предметам\n" +
                         "• ➕ Добавление новых лекций (для администраторов)\n" +
                         "• 👥 Управление группами (для администраторов)\n\n" +
                         "📝 Команды:\n" +
                         "/start - Главное меню\n" +
                         "/register - Регистрация\n" +
                         "/logout - Выход\n" +
                         "/status - Статус бота\n" +
                         "/publication {id} - Просмотр публикации\n\n" +
                         "⚠️ Для некоторых функций требуется авторизация";

            await botClient.SendTextMessageAsync(chatId, messageText, cancellationToken: cancellationToken);
        }

        public async Task ShowRoleBasedMenu(
            ITelegramBotClient botClient,
            long chatId,
            CancellationToken cancellationToken)
        {
            var userInfo = await _apiService.GetUserInfoAsync();
            var userRole = userInfo?.Role ?? "student";

            await ShowRoleBasedMenu(botClient, chatId, userRole, cancellationToken);
        }
    }
}