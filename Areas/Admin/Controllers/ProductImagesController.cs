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
    public class ProductImagesController : Controller
    {
        private readonly AppDbContext _context;

        public ProductImagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductImages
        public async Task<IActionResult> Index(string txtSearch, int page = 1, int pageSize = 5)
        {
            // Lấy danh sách các ProductImage, bao gồm thông tin Product liên quan
            var items = _context.ProductImages
                                .Include(pi => pi.Product) // Load thông tin Product
                                .AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc dữ liệu theo từ khóa
            if (!string.IsNullOrEmpty(txtSearch))
            {
                items = items.Where(x => x.Product.Title.Contains(txtSearch) || x.Product.Alias.Contains(txtSearch));
            }

            // Sắp xếp giảm dần theo Id và chuyển đổi thành danh sách
            var appDbContext = await items.OrderByDescending(x => x.Id).ToListAsync();

            // Phân trang
            var pageList = appDbContext.ToPagedList(page, pageSize);

            // Truyền từ khóa tìm kiếm hiện tại vào ViewBag
            ViewBag.txtSearch = txtSearch;

            return View(pageList);
        }

        // GET: Admin/ProductImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        // GET: Admin/ProductImages/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title");
            return View();
        }

        // POST: Admin/ProductImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,Image,IsDefault")] ProductImage productImage, IFormFile ImageFile)
        {
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

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file lên server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn file vào cơ sở dữ liệu
                    productImage.Image = $"/Uploads/{fileName}";
                }

                _context.Add(productImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", productImage.ProductId);
            return View(productImage);
        }

        // GET: Admin/ProductImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", productImage.ProductId);
            return View(productImage);
        }

        // POST: Admin/ProductImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,Image,IsDefault")] ProductImage productImage, IFormFile ImageFile)
        {
            if (id != productImage.Id)
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

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file lên server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn file vào cơ sở dữ liệu
                    productImage.Image = $"/Uploads/{fileName}";
                }

                try
                {
                    _context.Update(productImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductImageExists(productImage.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Title", productImage.ProductId);
            return View(productImage);
        }

        // GET: Admin/ProductImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImages
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        // POST: Admin/ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage != null)
            {
                _context.ProductImages.Remove(productImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductImageExists(int id)
        {
            return _context.ProductImages.Any(e => e.Id == id);
        }
    }
}
