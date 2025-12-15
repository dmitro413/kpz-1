using System.Security.Claims;
using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        private async Task<User?> GetCurrentUserAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return null;
            return await _unitOfWork.Users.GetByIdAsync(int.Parse(idStr));
        }
    }
}