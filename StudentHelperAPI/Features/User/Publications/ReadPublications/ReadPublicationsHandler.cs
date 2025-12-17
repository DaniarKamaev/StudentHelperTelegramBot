using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Publications.ReadPublications
{
    public class ReadPublicationsHandler : IRequestHandler<ReadPublicationsRequest, ReadPublicationsResponse>
    {
        private readonly HelperDbContext _db;
        public ReadPublicationsHandler(HelperDbContext db)
        {
            _db = db;
        }

        public async Task<ReadPublicationsResponse> Handle(ReadPublicationsRequest request, CancellationToken cancellationToken)
        {
            var pubications = await _db.Publications.ToListAsync(cancellationToken);
            if (pubications != null) 
                return new ReadPublicationsResponse(true, "Данные успешно получены", pubications);

            return new ReadPublicationsResponse(false, "Произошла ошибка", null);
        }
    }
}
