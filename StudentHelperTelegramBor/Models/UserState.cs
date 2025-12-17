using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentHelperTelegramBot.Models
{
    public class UserSession
    {
        public long ChatId { get; set; }
        public UserState State { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        public List<int> MessagesToDelete { get; set; } = new();
    }
}