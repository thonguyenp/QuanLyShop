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

namespace QuanLyShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductCategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public ProductCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProductCategories.ToListAsync());
        }

        // GET: Admin/ProductCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ProductCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Alias,Description,Icon,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                productCategory.CreatedDate = DateTime.Now;
                productCategory.ModifiedDate = DateTime.Now;
                //Chuyển title tiếng việt có dấu thành alias (tiếng việt không dấu)
                productCategory.Alias = Models.Common.Filter.FilterChar(productCategory.Title);

                _context.Add(productCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return NotFound();
            }
            return View(productCategory);
        }

        // POST: Admin/ProductCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Alias,Description,Icon,SeoTitle,SeoDescription,SeoKeywords,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] ProductCategory productCategory, IFormFile ImageFile)
        {
            if (id != productCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Đường dẫn lưu file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    //// Xóa ảnh cũ nếu tồn tại
                    //if (!string.IsNullOrEmpty(productCategory.Icon))
                    //{
                    //    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productCategory.Icon.TrimStart('/'));
                    //    if (System.IO.File.Exists(oldFilePath))
                    //    {
                    //        System.IO.File.Delete(oldFilePath);
                    //    }
                    //}

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file lên server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn file vào cơ sở dữ liệu
                    productCategory.Icon = $"/Uploads/{fileName}";
                }
                try
                {
                    _context.Update(productCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductCategoryExists(productCategory.Id))
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
            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: Admin/ProductCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory != null)
            {
                _context.ProductCategories.Remove(productCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductCategoryExists(int id)
        {
            return _context.ProductCategories.Any(e => e.Id == id);
        }
    }
}
