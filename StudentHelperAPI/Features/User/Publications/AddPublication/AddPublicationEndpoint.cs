using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentHelperAPI.Core.Common;

namespace StudentHelperAPI.Features.User.Publications.AddPublication
{
    public static class AddPublicationEndpoint
    {
        public static void AddPublicationMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("helper/publications", async (
                [FromBody] AddPublicationRequest request,
                IMediator mediator,
                CancellationToken token) => 
            {
                try
                {
                    var response = await mediator.Send(request, token);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .RequireAuthorization();
        }
    }
}
