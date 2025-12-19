using CourseWork.Data;
using CourseWork.Extensions;
using CourseWork.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseWork.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private const string CartKey = "Cart";

        public CheckoutController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            foreach (var item in cart)
            {
                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.VariantId);
                int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(item.VariantId);

                if (item.Quantity > availableStock)
                {
                    TempData["Error"] = $"Товару '{item.ProductName}' недостатньо на складі. Доступно: {availableStock} шт.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var model = new Order();

            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user != null)
                    {
                        model.CustomerName = user.FullName;
                        model.CustomerEmail = user.Email;
                        model.CustomerPhone = user.Phone;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(Order orderModel)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int parsedId))
                {
                    userId = parsedId;
                }
            }
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                StatusId = 1,
                CustomerName = orderModel.CustomerName,
                CustomerEmail = orderModel.CustomerEmail,
                CustomerPhone = orderModel.CustomerPhone,
                DeliveryAddress = orderModel.DeliveryAddress,
                DeliveryDate = DateTime.UtcNow.AddDays(4),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _unitOfWork.Orders.AddAsync(order);

                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        Order = order,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    await _unitOfWork.OrderDetails.AddAsync(detail);

                    await _unitOfWork.ProductBatches.DecreaseStockAsync(item.VariantId, item.Quantity);
                }

                await _unitOfWork.SaveAsync();

                HttpContext.Session.Remove(CartKey);

                return View("Success");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("не може бути від'ємною"))
                {
                    TempData["Error"] = "На жаль, товару на складі вже не вистачає. Хтось встиг купити його раніше. Будь ласка, перевірте кошик.";
                    return RedirectToAction("Index", "Cart");
                }

                ModelState.AddModelError("", "Помилка сервера: " + ex.Message);
                return View("Index", orderModel);
            }
        }
    }
}