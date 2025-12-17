namespace StudentHelperAPI.Features.User.Publications.ReadCurrentPublications
{
    public record ReadCurrentPublicationsResponse(bool Success, string Message, Models.Publication? Publication);
}
