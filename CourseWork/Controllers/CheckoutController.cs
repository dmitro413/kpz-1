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

            if (cart.Any(x => x.Quantity <= 0 || x.Price < 0))
            {
                HttpContext.Session.Remove(CartKey);
                TempData["Error"] = "Некоректні дані в кошику. Кошик очищено.";
                return RedirectToAction("Index", "Shop");
            }

            bool hasErrors = false;
            for (int i = 0; i < cart.Count; i++)
            {
                var item = cart[i];
                int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(item.VariantId);

                if (item.Quantity > availableStock)
                {
                    hasErrors = true;
                    if (availableStock > 0)
                    {
                        TempData["Error"] = $"Товару '{item.ProductName}' недостатньо. Кількість змінено на {availableStock} шт.";
                        item.Quantity = availableStock;
                    }
                    else
                    {
                        TempData["Error"] = $"Товар '{item.ProductName}' закінчився.";
                        item.Quantity = 0;
                    }
                }
            }

            if (hasErrors)
            {
                cart.RemoveAll(x => x.Quantity == 0);
                HttpContext.Session.SetObject(CartKey, cart);
                return RedirectToAction("Index", "Cart");
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
            ViewBag.CartTotalCount = cart.Sum(x => x.Quantity);
            ViewBag.CartTotalPrice = cart.Sum(x => x.TotalPrice);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(Order orderModel)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey);
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            for (int i = cart.Count - 1; i >= 0; i--)
            {
                var item = cart[i];
                if (item.Quantity <= 0)
                {
                    cart.RemoveAt(i);
                    HttpContext.Session.SetObject(CartKey, cart);
                    TempData["Error"] = "Некоректна кількість товару.";
                    return RedirectToAction("Index", "Cart");
                }

                int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(item.VariantId);
                if (item.Quantity > availableStock)
                {
                    TempData["Error"] = $"Товару '{item.ProductName}' недостатньо для завершення замовлення.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int parsedId)) userId = parsedId;
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
                    await _unitOfWork.ProductBatches.DecreaseStockAsync(item.VariantId, item.Quantity);

                    var detail = new OrderDetail
                    {
                        Order = order,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    await _unitOfWork.OrderDetails.AddAsync(detail);
                }

                await _unitOfWork.SaveAsync();
                HttpContext.Session.Remove(CartKey);
                return View("Success");
            }
            catch (Exception)
            {
                TempData["Error"] = "Помилка при створенні замовлення. Можливо, товар закінчився.";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}