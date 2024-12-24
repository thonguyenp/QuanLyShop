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


namespace QuanLyShop.ViewComponents.Order
{
    public class ProductInOrderViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ProductInOrderViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            Console.WriteLine($"OrderId được truyền vào: {id}");
            var items = _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToList();
            Console.WriteLine($"Số lượng sản phẩm trong đơn hàng: {items.Count}");
            return View(items);
        }
    }
}
