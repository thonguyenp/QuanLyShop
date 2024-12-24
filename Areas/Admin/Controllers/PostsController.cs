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
    public class PostsController : Controller
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Posts
        public async Task<IActionResult> Index(string txtSearch,int page = 1, int pageSize = 5)
        {
            // Lấy danh sách các bài viết, sắp xếp giảm dần theo Id
            var items = _context.Posts.AsQueryable();

            // Tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(txtSearch))
            {
                items = items.Where(x => x.Alias.Contains(txtSearch) || x.Title.Contains(txtSearch));
            }

            // Sắp xếp và chuyển đổi thành danh sách
            var appDbContext = await items.OrderByDescending(x => x.Id).ToListAsync();

            // Phân trang
            var pageList = appDbContext.ToPagedList(page, pageSize);

            // Truyền từ khóa tìm kiếm hiện tại vào ViewBag
            ViewBag.txtSearch = txtSearch;

            return View(pageList);
        }

        // GET: Admin/Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posts == null)
            {
                return NotFound();
            }

            return View(posts);
        }

        // GET: Admin/Posts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title");
            return View();
        }

        // POST: Admin/Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Alias,Description,Detail,Image,CategoryId,SeoTitle,SeoDescription,SeoKeywords,IsActive,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Posts posts)
        {
            if (ModelState.IsValid)
            {
                posts.CreatedDate = DateTime.Now;
                posts.ModifiedDate = DateTime.Now;
                //Chuyển title tiếng việt có dấu thành alias (tiếng việt không dấu)
                posts.Alias = Models.Common.Filter.FilterChar(posts.Title);

                _context.Add(posts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", posts.CategoryId);
            return View(posts);
        }

        // GET: Admin/Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts.FindAsync(id);
            if (posts == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", posts.CategoryId);
            return View(posts);
        }

        // POST: Admin/Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Alias,Description,Detail,Image,CategoryId,SeoTitle,SeoDescription,SeoKeywords,IsActive,CreatedBy,CreatedDate,ModifiedDate,Modifiedby")] Posts posts)
        {
            if (id != posts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostsExists(posts.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Title", posts.CategoryId);
            return View(posts);
        }

        // GET: Admin/Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (posts == null)
            {
                return NotFound();
            }

            return View(posts);
        }

        // POST: Admin/Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var posts = await _context.Posts.FindAsync(id);
            if (posts != null)
            {
                _context.Posts.Remove(posts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostsExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
