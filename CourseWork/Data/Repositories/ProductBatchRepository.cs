using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class ProductBatchRepository : Repository<ProductBatch>
    {
        public ProductBatchRepository(MyDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductBatch>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbSet
                .Include(b => b.Variant).ThenInclude(v => v.Product)
                .Include(b => b.Variant).ThenInclude(v => v.Weight)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductBatch>> GetExpiringBatchesAsync(int days)
        {
            var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(days));

            return await _dbSet
                .Include(b => b.Variant).ThenInclude(v => v.Product)
                .Where(b => b.ExpiryDate <= thresholdDate && b.Stock > 0)
                .ToListAsync();
        }
    }
}