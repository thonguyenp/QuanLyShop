using Microsoft.AspNetCore.Mvc;
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
using QuanLyShop.Common;


namespace QuanLyShop.Controllers
{
	public class ShoppingCartController : Controller
	{
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;


        public ShoppingCartController(AppDbContext context, IWebHostEnvironment env,
            EmailService emailService, IConfiguration configuration)
		{
            _emailService = emailService;
            _context = context;
            _env = env;
            _configuration = configuration;
		}


		public IActionResult Index()
		{
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart != null && cart.Items.Any())
            {
                //Đưa cart vào ViewBag
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        
        public IActionResult Partial_Item_Cart_Delete()
        {
            //Nếu không xóa trong session => session còn lưu lại sp => không xóa được
            HttpContext.Session.Remove("Cart");

            return ViewComponent("ItemCart"); // Tên "ItemCart" trùng với tên class ItemCartViewComponent
        }

        public IActionResult Partial_Item_Cart_Update()
        {

            return ViewComponent("ItemCart"); // Tên "ItemCart" trùng với tên class ItemCartViewComponent
        }

        public ActionResult CheckOut()
        {
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart");
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel req)
        {
            var code = new { Success = false, Code = -1, Url = "" };
            if (ModelState.IsValid)
            {
                ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart");
                if (cart != null) 
                {
                    Order order = new Order();
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }));
                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.CreatedBy = req.Phone;
                    order.Modifiedby = "admin";
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    _context.Orders.Add(order);
                    _context.SaveChanges();
                    //Send mail cho khách hàng
                    var strSanPham = "";
                    var thanhtien = 0;
                    var TongTien = 0;
                    foreach (var sp in cart.Items)
                    {
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + sp.TotalPrice +"</td>";
                        strSanPham += "</tr>";
                        thanhtien += sp.Price * sp.Quantity;
                    }
                    TongTien = thanhtien;
                    string filePath = Path.Combine(_env.WebRootPath, "Content/templates/send2.html");
                    string contentCustomer = System.IO.File.ReadAllText(filePath);
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", thanhtien.ToString());
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", TongTien.ToString());
                    _emailService.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentCustomer, req.Email);
                    //send mail cho admin
                    string filePath2 = Path.Combine(_env.WebRootPath, "Content/templates/send1.html");
                    string contentAdmin = System.IO.File.ReadAllText(filePath2);
                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
                    contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
                    contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", thanhtien.ToString());
                    contentAdmin = contentAdmin.Replace("{{TongTien}}", TongTien.ToString());
                    _emailService.SendMail("ShopOnline", "Đơn hàng mới có: #" + order.Code, contentAdmin, _configuration["AppSettings:Email"]);
                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    HttpContext.Session.Remove("Cart");
                    code = new { Success = true, Code = 1, Url = "" };
                    return RedirectToAction("OrderSuccess", new { orderId = order.Code });
                }
            }
            return View(req);
        }

        public ActionResult OrderSuccess(string orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }


        public ActionResult ShowCount()
        {
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart");
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count });
            }
			else
				return Json(new { Count = 0 });
        }
        [HttpPost]
		public ActionResult AddToCart(int id, int quantity)
		{
			var code = new { Success = false, msg = "", code = -1, Count = 0 };
			var checkProduct = _context.Products
				.Include(p => p.ProductImage)
				.Include(p => p.ProductCategory)
				.FirstOrDefault(x => x.Id == id);
			if (checkProduct != null)
			{
				ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
				if (cart == null)
				{
					cart = new ShoppingCart();
				}
				ShoppingCartItem item = new ShoppingCartItem
				{
					ProductId = checkProduct.Id,
					ProductName = checkProduct.Title,
					CategoryName = checkProduct?.ProductCategory?.Title,
					Alias = checkProduct?.Alias,
					Quantity = quantity
				};
				if (checkProduct.ProductImage?.FirstOrDefault(x => x.IsDefault) != null)
				{
					item.ProductImg = checkProduct.ProductImage?.FirstOrDefault(x => x.IsDefault)?.Image;
				}
				item.Price = checkProduct.Price;
				if (checkProduct.PriceSale > 0)
				{
					item.Price = (int)checkProduct.PriceSale;
				}
				item.TotalPrice = item.Quantity * item.Price;
				cart.AddToCart(item, quantity);
				HttpContext.Session.Set("Cart", cart);
				code = new { Success = true, msg = "Thêm sản phẩm vào giở hàng thành công!", code = 1, Count = cart.Items.Count };
			}
			return Json(code);
		}

        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                HttpContext.Session.Set("Cart", cart);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    HttpContext.Session.Set("Cart", cart);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }
        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = HttpContext.Session.Get<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
    }
}
