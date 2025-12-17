using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using StudentHelperAPI.Models;
using System.Security.Claims;
using System.Text;

namespace StudentHelperAPI.Features.Authentication.Auth
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
            var userMain = await _db.Users
                .FirstOrDefaultAsync(x => x.Email == request.email, cancellationToken);

            if (userMain == null)
                return new AuthResponse(null, false, "Нет такого пользователя");

            bool isValidPassword = HashCreater.VerifyPassword(request.password, userMain.PasswordHash);
            if (isValidPassword)
            {
                var token = AuthHelper.GenerateJwtToken(userMain, _configuration);
                return new AuthResponse(token, true, "Вход успешо выполнен");
            }
            return new AuthResponse(null, false, "Неверный логин или пароль");
        }
        
    }
}
