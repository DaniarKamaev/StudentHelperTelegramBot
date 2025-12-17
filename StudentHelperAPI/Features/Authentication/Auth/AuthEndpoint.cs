using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StudentHelperAPI.Features.Authentication.Auth
{
    public static class AuthEndpoint
    {
        public static void AuthMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("helper/auth", async (
                [FromBody] AuthRequest reqest,
                IMediator mediator,
                CancellationToken token) => 
            {
                try
                {
                    var response = await mediator.Send(reqest, token);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
    }
}
