using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using QuanLyShop.Models;
using QuanLyShop.Helper;
using QuanLyShop.Models.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyShop.ViewComponents.Cart
{
    public class InfoCheckOutViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public InfoCheckOutViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(OrderViewModel req)
        {
            return View(req);
        }
    }
}
