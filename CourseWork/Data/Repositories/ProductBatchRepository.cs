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

            return await _context.ProductBatches
                .Include(b => b.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Brand)
                .Include(b => b.Variant).ThenInclude(v => v.Weight)
                .Where(b => b.ExpiryDate <= thresholdDate && b.Stock > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();
        }

        public async Task DecreaseStockAsync(int variantId, int quantityNeeded)
        {
            var batches = await _context.ProductBatches
                .Where(b => b.VariantId == variantId && b.Stock > 0)
                .OrderBy(b => b.ManufactureDate)
                .ToListAsync();

            int remaining = quantityNeeded;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                if (batch.Stock >= remaining)
                {
                    batch.Stock -= remaining;
                    remaining = 0;
                }
                else
                {
                    remaining -= batch.Stock;
                    batch.Stock = 0;
                }

                _context.ProductBatches.Update(batch);
            }

            if (remaining > 0)
            {
                throw new Exception($"Недостатньо товару на складі! Не вистачає {remaining} шт.");
            }
        }
    }
}