using MediatR;

namespace StudentHelperAPI.Features.User.Publications.AddPublication
{
    public record AddPublicationRequest(
        string title,
        string content,
        string publication_type) : IRequest<AddPublicationResponse>;
}
