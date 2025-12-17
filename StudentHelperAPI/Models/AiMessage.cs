using System;
using System.Collections.Generic;

namespace StudentHelperAPI.Models;

public partial class AiMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsUserMessage { get; set; }

    public string? AiModel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AiConversation Conversation { get; set; } = null!;
}
