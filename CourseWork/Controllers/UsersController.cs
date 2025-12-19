using CourseWork.Data;
using CourseWork.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public UsersController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Home/FormUser.cshtml", new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Введіть пароль");
            }
          

            if (ModelState.IsValid)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Користувача створено.";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("Email", "Цей Email вже існує.");
                }
            }
            return View("~/Views/Home/FormUser.cshtml", user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            
            user.PasswordHash = ""; 
            
            return View("~/Views/Home/FormUser.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(User user)
        {
            ModelState.Remove("PasswordHash");

           

            if (ModelState.IsValid)
            {
                var existing = await _unitOfWork.Users.GetByIdAsync(user.UserId);
                if (existing == null) return NotFound();

                if (existing.Email != user.Email)
                {
                    var userWithSameEmail = await _unitOfWork.Users.GetByEmailAsync(user.Email);
                    if (userWithSameEmail != null)
                    {
                        ModelState.AddModelError("Email", "Цей Email вже зайнятий.");
                        return View("~/Views/Home/FormUser.cshtml", user);
                    }
                }
                existing.FullName = user.FullName;
                existing.Email = user.Email;
                existing.Phone = user.Phone;
                existing.UpdatedAt = DateTime.UtcNow;

                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (currentUserIdStr == null || int.Parse(currentUserIdStr) != user.UserId)
                {
                    existing.Role = user.Role;
                }

                try
                {
                    _unitOfWork.Users.Update(existing);
                    await _unitOfWork.SaveAsync();

                    TempData["Success"] = "Користувача успішно оновлено.";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Помилка бази даних.");
                }
            }
            return View("~/Views/Home/FormUser.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out int currentUserId))
            {
                if (currentUserId == id)
                {
                    TempData["Error"] = "Ви не можете видалити власний акаунт.";
                    return RedirectToAction("Index", "Home");
                }
            }


            var userOrders = await _unitOfWork.Orders.GetByUserIdAsync(id);
            bool hasActiveOrders = userOrders.Any(o => o.StatusId != 3 && o.StatusId != 4);
            if (hasActiveOrders)
            {
                TempData["Error"] = "Неможливо видалити користувача: у нього є замовлення в обробці або доставці.";
                return RedirectToAction("Index", "Home");
            }
            
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user != null)
            {
                try
                {
                    foreach (var order in userOrders)
                    {
                        order.UserId = null;
                        _unitOfWork.Orders.Update(order);
                    }
                    var userReviews = await _unitOfWork.Reviews.GetByUserIdAsync(id);
                    foreach (var review in userReviews)
                    {
                        _unitOfWork.Reviews.Remove(review);
                    }

                    _unitOfWork.Users.Remove(user);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Користувача видалено.";
                }
                catch
                {
                    TempData["Error"] = "Неможливо видалити: у користувача є замовлення.";
                }
            }
            return RedirectToAction("Index", "Home");
        }

    }
}