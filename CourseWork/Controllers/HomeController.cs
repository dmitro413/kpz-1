using CourseWork.Core.Models;
using CourseWork.Core.Data;
using CourseWork.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize(Roles = "Admin,Manager")]
public class HomeController : Controller
{
    private readonly UnitOfWork _unitOfWork;

    public HomeController(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(DashboardFilter filter)
    {
        const int pageSize = 10;

        //Отримання даних для Товарів
        var productData = await _unitOfWork.Products.GetFilteredAsync(
            filter.Page,
            pageSize,
            filter.SearchString,
            filter.BrandId,
            filter.TypeId,
            filter.ShowDeleted);

        //Отримання даних для Замовлень
        var (orderItems, orderTotal) = await _unitOfWork.Orders.GetFilteredOrdersAsync(
            filter.OrderPage,
            pageSize,
            filter.OrderSearch ?? "",
            filter.OrderStatusId);

        //Отримання даних для Користувачів
        var (userItems, userTotal) = await _unitOfWork.Users.GetFilteredUsersAsync(
            filter.UserPage,
            pageSize,
            filter.UserSearch ?? "");

        //Отримання варіантів з пошуком 
        var (variantItems, variantTotal) = await _unitOfWork.ProductVariants.GetFilteredVariantsAsync(
            filter.VariantPage,
            pageSize,
            filter.VariantSearch ?? "");

        //Налаштування та отримання Звітів (використовуємо значення з фільтра або дефолтні)
        int actualTopCount = filter.ReportTopCount ?? 5;
        int actualLowStock = filter.ReportLowStock ?? 10;
        int actualDaysExpiry = filter.ReportDaysExpiry ?? 30;

        var topBrands = await _unitOfWork.Brands.GetTopBrandsByRevenueAsync(actualTopCount);
        var lowStock = await _unitOfWork.Products.GetLowStockProductsAsync(actualLowStock);
        var expiringBatches = await _unitOfWork.ProductBatches.GetExpiringBatchesAsync(actualDaysExpiry);

        var allBrands = await _unitOfWork.Brands.GetAllAsync();
        var allTypes = await _unitOfWork.TypeOfProducts.GetAllAsync();

        //Формування ViewModel
        var model = new DashboardViewModel
        {
            // ТОВАРИ
            Products = productData.Items,
            TotalProducts = productData.TotalCount,
            ShowDeleted = filter.ShowDeleted,
            CurrentPage = filter.Page,
            CurrentSearch = filter.SearchString,
            CurrentBrandId = filter.BrandId,
            CurrentTypeId = filter.TypeId,

            // ЗАМОВЛЕННЯ
            Orders = orderItems,
            TotalOrders = orderTotal,
            OrderSearch = filter.OrderSearch,
            OrderCurrentPage = filter.OrderPage,
            CurrentOrderStatusId = filter.OrderStatusId,
            OrderTotalPages = (int)Math.Ceiling((double)orderTotal / pageSize),
            OrderStatuses = await _unitOfWork.OrderStatuses.GetAllAsync(),

            // ВАРІАНТИ
            Variants = variantItems,
            TotalVariants = variantTotal,
            VariantSearch = filter.VariantSearch,
            VariantCurrentPage = filter.VariantPage,
            VariantTotalPages = (int)Math.Ceiling((double)variantTotal / pageSize),

            // АНАЛІТИКА
            TopBrands = topBrands,
            LowStockProducts = lowStock,
            ExpiringBatches = expiringBatches,
            ReportTopCount = actualTopCount,
            ReportLowStockThreshold = actualLowStock,
            ReportDaysUntilExpiry = actualDaysExpiry,

            // БРЕНДИ ТА ПАРТІЇ (використовуємо загальну сторінку з фільтра)
            Brands = await _unitOfWork.Brands.GetPagedAsync(filter.Page, pageSize),
            TotalBrands = await _unitOfWork.Brands.CountAsync(),

            Batches = await _unitOfWork.ProductBatches.GetPagedAsync(filter.Page, pageSize),
            TotalBatches = await _unitOfWork.ProductBatches.CountAsync(),

            // КОРИСТУВАЧІ
            Users = userItems,
            TotalUsers = userTotal,
            UserSearch = filter.UserSearch,

            // ВІДГУКИ
            Reviews = await _unitOfWork.Reviews.GetPagedAsync(filter.Page, pageSize),
            TotalReviews = await _unitOfWork.Reviews.CountAsync(),

            // ДАНІ БЕЗ ПАГІНАЦІЇ
            Types = allTypes,
            TotalTypes = allTypes.Count(),
            Weights = await _unitOfWork.Weights.GetAllAsync(),
            TotalWeights = await _unitOfWork.Weights.CountAsync(),

            // СПИСКИ ДЛЯ ДРОПДАУНІВ
            BrandList = new SelectList(allBrands, "BrandId", "BrandName", filter.BrandId),
            TypeList = new SelectList(allTypes, "TypeOfProductId", "TypeOfProductName", filter.TypeId),

            PageSize = pageSize
        };

        return View(model);
    }
}