using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;


namespace CourseWork.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(MyDbContext context) : base(context) { }

        public async Task<(IEnumerable<User> Items, int TotalCount)> GetFilteredUsersAsync(int page, int pageSize, string searchTerm)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) ||
                                         u.Email.Contains(searchTerm) ||
                                         u.Phone.Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}