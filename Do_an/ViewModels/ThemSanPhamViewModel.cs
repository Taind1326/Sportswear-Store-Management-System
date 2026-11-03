using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class ThemSanPhamViewModel
    {
        public ThemSanPhamViewModel()
        {
            PhanLoai = new List<PhanLoaiSanPhamInput>();
        }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(
            200,
            ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự.")]
        public string TenSanPham { get; set; }

        [Display(Name = "Loại hàng")]
        [Required(ErrorMessage = "Vui lòng chọn loại hàng.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn loại hàng hợp lệ.")]
        public int MaLoaiHang { get; set; }

        [Display(Name = "Thương hiệu")]
        [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Vui lòng chọn thương hiệu hợp lệ.")]
        public int MaThuongHieu { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Sản phẩm nổi bật")]
        public bool NoiBat { get; set; }

        public List<PhanLoaiSanPhamInput> PhanLoai { get; set; }
    }

    public class PhanLoaiSanPhamInput
    {
        [Display(Name = "Màu")]
        [Required(ErrorMessage = "Vui lòng nhập màu sắc.")]
        [StringLength(
            50,
            ErrorMessage = "Màu sắc không được vượt quá 50 ký tự.")]
        public string MauSac { get; set; }

        [Display(Name = "Size / Kích cỡ")]
        [Required(ErrorMessage = "Vui lòng nhập size hoặc kích cỡ.")]
        [StringLength(
            30,
            ErrorMessage = "Size hoặc kích cỡ không được vượt quá 30 ký tự.")]
        public string KichThuoc { get; set; }

        [Display(Name = "Giá gốc")]
        [Required(ErrorMessage = "Vui lòng nhập giá gốc.")]
        [Range(
    typeof(decimal),
    "0",
    "9999999999999999.99",
    ErrorMessage = "Giá gốc phải từ 0 đến 9999999999999999,99 đồng.")]
        public decimal? GiaBan { get; set; }

        [Display(Name = "Tồn kho")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho.")]
        [Range(
    0,
    int.MaxValue,
    ErrorMessage = "Số lượng tồn kho phải từ 0 đến 2147483647.")]
        public int? SoLuongTon { get; set; }
    }
}