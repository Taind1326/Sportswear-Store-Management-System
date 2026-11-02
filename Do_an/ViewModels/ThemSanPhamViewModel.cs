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

        [Required(ErrorMessage = "Nhập tên sản phẩm.")]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string TenSanPham { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Chọn loại hàng.")]
        [Display(Name = "Loại hàng")]
        public int MaLoaiHang { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Chọn thương hiệu.")]
        [Display(Name = "Thương hiệu")]
        public int MaThuongHieu { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Sản phẩm nổi bật")]
        public bool NoiBat { get; set; }

        public List<PhanLoaiSanPhamInput> PhanLoai { get; set; }
    }

    public class PhanLoaiSanPhamInput
    {
        [Required(ErrorMessage = "Nhập màu hoặc Mặc định.")]
        [StringLength(50)]
        [Display(Name = "Màu")]
        public string MauSac { get; set; }

        [Required(ErrorMessage = "Nhập size hoặc Mặc định.")]
        [StringLength(30)]
        [Display(Name = "Size")]
        public string KichThuoc { get; set; }

        [Required(ErrorMessage = "Nhập giá.")]
        [Range(typeof(decimal), "0", "9999999999999999.99",
            ErrorMessage = "Giá phải từ 0 trở lên.")]
        [Display(Name = "Giá gốc")]
        public decimal? GiaBan { get; set; }

        [Required(ErrorMessage = "Nhập số lượng tồn.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "Tồn kho phải từ 0 trở lên.")]
        [Display(Name = "Tồn kho")]
        public int? SoLuongTon { get; set; }
    }
}