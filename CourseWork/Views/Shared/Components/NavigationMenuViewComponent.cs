using Microsoft.AspNetCore.Mvc;
using CourseWork.Core.Data;

namespace CourseWork.Views.Shared.Components
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly UnitOfWork _unitOfWork;

        public NavigationMenuViewComponent(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var types = await _unitOfWork.TypeOfProducts.GetAllAsync();

            ViewBag.SelectedType = RouteData?.Values["typeId"];

            return View(types);
        }
    }
}