using CourseWork.Constants;
using CourseWork.Core.Data;
using CourseWork.Core.Models;
using CourseWork.Core.Models;
using CourseWork.Extensions;
using CourseWork.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseWork.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private const string CartKey = SessionConstants.CartKey;
        private readonly IHubContext<ShopHub> _hubContext;

        public CheckoutController(UnitOfWork unitOfWork, IHubContext<ShopHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
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

            var stockError = await ValidateStockAvailability(cart);
            if (stockError != null)
            {
                TempData["Error"] = stockError;
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var order = PrepareOrderEntity(orderModel);
                await SaveOrderWithDetails(order, cart);
                HttpContext.Session.Remove(CartKey);
                return View("Success");
            }
            catch (Exception)
            {
                TempData["Error"] = "Помилка при створенні замовлення. Спробуйте пізніше.";
                return RedirectToAction("Index", "Cart");
            }
        }
        private async Task<string?> ValidateStockAvailability(List<CartItem> cart)
        {
            foreach (var item in cart)
            {
                int availableStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(item.VariantId);
                if (item.Quantity > availableStock)
                {
                    return $"Товару '{item.ProductName}' недостатньо для завершення замовлення.";
                }
            }
            return null;
        }

        private Order PrepareOrderEntity(Order model)
        {
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int parsedId)) userId = parsedId;
            }

            return new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                StatusId = 1,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                DeliveryAddress = model.DeliveryAddress,
                DeliveryDate = DateTime.UtcNow.AddDays(4),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private async Task SaveOrderWithDetails(Order order, List<CartItem> cart)
        {
            await _unitOfWork.Orders.AddAsync(order);

            foreach (var item in cart)
            {
                await _unitOfWork.ProductBatches.DecreaseStockAsync(item.VariantId, item.Quantity);
                await _unitOfWork.SaveAsync();

                int newStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(item.VariantId);


                await _hubContext.Clients.All.SendAsync(
                    "ReceiveStockUpdate",
                    item.ProductId,
                    item.VariantId,
                    newStock
                );

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
            Console.WriteLine("Замовлення збережено");
        }
    }
}