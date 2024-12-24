using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuanLyShop.Models;

namespace QuanLyShop.ViewComponents
{
	public class MenuArrivalProductViewComponent : ViewComponent
	{
		private readonly AppDbContext _context;

		public MenuArrivalProductViewComponent(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var items = _context.Products.Include(p => p.ProductImage).
				Where(x => x.IsHome && x.IsActive).
				ToList();
			return View(items);
		}
	}
}
