using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ.")]
        [StringLength(45, ErrorMessage = "Họ tối đa 45 ký tự.")]
        public string Ho { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        [StringLength(45, ErrorMessage = "Tên tối đa 45 ký tự.")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(QuyTacNhapLieu.SoDienThoai,
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ 0987654321).")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [RegularExpression(QuyTacNhapLieu.MatKhau,
            ErrorMessage = "Mật khẩu từ 8 ký tự, gồm chữ hoa, chữ số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; }

        public bool DongYDieuKhoan { get; set; }

        public bool NhanThongBao { get; set; }

        public string ReturnUrl { get; set; }
    }
}