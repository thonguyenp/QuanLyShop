using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using QuanLyShop.Models.EF;
using X.PagedList.Extensions;

namespace QuanLyShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string txtSearch, string sort,int page = 1, int pageSize = 4)
        {
			// Lấy danh sách các bài viết, sắp xếp giảm dần theo Id
			var items = _context.Products
			   .Include(p => p.ProductImage)
				.Include(p => p.ProductCategory).AsQueryable();

			// Tìm kiếm nếu có từ khóa
			if (!string.IsNullOrEmpty(txtSearch))
			{
				items = items.Where(x => x.Alias.Contains(txtSearch) || x.Title.Contains(txtSearch));
			}

			// Sắp xếp dựa trên giá trị 'sort'
			if (!string.IsNullOrEmpty(sort))
			{
				switch (sort)
				{
					case "asc_price":
						items = items.OrderBy(x => x.Price);
						break;
					case "desc_price":
						items = items.OrderByDescending(x => x.Price);
						break;
					case "a_to_z":
						items = items.OrderBy(x => x.Title);
						break;
					case "z_to_a":
						items = items.OrderByDescending(x => x.Title);
						break;
					default:
						// Không thay đổi nếu không có tham số sắp xếp
						break;
				}
			}

			// Sắp xếp và chuyển đổi thành danh sách
			var appDbContext = await items.ToListAsync();

			// Phân trang
			var pageList = appDbContext.ToPagedList(page, pageSize);

			// Truyền từ khóa tìm kiếm hiện tại vào ViewBag
			ViewBag.txtSearch = txtSearch;
			ViewBag.pageSize = pageSize;
			return View(pageList);
        }

        public async Task<IActionResult> ProductCategory(string alias, int id, string txtSearch, int page = 1, int pageSize = 5)
        {
			// Lấy danh sách các bài viết, sắp xếp giảm dần theo Id
			var items = _context.Products
			   .Include(p => p.ProductImage)
				.Include(p => p.ProductCategory).AsQueryable();

			// Tìm kiếm nếu có từ khóa
			if (!string.IsNullOrEmpty(txtSearch))
			{
				items = items.Where(x => x.Alias.Contains(txtSearch) || x.Title.Contains(txtSearch));
			}
			if (id > 0)
			{
				items = items.Where(x => x.ProductCategoryId == id);
			}
			var cate = _context.ProductCategories.Find(id);
			if (cate != null)
			{
				ViewBag.CateName = cate.Title;
			}

			// Sắp xếp và chuyển đổi thành danh sách
			var appDbContext = await items.ToListAsync();

			// Phân trang
			var pageList = appDbContext.ToPagedList(page, pageSize);

			// Truyền từ khóa tìm kiếm hiện tại vào ViewBag
			ViewBag.txtSearch = txtSearch;

			return View(pageList);
		}

		// GET: Products/Details/5
		public async Task<IActionResult> Detail(string alias, int id)
		{
			var item = await _context.Products.Include(p => p.ProductCategory)
                .Include(p => p.ProductImage)
                .FirstOrDefaultAsync(m => m.Id == id); 
            if (item != null)
            {
                _context.Products.Attach(item);
				item.ViewCount = item.ViewCount + 1;
				_context.Entry(item).Property(x => x.ViewCount).IsModified = true;
				_context.SaveChanges();
			}
			var countReview = _context.Reviews.Where(x => x.ProductId == id).Count();
			ViewBag.CountReview = countReview;
			return View(item);
		}
    }
}
