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
 
        public async Task<List<ExpiringBatchDto>> GetExpiringBatchesSPAsync(int days)
        {
            return await _context.ExpiringBatches
                .FromSqlInterpolated($"EXEC dbo.FindExpiringProductBatches @DaysUntilExpiry={days}")
                .ToListAsync();
        }
    }
}