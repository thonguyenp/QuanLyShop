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
	public class LoadReviewViewComponent :ViewComponent 
	{
		private readonly AppDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		public LoadReviewViewComponent(AppDbContext context, UserManager<IdentityUser> userManager,
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
			var item = _context.Reviews.Where(x => x.ProductId == productId).OrderByDescending(x => x.Id).ToList();
			ViewBag.Count = item.Count;
			return View(item);
		}
	}
}
