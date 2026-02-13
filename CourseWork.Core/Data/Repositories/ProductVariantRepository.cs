using CourseWork.Core.Models;
using CourseWork.Core.Data;
using Microsoft.EntityFrameworkCore;
using CourseWork.Core.Repositories;

public class ProductVariantRepository : Repository<ProductVariant>
{
    public ProductVariantRepository(MyDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductVariant>> GetPagedAsync(int page, int pageSize)
    {
        return await _dbSet.Include(v => v.Product).Include(v => v.Weight)
            .OrderBy(v => v.Product.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
    public async Task<IEnumerable<ProductVariant>> GetVariantsWithDetailsAsync()
    {
        return await _dbSet
            .Include(v => v.Product)
            .Include(v => v.Weight)
            .OrderBy(v => v.Product.Name)
            .ToListAsync();
    }

    public async Task<int> GetTotalStockAsync(int variantId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        return await _context.ProductBatches
            .Where(b => b.VariantId == variantId && b.ExpiryDate > today)
            .SumAsync(b => b.Stock);

    }
    public async Task<(IEnumerable<ProductVariant> Items, int TotalCount)> GetFilteredVariantsAsync(
        int page, int pageSize, string searchTerm)
    {
        var query = _dbSet
        .Include(v => v.Product)
        .Include(v => v.Weight)
        .Where(v => !v.Product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(v => v.Product.Name.Contains(searchTerm));
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(v => v.Product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<List<ProductVariant>> GetByProductIdAsync(int productId)
    {
        return await _dbSet.IgnoreQueryFilters().Where(v => v.ProductId == productId).ToListAsync();
    }
}