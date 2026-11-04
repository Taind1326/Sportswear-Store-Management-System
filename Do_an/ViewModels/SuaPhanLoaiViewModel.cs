using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class SuaPhanLoaiViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Mã phân loại không hợp lệ."
        )]
        public int MaBienThe { get; set; }

        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        public string SKU { get; set; }

        [Display(Name = "Màu")]
        [StringLength(
            50,
            ErrorMessage = "Màu không được vượt quá 50 ký tự."
        )]
        public string MauSac { get; set; }

        [Display(Name = "Kích cỡ / dung tích")]
        [StringLength(
            30,
            ErrorMessage = "Kích cỡ không được vượt quá 30 ký tự."
        )]
        public string KichThuoc { get; set; }

        [Display(Name = "Giá gốc")]
        [Required(ErrorMessage = "Vui lòng nhập giá gốc.")]
        [Range(
            typeof(decimal),
            "0",
            "9999999999999999.99",
            ErrorMessage = "Giá gốc phải từ 0 đến 9999999999999999,99 đồng."
        )]
        public decimal? GiaBan { get; set; }

        [Display(Name = "Đang bán")]
        public bool DangBan { get; set; }
    }
}