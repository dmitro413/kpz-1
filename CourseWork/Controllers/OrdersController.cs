using CourseWork.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")] 
    public class OrdersController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public OrdersController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, int statusId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order != null)
            {
                order.StatusId = statusId;      
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = $"Статус замовлення #{orderId} оновлено.";
            }
            return RedirectToAction("Index", "Home");
        }
    }
}