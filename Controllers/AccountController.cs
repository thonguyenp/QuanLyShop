using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyShop.Models;
using QuanLyShop;
using Microsoft.AspNetCore.Authentication;
using System.Data;
using QuanLyShop.Common;

namespace QuanLyShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        // Constructor sử dụng Dependency Injection
        public AccountController(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager, AppDbContext context, EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // Thuộc tính chỉ đọc cho UserManager và SignInManager
        protected UserManager<IdentityUser> UserManager => _userManager;
        protected SignInManager<IdentityUser> SignInManager => _signInManager;

        // GET: /Account/Create
        public ActionResult Register()
        {
            // Tạo SelectList từ các vai trò
            return View();
        }


        // POST: /Account/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
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

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Uncomment the following lines if you want to sign the user in after registration.
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                AddErrors(result);
            }
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
            return RedirectToAction("Login", "Account"); // Hoặc điều hướng đến trang khác
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

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                _emailService.SendMail("ShopOnline", "Quên mật khẩu", "Bạn click vào <a href='" + callbackUrl + "'>link này</a> để reset mật khẩu", model.Email);
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [AllowAnonymous]
        public async Task<ActionResult> Profile()
        {
            //// Kiểm tra xem người dùng đã đăng nhập chưa
            if (User.Identity?.IsAuthenticated != true)
            {
                // Chuyển hướng về trang đăng nhập nếu không đăng nhập
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin tên người dùng từ Identity
            var userName = User.Identity.Name;

            // Tìm người dùng từ UserManager
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                // Trường hợp không tìm thấy người dùng
                return RedirectToAction("Login", "Account");
            }

            // Tạo ViewModel và gán dữ liệu
            var item = new CreateAccountViewModel
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                UserName = user.UserName
            };

            // Trả về View với dữ liệu
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostProfile(CreateAccountViewModel req)
        {
            var users = await _userManager.Users.Where(u => u.Email == req.Email).ToListAsync();

            if (users.Count > 1)
            {
                ModelState.AddModelError("", "Có nhiều tài khoản trùng email. Liên hệ quản trị viên để khắc phục.");
                return View(req);
            }
            else if (users.Count == 0)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản với email này.");
                return View(req);
            }
            //Vì lấy ra list với 1 email thì lấy cái đầu tiên cũng chính là duy nhất
            var user = users.First();
            //user.UserName = req.FullName;
            user.PhoneNumber = req.Phone;
            var rs = await _userManager.UpdateAsync(user);
            if (rs.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            // Xử lý nếu update không thành công
            foreach (var error in rs.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(req);
        }

    }
}
