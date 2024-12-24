using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuanLyShop.Models;

namespace QuanLyShop.ViewComponents
{
    public class MenuSaleViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public MenuSaleViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
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
