using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentHelperAPI.Features.User.Lectures.ReadLectureOnSubject;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Lectures.ReadLectureOnSubject
{
    public static class GetLectureEndpoint
    {
        public static void GetLectureMap(this IEndpointRouteBuilder app)
        {
            app.MapGet("helper/lectures/{subject}", async (
                [FromRoute] string subject,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(subject))
                {
                    return Results.BadRequest("Параметр не валиден");
                }

                try
                {
                    var request = new GetLectureRequest(subject);
                    var lectures = await mediator.Send(request, cancellationToken);

                    return Results.Ok(lectures);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithName("GetLecturesBySubject")
            .WithTags("Lectures")
            .Produces<IEnumerable<Lecture>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        } 
    }
}
