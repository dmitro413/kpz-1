using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class ProductRepository : Repository<Product>
    {
    
        public ProductRepository(MyDbContext context) : base(context)
        {
        }
        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
             int page,
             int pageSize,
             string? searchString = null,
             int? brandId = null,
             int? typeId = null,
             bool showDeleted = false)
        {
            var query = showDeleted
                ? _context.Products.IgnoreQueryFilters().AsQueryable()
                : _context.Products.AsQueryable();

            query = query
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct);

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => p.Name.Contains(searchString));

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (typeId.HasValue)
                query = query.Where(p => p.TypeOfProductId == typeId.Value);

            query = query.OrderByDescending(p => p.UpdatedAt).ThenByDescending(p => p.ProductId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return false;

            product.IsDeleted = false;
            product.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(product);
            return true;
        }

        public void HardDelete(Product product)
        {
            _dbSet.Remove(product);
        }

        public async Task<Product?> GetByIdWithFullDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Weight)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductBatches)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetShopProductsAsync(
     string searchString, int? brandId, int? typeId, string sortOrder, int page, int pageSize, int? minRating)
        {
            var minDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Include(p => p.ProductVariants).ThenInclude(v => v.Weight)
                .Include(p => p.ProductVariants).ThenInclude(v => v.ProductBatches)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(p => p.Name.Contains(searchString));
            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);
            if (typeId.HasValue)
                query = query.Where(p => p.TypeOfProductId == typeId);
            if (minRating.HasValue)
                query = query.Where(p => p.AggregateRating >= minRating.Value);

            query = sortOrder switch
            {
                "price_asc" => query
                    .OrderByDescending(p => p.ProductVariants.Any(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate)))
                    .ThenBy(p => p.ProductVariants
                        .Where(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate))
                        .Select(v => (decimal?)v.Price).Min() ?? p.ProductVariants.Min(v => v.Price)),

                "price_desc" => query
                    .OrderByDescending(p => p.ProductVariants.Any(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate)))
                    .ThenByDescending(p => p.ProductVariants
                        .Where(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate))
                        .Select(v => (decimal?)v.Price).Max() ?? p.ProductVariants.Max(v => v.Price)),

                "rating" => query
                    .OrderByDescending(p => p.ProductVariants.Any(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate)))
                    .ThenByDescending(p => p.AggregateRating),

                "newest" => query
                    .OrderByDescending(p => p.ProductVariants.Any(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate)))
                    .ThenByDescending(p => p.CreatedAt),

                _ => query
                    .OrderByDescending(p => p.ProductVariants.Any(v => v.ProductBatches.Any(b => b.Stock > 0 && b.ExpiryDate > minDate)))
                    .ThenBy(p => p.Name)
            };

            int totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var query = await _context.ProductVariants
                .Include(v => v.Product).ThenInclude(p => p.Brand)
                .Include(v => v.ProductBatches)
                .Where(v => !v.Product.IsDeleted)
                .Select(v => new
                {
                    VariantId = v.VariantId,
                    Name = v.Product.Name + " (" + v.Weight.WeightValue + " " + v.Weight.Unit + ")",
                    BrandName = v.Product.Brand.BrandName,
                    TotalStock = v.ProductBatches
                        .Where(b => b.ExpiryDate > today)
                        .Sum(b => b.Stock)
                })
                .Where(x => x.TotalStock <= threshold)
                .ToListAsync();

            return query.Select(x => new LowStockProductDto
            {
                VariantId = x.VariantId,
                Name = x.Name,
                BrandName = x.BrandName,
                TotalStock = x.TotalStock
            }).OrderBy(x => x.TotalStock).ToList();
        }

        public async Task<Product?> GetByIdIncludingDeletedAsync(int id)
        {
            return await _context.Products
                .IgnoreQueryFilters() 
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }
    }
}