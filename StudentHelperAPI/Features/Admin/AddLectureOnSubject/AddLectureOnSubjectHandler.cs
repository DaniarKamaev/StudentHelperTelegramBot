using MediatR;
using StudentHelperAPI.Features.Authentication;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.Admin.AddLectureOnSubject
{
    public class AddLectureOnSubjectHandler : IRequestHandler<AddLectureOnSubjectRequest, AddLectureOnSubjectResponse>
    {
        private readonly HelperDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AddLectureOnSubjectHandler(HelperDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AddLectureOnSubjectResponse> Handle(AddLectureOnSubjectRequest request, CancellationToken cancellationToken)
        {
            var role = AuthHelper.GetCurrentRole(_httpContextAccessor);
            if (role != "admin")
                return new AddLectureOnSubjectResponse(null, false, "У тебя нет прав");

            var user = AuthHelper.GetCurrentUserId(_httpContextAccessor);

            var lecture = new Lecture
            {
                Title = request.title,
                Description = request.description,
                ExternalUrl = request.external_url,
                Subject = request.subject,
                CreatedBy = user,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Lectures.AddAsync(lecture, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new AddLectureOnSubjectResponse(lecture.Id, true, "Лекция успешно добавлена");
        }
    }
}
