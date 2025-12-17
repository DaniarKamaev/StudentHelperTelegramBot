using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Publications.ReadPublications
{
    public record ReadPublicationsResponse(bool Success, string Message, List<Publication>? Publications);
}
