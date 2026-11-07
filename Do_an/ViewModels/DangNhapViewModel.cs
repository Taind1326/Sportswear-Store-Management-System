using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email.")]
        [StringLength(254, ErrorMessage = "Giá trị nhập quá dài.")]
        [Display(Name = "Tên đăng nhập hoặc email")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        public string ReturnUrl { get; set; }
    }
}