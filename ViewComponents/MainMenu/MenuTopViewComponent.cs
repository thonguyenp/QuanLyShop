using Microsoft.AspNetCore.Mvc;
using QuanLyShop.Models;
using QuanLyShop.Models.EF; // Thay bằng namespace thực tế của bạn
using System.Linq;
using System.Threading.Tasks;

public class MenuTopViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public MenuTopViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = _context.Categories.OrderBy(c => c.Position).ToList();
        return View(categories);
    }
}