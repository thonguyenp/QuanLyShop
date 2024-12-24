using Microsoft.AspNetCore.Mvc;
using QuanLyShop.Models;
using QuanLyShop.Models.EF; // Thay bằng namespace thực tế của bạn
using System.Linq;
using System.Threading.Tasks;

public class MenuCategoryViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public MenuCategoryViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = _context.ProductCategories.ToList();
        return View(categories);
    }
}