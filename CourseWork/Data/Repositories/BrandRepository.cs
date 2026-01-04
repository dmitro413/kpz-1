using CourseWork.Data;
using CourseWork.Models;
using CourseWork.Repositories;
using Microsoft.EntityFrameworkCore;

public class BrandRepository : Repository<Brand>
{
    public BrandRepository(MyDbContext context) : base(context) { }

    public async Task<IEnumerable<Brand>> GetPagedAsync(int page, int pageSize)
    {
        return await _dbSet.OrderBy(b => b.BrandName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
    public async Task<List<BrandStatsDto>> GetTopBrandsByRevenueAsync(int count)
    {
        var rawData = await _context.OrderDetails
            .IgnoreQueryFilters()
            .Include(od => od.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Brand)
            .Include(od => od.Variant).ThenInclude(v => v.ProductBatches)
            .Where(od => od.Variant.Product.Brand != null)
            .Select(od => new
            {
                BrandName = od.Variant.Product.Brand.BrandName,
                quantity = od.Quantity,
                unitPrice = od.UnitPrice,
                avgPurchasePrice = od.Variant.ProductBatches.Any()
                ? od.Variant.ProductBatches.Average(b => b.PurchasePrice ?? 0): 0
            })
            .ToListAsync();

        var stats = rawData
            .GroupBy(x => x.BrandName)
            .Select(g => new BrandStatsDto
            {
                BrandName = g.Key,
                SalesCount = g.Sum(x => x.quantity),
                TotalRevenue = g.Sum(x => x.unitPrice * x.quantity),

                EstimatedProfit = g.Sum(x => (x.unitPrice - x.avgPurchasePrice) * x.quantity)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(count)
            .ToList();

        return stats;
    }

}