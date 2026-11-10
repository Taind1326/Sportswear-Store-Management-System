using System;
using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public static class QuyTacNhapLieu
    {
        public const string TenDangNhap = @"^[A-Za-z0-9._]{4,50}$";
        public const string SoDienThoai = @"^(0\d{9}|\+84\d{9})$";
        public const string MatKhau = @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$";
    }

    public class HoSoViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } // chỉ hiển thị

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [RegularExpression(QuyTacNhapLieu.SoDienThoai,
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
    }

    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [RegularExpression(QuyTacNhapLieu.MatKhau,
            ErrorMessage = "Mật khẩu từ 8 ký tự, gồm chữ hoa, chữ số và ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; }
    }

    public class DiaChiViewModel
    {
        public int MaDiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự.")]
        [Display(Name = "Người nhận")]
        public string HoTenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(QuyTacNhapLieu.SoDienThoai,
            ErrorMessage = "Số điện thoại không hợp lệ (ví dụ 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string TinhThanh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phường/xã.")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự.")]
        [Display(Name = "Phường/Xã")]
        public string PhuongXa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
        [StringLength(255, ErrorMessage = "Địa chỉ cụ thể tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ cụ thể")]
        public string DiaChiCuThe { get; set; }

        [Display(Name = "Đặt làm địa chỉ mặc định")]
        public bool MacDinh { get; set; }
    }

    // Dòng hiển thị trong trang quản lý khách hàng (không lộ MatKhauHash)
    public class KhachHangItemViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public bool HoatDong { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoDonHang { get; set; }
    }
}