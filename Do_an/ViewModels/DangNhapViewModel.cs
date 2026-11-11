using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [StringLength(254, ErrorMessage = "Giá trị nhập quá dài.")]
        [Display(Name = "Email")]
        public string TenDangNhap { get; set; } // nhận email (hoặc tên đăng nhập của admin)

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        public bool GhiNho { get; set; }

        public string ReturnUrl { get; set; }
    }
}