using MediatR;

namespace StudentHelperAPI.Features.Admin.AddLectureOnSubject
{
    public record AddLectureOnSubjectRequest(
        string title,
        string description,
        string external_url,
        string subject) : IRequest<AddLectureOnSubjectResponse>;
}
