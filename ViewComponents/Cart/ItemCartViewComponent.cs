﻿using Microsoft.AspNetCore.Mvc;
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

namespace QuanLyShop.ViewComponents.Cart
{
    public class ItemCartViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ItemCartViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart != null && cart.Items.Any())
            {
                return View(cart.Items);
            }
            return View();
        }
    }
}