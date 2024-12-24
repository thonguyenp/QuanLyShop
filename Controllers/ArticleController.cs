using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using QuanLyShop.Models;
using QuanLyShop.Helper;
using QuanLyShop.Models.EF;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Common;
using X.PagedList.Extensions;

namespace QuanLyShop.Controllers
{
    public class ArticleController : Controller
    {
        private readonly AppDbContext _context;

        public ArticleController (AppDbContext context)
        {
            _context = context;
        }
        // GET: Article
        public async Task<IActionResult> Index(string txtSearch, int page = 1, int pageSize = 5)
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

        public ActionResult Detail(int id)
        {
            var item = _context.Posts.Find(id);
            return View(item);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}