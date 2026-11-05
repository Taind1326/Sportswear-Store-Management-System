using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class LoaiHangViewModel
    {
        public int MaLoaiHang { get; set; }

        [Display(Name = "Tên loại hàng")]
        [Required(ErrorMessage = "Vui lòng nhập tên loại hàng.")]
        [StringLength(
            100,
            ErrorMessage = "Tên loại hàng không được vượt quá 100 ký tự.")]
        public string TenLoaiHang { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(
            500,
            ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string MoTa { get; set; }

        [Display(Name = "Đang sử dụng")]
        public bool HoatDong { get; set; }
    }
}