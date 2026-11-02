using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Do_an.Validation;

namespace Do_an.ViewModels
{
    public class ThemSanPhamViewModel
    {
        public ThemSanPhamViewModel()
        {
            PhanLoai = new List<PhanLoaiSanPhamInput>();
        }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = ValidationMessages.Required)]
        [StringLength(
            200,
            ErrorMessage = ValidationMessages.StringLength)]
        public string TenSanPham { get; set; }

        [Display(Name = "Loại hàng")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = ValidationMessages.Selection)]
        public int MaLoaiHang { get; set; }

        [Display(Name = "Thương hiệu")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = ValidationMessages.Selection)]
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
        [Required(ErrorMessage = ValidationMessages.Required)]
        [StringLength(
            50,
            ErrorMessage = ValidationMessages.StringLength)]
        public string MauSac { get; set; }

        [Display(Name = "Size / Kích cỡ")]
        [Required(ErrorMessage = ValidationMessages.Required)]
        [StringLength(
            30,
            ErrorMessage = ValidationMessages.StringLength)]
        public string KichThuoc { get; set; }

        [Display(Name = "Giá gốc")]
        [Required(ErrorMessage = ValidationMessages.Required)]
        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99",
            ErrorMessage = ValidationMessages.Range)]
        public decimal? GiaBan { get; set; }

        [Display(Name = "Tồn kho")]
        [Required(ErrorMessage = ValidationMessages.Required)]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = ValidationMessages.Range)]
        public int? SoLuongTon { get; set; }
    }
}