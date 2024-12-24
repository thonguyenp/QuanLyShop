using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using QuanLyShop.Models.Common;
using QuanLyShop.Models.EF;
using QuanLyShop.Services;

namespace QuanLyShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
			var items = _context.Products.
                Include(p => p.ProductCategory).
                Include(p => p.ProductImage).
				Where(x => x.IsSale && x.IsActive).
				ToList();
            return View(items);
		}


    }
}
