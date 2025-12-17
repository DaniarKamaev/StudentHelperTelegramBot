namespace StudentHelperAPI.Features.Authentication.Reg
{
    public record AuthResponse(Guid Id, bool Success, string Message, string? Token);
}
