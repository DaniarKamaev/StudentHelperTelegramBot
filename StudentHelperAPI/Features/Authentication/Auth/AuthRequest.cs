using MediatR;

namespace StudentHelperAPI.Features.Authentication.Auth
{
    public record AuthRequest(
        string email,
        string password) : IRequest<AuthResponse>;
}
