using MediatR;

namespace StudentHelperAPI.Features.User.Publications.ReadPublications
{
    public record ReadPublicationsRequest() : IRequest<ReadPublicationsResponse>;
}
