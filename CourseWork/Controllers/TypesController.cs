using CourseWork.Constants;
using CourseWork.Core.Models;
using CourseWork.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Controllers
{
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public class TypesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public TypesController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Home/FormType.cshtml", new TypeOfProduct());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TypeOfProduct type)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.TypeOfProducts.AddAsync(type);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = $"Тип '{type.TypeOfProductName}' створено.";
                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Такий тип продукту вже існує.");
                }
            }
            return View("~/Views/Home/FormType.cshtml", type);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _unitOfWork.TypeOfProducts.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View("~/Views/Home/FormType.cshtml", type);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TypeOfProduct type)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _unitOfWork.TypeOfProducts.GetByIdAsync(type.TypeOfProductId);
                    if (existing != null)
                    {
                        existing.TypeOfProductName = type.TypeOfProductName;
                        _unitOfWork.TypeOfProducts.Update(existing);
                        await _unitOfWork.SaveAsync();

                        TempData["Success"] = "Тип продукту оновлено.";
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("TypeOfProductName", "Такий тип продукту вже існує.");
                }
            }
            return View("~/Views/Home/FormType.cshtml", type);
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _unitOfWork.TypeOfProducts.GetByIdAsync(id);
            if (type != null)
            {
                try
                {
                    _unitOfWork.TypeOfProducts.Remove(type);
                    await _unitOfWork.SaveAsync();
                    TempData["Success"] = "Тип продукту видалено.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "Неможливо видалити: цей тип використовується в товарах.";
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}