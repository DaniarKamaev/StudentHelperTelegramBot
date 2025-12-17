using MediatR;
using StudentHelperAPI.Features.Authentication;
using StudentHelperAPI.Models;

namespace StudentHelperAPI.Features.User.Publications.AddPublication
{
    public class AddPublicationHandler : IRequestHandler<AddPublicationRequest, AddPublicationResponse>
    {
        private readonly HelperDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AddPublicationHandler(HelperDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AddPublicationResponse> Handle(AddPublicationRequest request, CancellationToken cancellationToken)
        {
            var author_id = AuthHelper.GetCurrentAuthor_id(_httpContextAccessor);
            var post = new Publication
            {
                Title = request.title,
                Content = request.content,
                PublicationType = request.publication_type,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                AuthorId = author_id
            };
            var response = await _db.AddAsync(post, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AddPublicationResponse(post.Id, true, "Пост успешно добавлен");
        }
    }
}
