using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using QuanLyShop;
using Microsoft.AspNetCore.Authentication;

namespace QuanLyShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        // Constructor sử dụng Dependency Injection
        public AccountController(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
        }

        // Thuộc tính chỉ đọc cho UserManager và SignInManager
        protected UserManager<IdentityUser> UserManager => _userManager;
        protected SignInManager<IdentityUser> SignInManager => _signInManager;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Users.ToListAsync();
            return View(items);
        }

        // GET: /Account/Create
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            // Tạo SelectList từ các vai trò
            return View();
        }


        // POST: /Account/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Thêm lỗi vào ModelState
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng sử dụng email khác.");
                    return View(model); // Trả về view với lỗi
                }

                // Kiểm tra xem UserName đã tồn tại chưa
                var existingUserNameUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserNameUser != null)
                {
                    ModelState.AddModelError("UserName", "Tên tài khoản này đã tồn tại. Vui lòng chọn tên tài khoản khác.");
                    return View(model);
                }

                var user = new IdentityUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (model.Roles != null)
                    {
                        foreach (var role in model.Roles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }

                    // Uncomment the following lines if you want to sign the user in after registration.
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Account");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            var rolesList = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            ViewBag.Role = rolesList;
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            returnUrl = HttpContext.Request.Query["returnUrl"];
            Console.WriteLine($"Return URL: {returnUrl}");
            ViewBag.ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl; // Mặc định về trang chủ
            return View();
            
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { returnUrl, rememberMe = model.RememberMe });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return RedirectToAction("Login", "Account", new { area = "Admin" }); // Hoặc điều hướng đến trang khác
        }

        // Phương thức helper để chuyển hướng về URL sau khi đăng nhập
        private ActionResult RedirectLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

		[HttpPost]
		public async Task<ActionResult> DeleteAccount(string user, string id)
		{
			var code = new { Success = false }; // Mặc định không xóa thành công.

			// Tìm người dùng theo tên
			var item = await _userManager.FindByNameAsync(user);
			if (item != null)
			{
				// Lấy danh sách các vai trò của người dùng
				var rolesForUser = await _userManager.GetRolesAsync(item);
				if (rolesForUser != null && rolesForUser.Any())
				{
					// Xóa người dùng khỏi tất cả vai trò
					foreach (var role in rolesForUser)
					{
						await _userManager.RemoveFromRoleAsync(item, role);
					}
				}

				// Xóa tài khoản người dùng
				var res = await _userManager.DeleteAsync(item);
				code = new { Success = res.Succeeded }; // Cập nhật trạng thái thành công hoặc thất bại
			}
			return Json(code);
		}

		public async Task<ActionResult> Edit(string id)
		{
			// Lấy thông tin người dùng
			var item = await _userManager.FindByIdAsync(id); // Thêm await để đợi kết quả
			if (item == null)
			{
				return NotFound(); // Trả về thông báo nếu người dùng không tồn tại
			}

			// Chuẩn bị dữ liệu để hiển thị
			var newUser = new EditAccountViewModel();
			var rolesForUser = await _userManager.GetRolesAsync(item); // Thêm await để đợi danh sách vai trò

			newUser.Email = item.Email;
			newUser.Phone = item.PhoneNumber; // Nếu `Phone` là `PhoneNumber` trong IdentityUser
			newUser.UserName = item.UserName;
			newUser.Roles = rolesForUser.ToList();

			// Gửi danh sách vai trò đến View
			ViewBag.Role = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

			return View(newUser); // Trả về View với dữ liệu đã chuẩn bị
		}

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu không hợp lệ, trả về lại View cùng với danh sách vai trò
                ViewBag.Role = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
                return View(model);
            }

            // Tìm người dùng
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return View(model);
            }

            // Cập nhật thông tin cơ bản
            user.Email = model.Email;
            user.PhoneNumber = model.Phone; // Đảm bảo sử dụng đúng thuộc tính PhoneNumber

            // Cập nhật người dùng
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                ViewBag.Role = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
                return View(model);
            }

            // Cập nhật vai trò
            if (model.Roles != null)
            {
                // Lấy danh sách vai trò hiện tại của người dùng
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Xóa các vai trò không còn trong danh sách mới
                var rolesToRemove = currentRoles.Except(model.Roles).ToList();
                foreach (var role in rolesToRemove)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                // Thêm các vai trò mới
                var rolesToAdd = model.Roles.Except(currentRoles).ToList();
                foreach (var role in rolesToAdd)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            return RedirectToAction("Index", "Account", new { area = "Admin" });
        }

    }
}
