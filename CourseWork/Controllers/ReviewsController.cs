using CourseWork.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize] 
public class ReviewsController : Controller
{
    private readonly UnitOfWork _unitOfWork;

    public ReviewsController(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null) return NotFound();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int currentUserId)) return Forbid();

        bool isAdmin = User.IsInRole("Admin");
        if (review.UserId != currentUserId && !isAdmin)
        {
            return Forbid(); 
        }

        int productId = review.ProductId; 
        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveAsync();

        TempData["Success"] = "Відгук видалено.";

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Details", "Shop", new { id = productId });
    }
}