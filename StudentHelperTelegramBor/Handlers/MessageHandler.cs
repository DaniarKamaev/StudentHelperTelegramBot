using StudentHelperTelegramBot.Models;
using StudentHelperTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StudentHelperTelegramBot.Handlers
{
    public class MessageHandler
    {
        private readonly ApiService _apiService;
        private readonly UserStateService _stateService;
        private readonly CommandHandler _commandHandler;
        private readonly ITelegramBotClient _botClient;
        private readonly MessageCleanupService _cleanupService;

        public MessageHandler(
            ApiService apiService,
            UserStateService stateService,
            CommandHandler commandHandler,
            ITelegramBotClient botClient,
            MessageCleanupService cleanupService)
        {
            _apiService = apiService;
            _stateService = stateService;
            _commandHandler = commandHandler;
            _botClient = botClient;
            _cleanupService = cleanupService;
        }

        public async Task HandleUpdateAsync(
    Update update,
    CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type != UpdateType.Message)
                    return;

                var message = update.Message;
                if (message == null || string.IsNullOrEmpty(message.Text))
                    return;

                var chatId = message.Chat.Id;
                var text = message.Text;
                var messageId = message.MessageId;

                Console.WriteLine($"Received: {text} from {message.From?.Username} (MessageId: {messageId})");

                var session = _stateService.GetOrCreateSession(chatId);

                if (session.State != UserState.None)
                {
                    if (session.State == UserState.WaitingForPassword ||
                        session.State == UserState.WaitingForEmail ||
                        session.State == UserState.WaitingForRegEmail ||
                        session.State == UserState.WaitingForRegPassword)
                    {
                        await _cleanupService.DeleteAuthMessagesAsync(chatId, session.MessagesToDelete);
                        session.MessagesToDelete.Clear();
                    }

                    session.MessagesToDelete.Add(messageId);
                    await ProcessUserState(chatId, text, cancellationToken);
                    return;
                }

                if (await _commandHandler.HandleCommand(_botClient, chatId, text, cancellationToken))
                    return;

                if (!_apiService.IsAuthenticated && session.State == UserState.None)
                {
                    await _commandHandler.ShowUnauthenticatedMenu(_botClient, chatId, cancellationToken);
                    return;
                }

                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Пожалуйста, выберите команду из меню.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling update: {ex.Message}");
            }
        }

        private async Task ProcessUserState(
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            var session = _stateService.GetOrCreateSession(chatId);

            switch (session.State)
            {
                case UserState.WaitingForEmail:
                    await HandleEmailInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForPassword:
                    await HandlePasswordInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForRegEmail:
                    await HandleRegEmailInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForRegPassword:
                    await HandleRegPasswordInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForRegFirstName:
                    await HandleRegFirstNameInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForRegLastName:
                    await HandleRegLastNameInput(chatId, text, cancellationToken);
                    break;


                case UserState.WaitingForRegGroupId:
                    await HandleRegGroupIdInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForPublicationTitle:
                    await HandlePublicationTitleInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForPublicationContent:
                    await HandlePublicationContentInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForPublicationType:
                    await HandlePublicationTypeInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForGroupName:
                    await HandleGroupNameInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForAICategory:
                    await HandleAICategoryInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForAIQuestion:
                    await HandleAIQuestionInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForLectureTitle:
                    await HandleLectureTitleInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForLectureDescription:
                    await HandleLectureDescriptionInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForLectureSearchSubject:
                    await HandleLectureSearchSubjectInput(chatId, text, cancellationToken);
                    break;


                case UserState.WaitingForLectureExternalUrl:
                    await HandleLectureExternalUrlInput(chatId, text, cancellationToken);
                    break;

                case UserState.WaitingForLectureAddSubject:
                    await HandleLectureAddSubjectInput(chatId, text, cancellationToken);
                    break;

                default:
                    await _botClient.SendTextMessageAsync(
                        chatId,
                        "Пожалуйста, выберите команду из меню.",
                        cancellationToken: cancellationToken);
                    break;
            }
        }

        #region Auth Handlers
        private async Task HandleEmailInput(long chatId, string email, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            session.Data["email"] = email;
            _stateService.UpdateState(chatId, UserState.WaitingForPassword);

            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId,
                "🔑 Введите пароль:",
                cancellationToken: ct);

            session.MessagesToDelete.Add(sentMessage.MessageId);
        }

        private async Task HandlePasswordInput(long chatId, string password, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            var email = session.Data.GetValueOrDefault("email");
            if (string.IsNullOrEmpty(email))
                return;

            var response = await _apiService.LoginAsync(email, password);

            if (response.Success && !string.IsNullOrEmpty(response.Token))
            {
                _apiService.SetToken(response.Token);

                await _cleanupService.DeleteAuthMessagesAsync(chatId, session.MessagesToDelete);
                session.MessagesToDelete.Clear();

                var userInfo = await _apiService.GetUserInfoAsync();
                var userRole = "student";

                if (userInfo != null && !string.IsNullOrEmpty(userInfo.Role))
                {
                    userRole = userInfo.Role.ToLower();
                }

                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Авторизация успешна!\n\n" +
                    $"Добро пожаловать, {userInfo?.FirstName ?? "пользователь"}!\n" +
                    $"Роль: {(userRole == "admin" ? "👑 Администратор" : "👨‍🎓 Студент")}",
                    cancellationToken: ct);

                await ShowRoleBasedMenuWithRole(_botClient, chatId, userRole, ct);
            }

            _stateService.ClearSession(chatId);
        }
        private async Task ShowRoleBasedMenuWithRole(
            ITelegramBotClient botClient,
            long chatId,
            string userRole,
            CancellationToken ct)
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
                cancellationToken: ct);
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
        #endregion

        #region Registration Handlers
        private async Task HandleRegEmailInput(long chatId, string email, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            session.Data["regEmail"] = email;
            _stateService.UpdateState(chatId, UserState.WaitingForRegPassword);

            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId,
                "🔑 Введите пароль для регистрации:",
                cancellationToken: ct);

            session.MessagesToDelete.Add(sentMessage.MessageId);
        }

        private async Task HandleRegPasswordInput(long chatId, string password, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            var email = session.Data.GetValueOrDefault("regEmail");
            if (string.IsNullOrEmpty(email))
                return;

            session.Data["regPassword"] = password;
            _stateService.UpdateState(chatId, UserState.WaitingForRegFirstName);

            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId,
                "👤 Введите имя:",
                cancellationToken: ct);

            session.MessagesToDelete.Add(sentMessage.MessageId);
        }

        private async Task HandleRegFirstNameInput(long chatId, string firstName, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            session.Data["regFirstName"] = firstName;
            _stateService.UpdateState(chatId, UserState.WaitingForRegLastName);

            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId,
                "👤 Введите фамилию:",
                cancellationToken: ct);

            session.MessagesToDelete.Add(sentMessage.MessageId);
        }

        private async Task HandleRegLastNameInput(long chatId, string lastName, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            session.Data["regLastName"] = lastName;
            _stateService.UpdateState(chatId, UserState.WaitingForRegGroupId);

            var sentMessage = await _botClient.SendTextMessageAsync(
                chatId,
                "🏫 Введите название группы(если нет, поставте -):",
                cancellationToken: ct);

            session.MessagesToDelete.Add(sentMessage.MessageId);
        }

        private async Task HandleRegGroupIdInput(long chatId, string groupId, CancellationToken ct)
        {
            var session = _stateService.GetOrCreateSession(chatId);
            var email = session.Data.GetValueOrDefault("regEmail");
            var password = session.Data.GetValueOrDefault("regPassword");
            var firstName = session.Data.GetValueOrDefault("regFirstName");
            var lastName = session.Data.GetValueOrDefault("regLastName");
            var role = "student";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Ошибка данных регистрации. Начните заново.",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var finalGroupId = string.IsNullOrWhiteSpace(groupId) ? null : groupId;
            var response = await _apiService.RegisterAsync(email, password, firstName, lastName, finalGroupId);

            await _cleanupService.DeleteAuthMessagesAsync(chatId, session.MessagesToDelete);

            if (response.Success && !string.IsNullOrEmpty(response.Token))
            {
                _apiService.SetToken(response.Token);

                var userInfo = await _apiService.GetUserInfoAsync();
                var userRole = userInfo?.Role ?? "student";

                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Регистрация успешна!\n\n" +
                    $"Добро пожаловать, {userInfo?.FirstName ?? firstName}!\n" +
                    $"ID: {response.Id}\n" +
                    $"Email: {email}\n" +
                    $"Роль: {(userRole == "admin" ? "👑 Администратор" : "👨‍🎓 Студент")}",
                    cancellationToken: ct);

                await ShowRoleBasedMenuWithRole(_botClient, chatId, userRole, ct);
            }
            else if (response.Success)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Регистрация успешна!\n\n" +
                    $"ID: {response.Id}\n" +
                    $"Email: {email}\n\n" +
                    $"Теперь войдите в систему через меню авторизации.",
                    cancellationToken: ct);

                await _commandHandler.ShowUnauthenticatedMenu(_botClient, chatId, ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка регистрации: {response.Message}",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion

        #region Lecture Creation Handlers
        private async Task HandleLectureTitleInput(long chatId, string title, CancellationToken ct)
        {
            _stateService.SetData(chatId, "lectureTitle", title);
            _stateService.UpdateState(chatId, UserState.WaitingForLectureDescription);

            await _botClient.SendTextMessageAsync(
                chatId,
                "📝 Введите описание лекции:",
                cancellationToken: ct);
        }

        private async Task HandleLectureDescriptionInput(long chatId, string description, CancellationToken ct)
        {
            _stateService.SetData(chatId, "lectureDescription", description);
            _stateService.UpdateState(chatId, UserState.WaitingForLectureAddSubject);

            await _botClient.SendTextMessageAsync(
                chatId,
                "📚 Введите предмет лекции:",
                cancellationToken: ct);
        }

        #region Lecture Search Handler
        private async Task HandleLectureAddSubjectInput(long chatId, string subject, CancellationToken ct)
        {
            _stateService.SetData(chatId, "lectureSubject", subject);
            _stateService.UpdateState(chatId, UserState.WaitingForLectureExternalUrl);

            await _botClient.SendTextMessageAsync(
                chatId,
                "🔗 Введите внешнюю ссылку на лекцию (URL):",
                cancellationToken: ct);
        }

        private async Task HandleLectureSearchSubjectInput(long chatId, string subject, CancellationToken ct)
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "🔍 Ищу лекции...",
                cancellationToken: ct);

            var lectures = await _apiService.GetLecturesAsync(subject);
            if (lectures.Any())
            {
                var messageText = $"📚 Лекции по предмету \"{subject}\":\n\n";
                foreach (var lecture in lectures.Take(10))
                {
                    messageText += $"📖 {lecture.Title}\n";
                    messageText += $"   Предмет: {lecture.Subject}\n";
                    messageText += $"   Описание: {lecture.Description}\n";
                    messageText += $"   Ссылка: {lecture.ExternalUrl}\n";
                    messageText += $"   Дата: {lecture.CreatedAt:dd.MM.yyyy}\n\n";
                }

                if (lectures.Count > 10)
                {
                    messageText += $"... и ещё {lectures.Count - 10} лекций";
                }

                await _botClient.SendTextMessageAsync(chatId, messageText, cancellationToken: ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Лекции по данному предмету не найдены.",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion

        private async Task HandleLectureExternalUrlInput(long chatId, string externalUrl, CancellationToken ct)
        {
            var title = _stateService.GetData(chatId, "lectureTitle");
            var description = _stateService.GetData(chatId, "lectureDescription");
            var subject = _stateService.GetData(chatId, "lectureSubject");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) ||
                string.IsNullOrEmpty(subject))
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Ошибка данных. Начните заново.",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            if (!_apiService.IsAuthenticated)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Требуется авторизация!",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var userInfo = await _apiService.GetUserInfoAsync();
            if (userInfo?.Role != "admin")
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Эта функция доступна только администраторам!",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var response = await _apiService.AddLectureAsync(title, description, externalUrl, subject);

            if (response.Success)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Лекция успешно добавлена!\n\n" +
                    $"ID: {response.Id}\n" +
                    $"Заголовок: {title}\n" +
                    $"Предмет: {subject}\n" +
                    $"Ссылка: {externalUrl}",
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка: {response.Message}",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion

        #region Publication Handlers
        private async Task HandlePublicationTitleInput(long chatId, string title, CancellationToken ct)
        {
            _stateService.SetData(chatId, "publicationTitle", title);
            _stateService.UpdateState(chatId, UserState.WaitingForPublicationContent);

            await _botClient.SendTextMessageAsync(
                chatId,
                "📝 Введите содержание публикации:",
                cancellationToken: ct);
        }

        private async Task HandlePublicationContentInput(long chatId, string content, CancellationToken ct)
        {
            var title = _stateService.GetData(chatId, "publicationTitle");
            if (string.IsNullOrEmpty(title))
                return;

            _stateService.SetData(chatId, "publicationContent", content);
            _stateService.UpdateState(chatId, UserState.WaitingForPublicationType);

            await _botClient.SendTextMessageAsync(
                chatId,
                "📄 Выберите тип публикации:\n• homework - домашнее задание\n• solution - решение\n• material - материал",
                cancellationToken: ct);
        }

        private async Task HandlePublicationTypeInput(long chatId, string publicationType, CancellationToken ct)
        {
            var title = _stateService.GetData(chatId, "publicationTitle");
            var content = _stateService.GetData(chatId, "publicationContent");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                return;

            var type = publicationType.ToLower();
            if (type != "homework" && type != "solution" && type != "material")
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Неверный тип публикации. Используйте: homework, solution или material",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var response = await _apiService.CreatePublicationAsync(title, content, type);

            if (response.Success)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Публикация создана!\n\n" +
                    $"ID: {response.Id}\n" +
                    $"Заголовок: {title}\n" +
                    $"Тип: {type}",
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка: {response.Message}",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion

        #region Group Handlers
        private async Task HandleGroupNameInput(long chatId, string groupName, CancellationToken ct)
        {
            if (!_apiService.IsAuthenticated)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Требуется авторизация!",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var userInfo = await _apiService.GetUserInfoAsync();
            if (userInfo?.Role != "admin")
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Эта функция доступна только администраторам!",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            var response = await _apiService.CreateGroupAsync(groupName);
            if (response.Success)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"✅ Группа создана!\n\n" +
                    $"ID: {response.Id}\n" +
                    $"Название: {groupName}",
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка: {response.Message}",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion

        #region AI Handlers
        private async Task HandleAICategoryInput(long chatId, string category, CancellationToken ct)
        {
            var validCategories = new[] { "math", "programming", "lectures", "general" };
            if (!validCategories.Contains(category.ToLower()))
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "❌ Неверная категория. Выберите из: math, programming, lectures, general",
                    cancellationToken: ct);
                _stateService.ClearSession(chatId);
                return;
            }

            _stateService.SetData(chatId, "aiCategory", category.ToLower());
            _stateService.UpdateState(chatId, UserState.WaitingForAIQuestion);

            await _botClient.SendTextMessageAsync(
                chatId,
                $"🤖 Категория: {category}\n\nВведите ваш вопрос:",
                cancellationToken: ct);
        }

        private async Task HandleAIQuestionInput(long chatId, string question, CancellationToken ct)
        {
            var category = _stateService.GetData(chatId, "aiCategory");
            if (string.IsNullOrEmpty(category))
                return;

            await _botClient.SendTextMessageAsync(
                chatId,
                "🤖 ИИ думает...",
                cancellationToken: ct);

            var aiResponse = await _apiService.SendAIMessageAsync(question, category);

            if (aiResponse.IsSuccess && aiResponse.Value != null)
            {
                var answer = aiResponse.Value.Answer;
                if (answer.Length > 4000)
                {
                    answer = answer.Substring(0, 4000) + "\n\n... (ответ обрезан)";
                }

                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"🤖 Ответ ИИ ({category}):\n\n{answer}\n\n" +
                    $"ID диалога: {aiResponse.Value.ConversationId}",
                    cancellationToken: ct);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Ошибка ИИ: {aiResponse.Error}",
                    cancellationToken: ct);
            }

            _stateService.ClearSession(chatId);
        }
        #endregion
    }

}