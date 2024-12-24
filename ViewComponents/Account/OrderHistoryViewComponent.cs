using QuanLyShop.Models;
using QuanLyShop.Models.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace QuanLyShop.ViewComponents.Account
{
    public class OrderHistoryViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public OrderHistoryViewComponent(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //Nếu người dùng đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                // Lấy thông tin người dùng hiện tại
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    // Lấy danh sách đơn hàng theo CustomerId
                    var items = await _context.Orders
                        .Where(x => x.Email == user.Email)
                        .ToListAsync();

                    return View(items);
                }
            }

            // Trả về view rỗng nếu không có dữ liệu hoặc người dùng chưa đăng nhập
            return View();
        }

    }
}
