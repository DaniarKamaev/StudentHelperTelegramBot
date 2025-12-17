using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentHelperAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudentHelperAPI.Features.User.Lectures.ReadLectureOnSubject
{
    public class GetLectureHandler : IRequestHandler<GetLectureRequest, IEnumerable<Lecture>>
    {
        private readonly HelperDbContext _context;

        public GetLectureHandler(HelperDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lecture>> Handle(GetLectureRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.subject))
            {
                throw new ArgumentException("Subject cannot be null or empty", nameof(request.subject));
            }

           
            var lectures = await _context.Lectures
                .AsNoTracking()
                .Where(l => l.Subject != null &&
                           l.Subject.ToLower() == request.subject.ToLower())
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);

            return lectures;
        }
    }
}