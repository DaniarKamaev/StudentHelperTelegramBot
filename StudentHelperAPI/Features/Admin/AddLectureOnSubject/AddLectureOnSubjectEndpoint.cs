using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StudentHelperAPI.Features.Admin.AddLectureOnSubject
{
    public static class AddLectureOnSubjectEndpoint
    {
        public static void AddLectureOnSubjectMap(this IEndpointRouteBuilder app)
        {
            app.MapPost("helper/admin/lecture/add", async (
                [FromBody] AddLectureOnSubjectRequest request,
                IMediator mediator,
                CancellationToken token) => 
            {
                var response = await mediator.Send(request, token);
                return Results.Ok(response);
            })
            .RequireAuthorization();
        }
    }
}
