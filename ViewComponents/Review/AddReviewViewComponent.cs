using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QuanLyShop.Models.EF;
using Microsoft.CodeAnalysis;

namespace QuanLyShop.ViewComponents.Review
{
    [Authorize]
    public class AddReviewViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AddReviewViewComponent (AppDbContext context, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public async Task<IViewComponentResult> InvokeAsync(int? productId)
        {
            ViewBag.ProductId = productId;

            var item = new ReviewProduct();

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    item.Email = user.Email;
                    item.FullName = user.UserName; // Assuming UserName is used for FullName
                    item.UserName = user.UserName;
                }
            }
            return View(item);
        }
    }
}
