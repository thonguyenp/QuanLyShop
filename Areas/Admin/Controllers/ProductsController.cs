using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using QuanLyShop.Models.EF;
using X.PagedList.Extensions;

namespace QuanLyShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index(string txtSearch, string sort, int page = 1, int pageSize = 4)
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
            // Sắp xếp và chuyển đổi thành danh sách
            var appDbContext = await items.ToListAsync();

            // Phân trang
            var pageList = appDbContext.ToPagedList(page, pageSize);

            // Truyền từ khóa tìm kiếm hiện tại vào ViewBag
            ViewBag.txtSearch = txtSearch;
            ViewBag.pageSize = pageSize;
            return View(pageList);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()   //oke
        {
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]  //oke
        public async Task<IActionResult> Create([Bind("Id,Title,Alias,ProductCode,Description,Detail,OriginalPrice,Price,PriceSale,Quantity,ViewCount,IsHome,IsSale,IsFeature,IsHot,IsActive,ProductCategoryId,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.ModifiedDate = DateTime.Now;
                //Chuyển title tiếng việt có dấu thành alias (tiếng việt không dấu)
                product.Alias = Models.Common.Filter.FilterChar(product.Title);

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]  //oke
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Alias,ProductCode,Description,Detail,OriginalPrice,Price,PriceSale,Quantity,ViewCount,IsHome,IsSale,IsFeature,IsHot,IsActive,ProductCategoryId,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.Alias = Models.Common.Filter.FilterChar(product.Title);
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "Id", "Alias", product.ProductCategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
