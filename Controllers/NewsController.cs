using X.PagedList.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyShop.Models;
using QuanLyShop.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLyShop.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;

        public NewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: News
        public async Task<IActionResult> Index(string txtSearch, int page = 1, int pageSize = 2)
        {
            // Lấy danh sách các bài viết, sắp xếp giảm dần theo Id
            var items = _context.News.AsQueryable();

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
            var item = _context.News.Find(id);
            return View(item);
        }
    }
}