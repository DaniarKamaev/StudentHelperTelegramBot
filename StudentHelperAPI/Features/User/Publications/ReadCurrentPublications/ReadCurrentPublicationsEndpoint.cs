using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace StudentHelperAPI.Features.User.Publications.ReadCurrentPublications
{
    public static class ReadCurrentPublicationsEndpoint
    {
        public static void ReadCurrentPublicationsMap(this IEndpointRouteBuilder app)
        {
            app.MapGet("helper/publications/{id}", async (
                Guid id,
                IMediator mediator,
                CancellationToken token) =>
            {
                var request = new ReadCurrentPublicationsRequest(id);
                var response = await mediator.Send(request, token);
                return Results.Ok(response);
            });
        }
    }
}
