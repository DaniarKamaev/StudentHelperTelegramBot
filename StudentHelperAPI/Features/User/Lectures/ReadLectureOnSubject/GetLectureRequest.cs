using MediatR;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Lectures.ReadLectureOnSubject
{
    public record GetLectureRequest(string subject) : IRequest<IEnumerable<Lecture>>;
        /*
          int Id,
        string title,
        string description,
        string url,
        string subject,
        string created_by,
        DateTime created_at
        */
}
