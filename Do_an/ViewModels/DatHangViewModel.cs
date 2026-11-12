
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class DatHangViewModel
    {
        public DatHangViewModel()
        {
            DongHang = new List<DatHangDongViewModel>();
            PhuongThucThanhToan = "COD";
        }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100)]
        public string HoTenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(
            @"^[0-9+\s().-]{9,20}$",
            ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
        [StringLength(100)]
        public string TinhThanh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phường/xã.")]
        [StringLength(100)]
        public string PhuongXa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
        [StringLength(255)]
        public string DiaChiCuThe { get; set; }

        [StringLength(1000)]
        public string GhiChu { get; set; }

        [StringLength(30)]
        public string MaCode { get; set; }

        public string PhuongThucThanhToan { get; set; }

        public List<DatHangDongViewModel> DongHang { get; set; }

        public int TongSoLuong { get; set; }

        public decimal TienHang { get; set; }

        public decimal TienGiam { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TongTien { get; set; }
    }

    public class DatHangDongViewModel
    {
        public int MaBienThe { get; set; }

        public string TenSanPham { get; set; }

        public string SKU { get; set; }

        public string MauSac { get; set; }

        public string KichThuoc { get; set; }

        public string DuongDanAnh { get; set; }

        public int SoLuong { get; set; }

        public int SoLuongTon { get; set; }

        public decimal GiaGoc { get; set; }

        public decimal DonGia { get; set; }

        public int PhanTramGiam { get; set; }

        public decimal ThanhTien
        {
            get
            {
                return DonGia * SoLuong;
            }
        }
    }
}
