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

        public async Task<(IEnumerable<Order> Items, int TotalCount)> GetFilteredOrdersAsync(
        int page, int pageSize, string searchTerm, int? statusId) 
        {
            var query = _dbSet
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Variant).ThenInclude(v => v.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o => o.CustomerName.Contains(searchTerm) ||
                                         o.CustomerPhone.Contains(searchTerm) ||
                                         o.OrderId.ToString() == searchTerm);
            }

            if (statusId.HasValue)
            {
                query = query.Where(o => o.StatusId == statusId.Value);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}