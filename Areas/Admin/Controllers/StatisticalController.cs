using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using System.Globalization;

namespace QuanLyShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("admin")]
    public class StatisticalController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticalController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetStatistical(string fromDate, string toDate)
        {
            // Truy vấn cơ bản từ các bảng
            var query = from o in _context.Orders
                        join od in _context.OrderDetails on o.Id equals od.OrderId
                        join p in _context.Products on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };

            // Lọc theo ngày bắt đầu nếu có
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
            {
                query = query.Where(x => x.CreatedDate >= startDate);
            }

            // Lọc theo ngày kết thúc nếu có
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                query = query.Where(x => x.CreatedDate < endDate.AddDays(1)); // Bao gồm cả ngày cuối cùng
            }

            // Nhóm dữ liệu theo ngày và tính toán
            var result = query
                .GroupBy(x => x.CreatedDate.Date) // Lấy phần ngày, bỏ qua giờ phút giây
                .Select(x => new
                {
                    Date = x.Key,
                    TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                    TotalSell = x.Sum(y => y.Quantity * y.Price),
                })
                .Select(x => new
                {
                    Date = x.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DoanhThu = x.TotalSell,
                    LoiNhuan = x.TotalSell - x.TotalBuy
                }).ToList();

            return Json(new { Data = result });
        }
    }
}
