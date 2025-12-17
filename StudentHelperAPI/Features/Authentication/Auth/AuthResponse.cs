namespace StudentHelperAPI.Features.Authentication.Auth
{
    public record AuthResponse(string? Token, bool Success, string Message);
}
