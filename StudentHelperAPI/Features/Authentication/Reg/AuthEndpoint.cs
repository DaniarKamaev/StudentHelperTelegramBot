using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentHelperAPI.Core.Common;

namespace StudentHelperAPI.Features.Authentication.Reg
{
    public static class AuthEndpoint
    {
        public static void RegMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("helper/reg", async (
                [FromBody] AuthRequest request,
                IMediator mediator,
                CancellationToken token) => 
            {
                try
                {
                    var result = await mediator.Send(request, token);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });
        }
    }
}
