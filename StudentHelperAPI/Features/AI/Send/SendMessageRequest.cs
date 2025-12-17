using MediatR;
using StudentHelperAPI.Core.Common;

namespace StudentHelperAPI.Features.AI.Send
{
    public record SendMessageRequest(
        string Message,
        string ContextType,
        Guid UserId) : IRequest<Result<SendMessageResponse>>;
    public record SendMessageCommand(string Message, string ContextType = "general");
}
