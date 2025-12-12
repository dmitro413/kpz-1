using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        private readonly MyDbContext _context;

        public ProductRepository(MyDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllWithIncludesAsync()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<Product?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted) 
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
             int page,
             int pageSize,
             string? searchString = null,
             int? brandId = null,
             int? typeId = null)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => p.Name.Contains(searchString));

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (typeId.HasValue)
                query = query.Where(p => p.TypeOfProductId == typeId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted) 
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public new async Task<int> CountAsync()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .CountAsync();
        }
        public async Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllWithIncludesAsync();

            return await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Where(p => p.Name.Contains(searchTerm))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByBrandIdAsync(int brandId)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted) 
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Where(p => p.BrandId == brandId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByTypeIdAsync(int typeId)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted) 
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Where(p => p.TypeOfProductId == typeId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllIncludingDeletedAsync()
        {
            return await _context.Products
                .IgnoreQueryFilters() 
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
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
        public async Task<Product?> GetByIdWithFullDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Include(p => p.ProductVariants).ThenInclude(v => v.Weight)
                .Include(p => p.Reviews).ThenInclude(r => r.User) 
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<IEnumerable<Product>> GetShopProductsAsync(
    string searchString, int? brandId, int? typeId, string sortOrder)
        {
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

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.ProductVariants.Min(v => v.Price)),
                "price_desc" => query.OrderByDescending(p => p.ProductVariants.Min(v => v.Price)),
                "rating" => query.OrderByDescending(p => p.AggregateRating),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetByBrandAndRatingSP(int brandId, int minRating)
        {
            var productsFromSp = await _context.Products
                .FromSqlRaw("EXEC dbo.sp_GetProductsByBrandAndRating @BrandID={0}, @MinRating={1}", brandId, minRating)
                .ToListAsync();

            if (!productsFromSp.Any()) return new List<Product>();
            var productIds = productsFromSp.Select(p => p.ProductId).ToList();

            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.TypeOfProduct)
                .Include(p => p.ProductVariants).ThenInclude(v => v.Weight)
                .Include(p => p.ProductVariants).ThenInclude(v => v.ProductBatches) 
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();
        }

        public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold)
        {
            return await _context.LowStockProducts
                .FromSqlInterpolated($"SELECT * FROM dbo.fn_GetLowStockProducts({threshold})")
                .ToListAsync();
        }
        public void HardDelete(Product product)
        {

            _dbSet.Remove(product);
        }
    }
}