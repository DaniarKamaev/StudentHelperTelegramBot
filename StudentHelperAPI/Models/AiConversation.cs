using System;
using System.Collections.Generic;

namespace StudentHelperAPI.Models;

public partial class AiConversation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Title { get; set; }

    public string? ContextType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiMessage> AiMessages { get; set; } = new List<AiMessage>();

    public virtual User User { get; set; } = null!;
}
