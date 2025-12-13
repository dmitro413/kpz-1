using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseWork.Controllers
{
    public class HomeController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public HomeController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string searchString,  int? brandId, int? typeId, int page = 1,
            int? reportTopCount = null, int? reportLowStock = null, int? reportDaysExpiry = null)
        {
            int pageSize = 10;
            int actualTopCount = reportTopCount ?? 5;
            int actualLowStock = reportLowStock ?? 10;
            int actualDaysExpiry = reportDaysExpiry ?? 30;

            var productData = await _unitOfWork.Products.GetFilteredAsync(page, pageSize, searchString, brandId, typeId);

            var allBrands = await _unitOfWork.Brands.GetAllAsync();
            var allTypes = await _unitOfWork.TypeOfProducts.GetAllAsync();
            var topBrands = await _unitOfWork.Brands.GetTopBrandsByRevenueAsync(actualTopCount);
            var lowStock = await _unitOfWork.Products.GetLowStockProductsAsync(actualLowStock);
            var expiringBatches = await _unitOfWork.ProductBatches.GetExpiringBatchesSPAsync(actualDaysExpiry);


            var model = new DashboardViewModel
            {
                Products = productData.Items,
                TotalProducts = productData.TotalCount,

                Brands = await _unitOfWork.Brands.GetPagedAsync(page, pageSize),
                Variants = await _unitOfWork.ProductVariants.GetPagedAsync(page, pageSize),
                TotalBrands = await _unitOfWork.Brands.CountAsync(),
                TotalVariants = await _unitOfWork.ProductVariants.CountAsync(),
                Batches = await _unitOfWork.ProductBatches.GetPagedAsync(page, pageSize),
                TotalBatches = await _unitOfWork.ProductBatches.CountAsync(),
                Types = await _unitOfWork.TypeOfProducts.GetAllAsync(),
                TotalTypes = await _unitOfWork.TypeOfProducts.CountAsync(),

                CurrentPage = page,
                PageSize = pageSize,
                CurrentSearch = searchString,
                CurrentBrandId = brandId,
                CurrentTypeId = typeId,

                TopBrands = topBrands,
                ExpiringBatches = expiringBatches,

                LowStockProducts = lowStock,
                ReportTopCount = actualTopCount,
                ReportLowStockThreshold = actualLowStock,
                ReportDaysUntilExpiry = actualDaysExpiry,

                BrandList = new SelectList(allBrands, "BrandId", "BrandName", brandId),
                TypeList = new SelectList(allTypes, "TypeOfProductId", "TypeOfProductName", typeId)
            };

            return View(model);
        }
    }
}