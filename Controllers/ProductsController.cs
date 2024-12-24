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

		//// GET: Products/Create
		//public IActionResult Create()
  //      {
  //          ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias");
  //          return View();
  //      }

  //      // POST: Products/Create
  //      // To protect from overposting attacks, enable the specific properties you want to bind to.
  //      // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  //      [HttpPost]
  //      [ValidateAntiForgeryToken]
  //      public async Task<IActionResult> Create([Bind("Id,Title,Alias,ProductCode,Description,Detail,Image,OriginalPrice,Price,PriceSale,Quantity,ViewCount,IsHome,IsSale,IsFeature,IsHot,IsActive,ProductCategoryId,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Product product)
  //      {
  //          if (ModelState.IsValid)
  //          {
  //              _context.Add(product);
  //              await _context.SaveChangesAsync();
  //              return RedirectToAction(nameof(Index));
  //          }
  //          ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
  //          return View(product);
  //      }

  //      // GET: Products/Edit/5
  //      public async Task<IActionResult> Edit(int? id)
  //      {
  //          if (id == null)
  //          {
  //              return NotFound();
  //          }

  //          var product = await _context.Products.FindAsync(id);
  //          if (product == null)
  //          {
  //              return NotFound();
  //          }
  //          ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
  //          return View(product);
  //      }

  //      // POST: Products/Edit/5
  //      // To protect from overposting attacks, enable the specific properties you want to bind to.
  //      // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  //      [HttpPost]
  //      [ValidateAntiForgeryToken]
  //      public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Alias,ProductCode,Description,Detail,Image,OriginalPrice,Price,PriceSale,Quantity,ViewCount,IsHome,IsSale,IsFeature,IsHot,IsActive,ProductCategoryId,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Product product)
  //      {
  //          if (id != product.Id)
  //          {
  //              return NotFound();
  //          }

  //          if (ModelState.IsValid)
  //          {
  //              try
  //              {
  //                  _context.Update(product);
  //                  await _context.SaveChangesAsync();
  //              }
  //              catch (DbUpdateConcurrencyException)
  //              {
  //                  if (!ProductExists(product.Id))
  //                  {
  //                      return NotFound();
  //                  }
  //                  else
  //                  {
  //                      throw;
  //                  }
  //              }
  //              return RedirectToAction(nameof(Index));
  //          }
  //          ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
  //          return View(product);
  //      }

  //      // GET: Products/Delete/5
  //      public async Task<IActionResult> Delete(int? id)
  //      {
  //          if (id == null)
  //          {
  //              return NotFound();
  //          }

  //          var product = await _context.Products
  //              .Include(p => p.ProductCategory)
  //              .FirstOrDefaultAsync(m => m.Id == id);
  //          if (product == null)
  //          {
  //              return NotFound();
  //          }

  //          return View(product);
  //      }

  //      // POST: Products/Delete/5
  //      [HttpPost, ActionName("Delete")]
  //      [ValidateAntiForgeryToken]
  //      public async Task<IActionResult> DeleteConfirmed(int id)
  //      {
  //          var product = await _context.Products.FindAsync(id);
  //          if (product != null)
  //          {
  //              _context.Products.Remove(product);
  //          }

  //          await _context.SaveChangesAsync();
  //          return RedirectToAction(nameof(Index));
  //      }

  //      private bool ProductExists(int id)
  //      {
  //          return _context.Products.Any(e => e.Id == id);
  //      }
    }
}
