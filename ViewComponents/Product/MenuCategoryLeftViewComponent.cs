using Microsoft.AspNetCore.Mvc;
using QuanLyShop.Models;
using QuanLyShop.Models.EF; // Thay bằng namespace thực tế của bạn
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyShop.ViewComponents.Product
{
    public class MenuCategoryLeftViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public MenuCategoryLeftViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? id)
        {
			if (id != null)
			{
				ViewBag.CateId = id;
			}
			var categories = _context.ProductCategories.ToList();
            return View(categories);
        }
    }
}
