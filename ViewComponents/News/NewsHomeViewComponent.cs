using QuanLyShop.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using QuanLyShop.Helper;
using QuanLyShop.Models.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyShop.ViewComponents.News
{
    public class NewsHomeViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public NewsHomeViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = _context.News.Take(3).ToList();
            return View(items);
        }
    }
}
