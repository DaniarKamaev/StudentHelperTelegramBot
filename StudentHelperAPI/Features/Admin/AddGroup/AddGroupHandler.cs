using MediatR;
using StudentHelperAPI.Features.Authentication;
using StudentHelperAPI.Features.Authentication.Auth;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.Admin.AddGroup
{
    public class AddGroupHandler : IRequestHandler<AddGroupRequest, AddGroupResponse>
    {
        private readonly HelperDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AddGroupHandler (HelperDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AddGroupResponse> Handle(AddGroupRequest request, CancellationToken cancellationToken)
        {

            string roul = AuthHelper.GetCurrentRole(_httpContextAccessor);
            if (roul == "admin")
            {
                var gruup = new StudentGroup
                {
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow
                };
                await _db.StudentGroups.AddAsync(gruup, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new AddGroupResponse(gruup.Id, true, "Группа успешно добавленна");
            }
            return new AddGroupResponse(null, false, "Группа не созданна");

        }
    }
}
