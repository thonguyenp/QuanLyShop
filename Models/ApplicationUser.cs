using Microsoft.AspNetCore.Identity;

namespace QuanLyShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [ProtectedPersonalData]  // Đánh dấu dữ liệu cá nhân
        public virtual string? Fullname { get; set; }
    }

}
