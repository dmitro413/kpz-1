using CourseWork.Repositories;

namespace CourseWork.Data
{
    public class UnitOfWork : IDisposable
    {
        private readonly MyDbContext _context;

        public BrandRepository Brands { get; }
        public ProductRepository Products { get; }
        public ProductVariantRepository ProductVariants { get; }
        public TypeOfProductRepository TypeOfProducts { get; }
        public WeightRepository Weights { get; }
        public ProductBatchRepository ProductBatches { get; } 

        public ReviewRepository Reviews { get; }

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
            Brands = new BrandRepository(_context);
            Products = new ProductRepository(_context);
            ProductVariants = new ProductVariantRepository(_context);
            TypeOfProducts = new TypeOfProductRepository(_context);
            Weights = new WeightRepository(_context);
            ProductBatches = new ProductBatchRepository(_context); 
            Reviews = new ReviewRepository(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}