using CourseWork.Core.Models;
using CourseWork.Core.Data;
using Microsoft.EntityFrameworkCore;


namespace CourseWork.Core.Repositories
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


    public class OrderDetailRepository : Repository<OrderDetail>
    {
        public OrderDetailRepository(MyDbContext context) : base(context) { }

        public async Task<List<OrderDetail>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }
    }

    public class OrderStatusRepository : Repository<Order> { public OrderStatusRepository(MyDbContext c) : base(c) { } }

    public class TypeOfProductRepository : Repository<TypeOfProduct> { public TypeOfProductRepository(MyDbContext c) : base(c) { } }
    public class WeightRepository : Repository<Weight> { public WeightRepository(MyDbContext c) : base(c) { } }
}