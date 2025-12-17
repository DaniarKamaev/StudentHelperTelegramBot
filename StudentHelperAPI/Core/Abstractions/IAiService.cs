using StudentHelperAPI.Core.Common;

namespace StudentHelperAPI.Core.Abstractions
{
    public interface IAiService
    {
        Task<Result<string>> GetResponseAsync(string message, string contextType, CancellationToken cancellationToken = default);
    }
}