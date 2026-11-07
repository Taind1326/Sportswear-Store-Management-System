using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [RegularExpression(QuyTacNhapLieu.TenDangNhap,
            ErrorMessage = "Tên đăng nhập gồm 4-50 ký tự: chữ không dấu, số, dấu chấm hoặc gạch dưới.")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [RegularExpression(QuyTacNhapLieu.SoDienThoai,
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [RegularExpression(QuyTacNhapLieu.MatKhau,
            ErrorMessage = "Mật khẩu từ 6 ký tự, gồm cả chữ và số.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        public string XacNhanMatKhau { get; set; }

        public string ReturnUrl { get; set; }
    }
}