using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Publications.ReadCurrentPublications
{
    public class ReadCurrentPublicationsHandler : IRequestHandler<ReadCurrentPublicationsRequest, ReadCurrentPublicationsResponse>
    {
        private readonly HelperDbContext _db;
        public ReadCurrentPublicationsHandler(HelperDbContext db)
        {
            _db = db;
        }
        public async Task<ReadCurrentPublicationsResponse> Handle(ReadCurrentPublicationsRequest request, CancellationToken cancellationToken)
        {
            var post = await _db.Publications
                .FirstOrDefaultAsync(x => x.Id == request.id, cancellationToken);
            if (post != null)
                return new ReadCurrentPublicationsResponse(true, "Пост успешно найден", post);
            return new ReadCurrentPublicationsResponse(false, "Пост не найден", null);
        }
    }
}
