namespace StudentHelperAPI.Features.AI.Send
{
    public record SendMessageResponse(
    string Answer,
    string ConversationId,
    DateTime CreatedAt);
}
