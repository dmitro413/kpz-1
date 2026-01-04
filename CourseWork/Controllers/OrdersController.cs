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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return NotFound();

            try
            {
                var details = await _unitOfWork.OrderDetails.GetByOrderIdAsync(id);

                foreach (var d in details)
                {
                    _unitOfWork.OrderDetails.Remove(d);
                }

                _unitOfWork.Orders.Remove(order);

                await _unitOfWork.SaveAsync();

                TempData["Success"] = $"Замовлення #{id} та всі його дані видалено.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Не вдалося видалити замовлення через технічну помилку.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}