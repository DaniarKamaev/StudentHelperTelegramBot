using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StudentHelperAPI.Features.Admin.AddGroup
{
    public static class AddGroupEndpoint
    {
        public static void AddGroupMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("helper/admin/group/add", async (
                [FromBody] AddGroupRequest request,
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
