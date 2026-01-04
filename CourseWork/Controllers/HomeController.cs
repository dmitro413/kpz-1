using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using CourseWork.Repositories;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class HomeController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public HomeController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(
            string searchString, int? brandId, int? typeId, bool showDeleted, int page = 1,
            string orderSearch = "", int? orderStatusId = null, int orderPage = 1,
            string variantSearch = "", int variantPage = 1,
            int? reportTopCount = null, int? reportLowStock = null, int? reportDaysExpiry = null,
             string userSearch = "", int userPage = 1)
        {
            const int pageSize = 10;

            // дані для товарів та замовлень
            var productData = await _unitOfWork.Products.GetFilteredAsync(page, pageSize, searchString, brandId, typeId, showDeleted);
            var (orderItems, orderTotal) = await _unitOfWork.Orders.GetFilteredOrdersAsync(orderPage, pageSize, orderSearch, orderStatusId);
            var (userItems, userTotal) = await _unitOfWork.Users.GetFilteredUsersAsync(userPage, pageSize, userSearch);

            // Отримання варіантів з пошуком 
            var (variantItems, variantTotal) = await _unitOfWork.ProductVariants.GetFilteredVariantsAsync(variantPage, pageSize, variantSearch);

            // Налаштування та отримання Звітів 
            int actualTopCount = reportTopCount ?? 5;
            int actualLowStock = reportLowStock ?? 10;
            int actualDaysExpiry = reportDaysExpiry ?? 30;

            var topBrands = await _unitOfWork.Brands.GetTopBrandsByRevenueAsync(actualTopCount);
            var lowStock = await _unitOfWork.Products.GetLowStockProductsAsync(actualLowStock);
            var expiringBatches = await _unitOfWork.ProductBatches.GetExpiringBatchesAsync(actualDaysExpiry);

            var allBrands = await _unitOfWork.Brands.GetAllAsync();
            var allTypes = await _unitOfWork.TypeOfProducts.GetAllAsync();

            var model = new DashboardViewModel
            {
                // ТОВАРИ
                Products = productData.Items,
                TotalProducts = productData.TotalCount,
                ShowDeleted = showDeleted,
                CurrentPage = page,
                CurrentSearch = searchString,
                CurrentBrandId = brandId,
                CurrentTypeId = typeId,

                //ЗАМОВЛЕННЯ
                Orders = orderItems,
                TotalOrders = orderTotal,
                OrderSearch = orderSearch,
                OrderCurrentPage = orderPage,
                CurrentOrderStatusId = orderStatusId,
                OrderTotalPages = (int)Math.Ceiling((double)orderTotal / pageSize),
                OrderStatuses = await _unitOfWork.OrderStatuses.GetAllAsync(),

                // ВАРІАНТИ
                Variants = variantItems,
                TotalVariants = variantTotal,
                VariantSearch = variantSearch,
                VariantCurrentPage = variantPage,
                VariantTotalPages = (int)Math.Ceiling((double)variantTotal / pageSize),

                // АНАЛІТИКА
                TopBrands = topBrands,
                LowStockProducts = lowStock,
                ExpiringBatches = expiringBatches,
                ReportTopCount = actualTopCount,
                ReportLowStockThreshold = actualLowStock,
                ReportDaysUntilExpiry = actualDaysExpiry,

                // інщі вкладки з загальною сторінкою page
                Brands = await _unitOfWork.Brands.GetPagedAsync(page, pageSize),
                TotalBrands = await _unitOfWork.Brands.CountAsync(),

                Batches = await _unitOfWork.ProductBatches.GetPagedAsync(page, pageSize),
                TotalBatches = await _unitOfWork.ProductBatches.CountAsync(),

                Users = userItems,
                TotalUsers = userTotal,
                UserSearch = userSearch,


                Reviews = await _unitOfWork.Reviews.GetPagedAsync(page, pageSize),
                TotalReviews = await _unitOfWork.Reviews.CountAsync(),

                // ДАНІ БЕЗ ПАГІНАЦІЇ
                Types = allTypes,
                TotalTypes = allTypes.Count(),
                Weights = await _unitOfWork.Weights.GetAllAsync(),
                TotalWeights = await _unitOfWork.Weights.CountAsync(),

                // СПИСКИ
                BrandList = new SelectList(allBrands, "BrandId", "BrandName", brandId),
                TypeList = new SelectList(allTypes, "TypeOfProductId", "TypeOfProductName", typeId),

                PageSize = pageSize
            };

            return View(model);
        }
    }
}