using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class ReviewRepository : Repository<Review>
    {
        public ReviewRepository(MyDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbSet
                .Include(r => r.User)    
                .Include(r => r.Product) 
                .OrderByDescending(r => r.CreatedAt) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}