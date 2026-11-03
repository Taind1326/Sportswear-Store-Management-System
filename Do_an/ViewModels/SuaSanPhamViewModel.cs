using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class SuaSanPhamViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Mã sản phẩm không hợp lệ."
        )]
        public int MaSanPham { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(
            200,
            ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự."
        )]
        public string TenSanPham { get; set; }

        [Display(Name = "Loại hàng")]
        [Required(ErrorMessage = "Vui lòng chọn loại hàng.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Loại hàng được chọn không hợp lệ."
        )]
        public int? MaLoaiHang { get; set; }

        [Display(Name = "Thương hiệu")]
        [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Thương hiệu được chọn không hợp lệ."
        )]
        public int? MaThuongHieu { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Giảm giá (%)")]
        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm.")]
        [Range(
            0,
            100,
            ErrorMessage = "Phần trăm giảm phải từ 0 đến 100."
        )]
        public int? PhanTramGiam { get; set; }

        [Display(Name = "Sản phẩm nổi bật")]
        public bool NoiBat { get; set; }

        [Display(Name = "Đang bán")]
        public bool DangBan { get; set; }
    }
}