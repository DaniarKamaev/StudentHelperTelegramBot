using MediatR;
using StudentHelperAPI.Features.User.Publications.ReadPublications;

namespace StudentHelperAPI.Features.User.Publications.ReadPublications
{
    public static class ReadPublicationsEndpoint
    {
        public static void ReadPublicationsMap(this IEndpointRouteBuilder app)
        {
            app.MapGet("helper/publications", async (
                IMediator mediator,
                CancellationToken token) => 
            {
                var request = new ReadPublicationsRequest();
                var response = await mediator.Send(request, token);
                return Results.Ok(response);
            });
        }
    }
}
