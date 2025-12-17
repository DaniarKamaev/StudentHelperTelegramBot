using MediatR;

namespace StudentHelperAPI.Features.AI.Send
{
    public static class SendMessageEndpoint
    {
        public static void SendMessageMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("/helper/ai/chat", async (SendMessageCommand request, ISender sender) =>
            {
                var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                var validContextTypes = new[] { "math", "programming", "lectures", "general" };
                var contextType = validContextTypes.Contains(request.ContextType?.ToLower())
                    ? request.ContextType.ToLower()
                    : "general";

                var command = new SendMessageRequest(
                    Message: request.Message,
                    ContextType: contextType,
                    UserId: userId
                );

                var result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(new { error = result.Error });
            })
            .RequireAuthorization()
            .WithName("SendMessage")
            .WithOpenApi();
        }
    }
}
