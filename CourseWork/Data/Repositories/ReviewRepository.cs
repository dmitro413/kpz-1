using CourseWork.Data;
using CourseWork.Models;

namespace CourseWork.Repositories
{
    public class ReviewRepository : Repository<Review>
    {
        public ReviewRepository(MyDbContext context) : base(context)
        {
        }
    }
}