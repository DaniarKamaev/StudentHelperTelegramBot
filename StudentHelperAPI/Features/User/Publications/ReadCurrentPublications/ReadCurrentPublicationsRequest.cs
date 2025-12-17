using MediatR;

namespace StudentHelperAPI.Features.User.Publications.ReadCurrentPublications
{
    public record ReadCurrentPublicationsRequest(Guid id) : IRequest<ReadCurrentPublicationsResponse>;
}
