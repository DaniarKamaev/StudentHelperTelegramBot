using MediatR;

namespace StudentHelperAPI.Features.Authentication.Reg
{
    public record AuthRequest(
        string email,
        string password,
        string firstNamem,
        string lastName,
        string GrupId) : IRequest<AuthResponse>;
}
