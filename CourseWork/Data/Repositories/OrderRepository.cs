using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class OrderRepository : Repository<Order>
    {
        public OrderRepository(MyDbContext context) : base(context) { }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                 .IgnoreQueryFilters()
                .Include(o => o.Status) 
                .Include(o => o.OrderDetails) 
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Weight) 
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt) 
                .ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _dbSet
                .Include(o => o.Status)
                .Include(o => o.User) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .OrderByDescending(o => o.CreatedAt) 
                .ToListAsync();
        }
    }
}