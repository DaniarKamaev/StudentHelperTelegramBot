
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentHelperAPI.Models;
using System.Text.RegularExpressions;

namespace StudentHelperAPI.Features.Authentication.Reg
{
    public class AuthHandler : IRequestHandler<AuthRequest, AuthResponse>
    {
        private readonly HelperDbContext _db;
        private readonly IConfiguration _configuration;
        public AuthHandler(HelperDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Handle(AuthRequest request, CancellationToken cancellationToken)
        {
            Guid nullGroupId = Guid.Parse("08de3bb7-23ba-4bc0-8623-0e0ea69477bb");
            var group = await _db.StudentGroups.FirstOrDefaultAsync(x => x.Name == request.GrupId);

            Guid userGroupId;

            if (group == null)
            {
                userGroupId = nullGroupId;
            }
            else
            {
                userGroupId = group.Id;
            }

            var password = HashCreater.HashPassword(request.password);
            var user = new Models.User
            {
                Email = request.email,
                PasswordHash = password,
                LastName = request.lastName,
                FirstName = request.firstNamem,
                Role = "student",
                GroupId = userGroupId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _db.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            string token = AuthHelper.GenerateJwtToken(user, _configuration);

            return new AuthResponse(user.Id, true, "Пользователь успешно зарегистрирован", token);
        }
    }
}
