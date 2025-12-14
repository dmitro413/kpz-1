using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;


namespace CourseWork.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(MyDbContext context) : base(context) { }

        public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbSet
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}