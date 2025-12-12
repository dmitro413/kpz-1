using CourseWork.Data;
using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Repositories
{
    public class Repository<T> where T : class
    {
        protected readonly MyDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(MyDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }



        public virtual void Remove(T entity)
        {
            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");

            if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
            {
  
                isDeletedProperty.SetValue(entity, true);

                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }




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
                .Include(od => od.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Brand)
                .Include(od => od.Variant).ThenInclude(v => v.ProductBatches)
                .Where(od => od.Variant.Product.Brand != null) 
                .Select(od => new
                {
                    BrandName = od.Variant.Product.Brand.BrandName,
                    quantity = od.Quantity,
                    unitPrice = od.UnitPrice,
                    avgPurchasePrice = od.Variant.ProductBatches.Any()
                                       ? od.Variant.ProductBatches.Average(b => b.PurchasePrice ?? 0)
                                       : 0
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
    }

    public class TypeOfProductRepository : Repository<TypeOfProduct> { public TypeOfProductRepository(MyDbContext c) : base(c) { } }
    public class WeightRepository : Repository<Weight> { public WeightRepository(MyDbContext c) : base(c) { } }
}