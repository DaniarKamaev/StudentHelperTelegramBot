using StudentHelperTelegramBot.Models;
using System.Collections.Concurrent;

namespace StudentHelperTelegramBot.Services
{
    public class UserStateService
    {
        private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

        public UserSession GetOrCreateSession(long chatId)
        {
            return _sessions.GetOrAdd(chatId, key => new UserSession
            {
                ChatId = key,
                State = UserState.None,
                Data = new Dictionary<string, string>()
            });
        }

        public void UpdateState(long chatId, UserState state)
        {
            var session = GetOrCreateSession(chatId);
            session.State = state;
            session.LastActivity = DateTime.UtcNow;
        }

        public void SetData(long chatId, string key, string value)
        {
            var session = GetOrCreateSession(chatId);
            session.Data[key] = value;
        }

        public string? GetData(long chatId, string key)
        {
            var session = GetOrCreateSession(chatId);
            return session.Data.TryGetValue(key, out var value) ? value : null;
        }

        public void ClearSession(long chatId)
        {
            _sessions.TryRemove(chatId, out _);
        }

        public void CleanupInactiveSessions(TimeSpan maxInactivity)
        {
            var cutoff = DateTime.UtcNow - maxInactivity;
            var inactiveKeys = _sessions
                .Where(kvp => kvp.Value.LastActivity < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in inactiveKeys)
            {
                _sessions.TryRemove(key, out _);
            }
        }
    }
}