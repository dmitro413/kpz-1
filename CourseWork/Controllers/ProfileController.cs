using System.Security.Claims;
using CourseWork.Core.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseWork.Core.Models;

namespace CourseWork.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public ProfileController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(User model)
        {
            ModelState.Remove("Email");
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");

            var user = await GetCurrentUserAsync();
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.Phone = model.Phone;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = "Профіль оновлено.";
                return RedirectToAction("Index");
            }
            return View("Index", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                return View("Index", user);
            }

            var userDb = await GetCurrentUserAsync();

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, userDb.PasswordHash))
            {
                TempData["Error"] = "Старий пароль невірний.";
                return RedirectToAction("Index");
            }

            userDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _unitOfWork.Users.Update(userDb);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = "Пароль успішно змінено.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.Role == "Admin" || user.Role == "Manager")
            {
                TempData["Error"] = "Адміністратори та менеджери не можуть видаляти свій акаунт.";
                return RedirectToAction("Index");
            }

            var userOrders = await _unitOfWork.Orders.GetByUserIdAsync(user.UserId);

            bool hasActiveOrders = userOrders.Any(o => o.StatusId != 3 && o.StatusId != 4);

            if (hasActiveOrders)
            {
                TempData["Error"] = "Неможливо видалити акаунт: у вас є активні замовлення. Дочекайтеся їх виконання.";
                return RedirectToAction("Index");
            }

            try
            {
                foreach (var order in userOrders)
                {
                    order.UserId = null;
                    _unitOfWork.Orders.Update(order);
                }

                var userReviews = await _unitOfWork.Reviews.GetByUserIdAsync(user.UserId);

                foreach (var review in userReviews)
                {
                    _unitOfWork.Reviews.Remove(review);
                }

                _unitOfWork.Users.Remove(user);

                await _unitOfWork.SaveAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                TempData["Success"] = "Ваш акаунт та особисті дані успішно видалено.";
                return RedirectToAction("Index", "Shop");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Сталася помилка при видаленні даних. Спробуйте пізніше.";
                return RedirectToAction("Index");
            }
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return null;
            return await _unitOfWork.Users.GetByIdAsync(int.Parse(idStr));
        }
    }
}