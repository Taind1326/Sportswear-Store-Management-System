using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Do_an.ViewModels
{
    public class ThemHinhAnhViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Mã sản phẩm không hợp lệ.")]
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        [Display(Name = "Ảnh sản phẩm")]
        [Required(
            ErrorMessage = "Vui lòng chọn ảnh sản phẩm.")]
        public HttpPostedFileBase TepAnh { get; set; }

        [Display(Name = "Mô tả ảnh")]
        [StringLength(
            200,
            ErrorMessage = "Mô tả ảnh không được vượt quá 200 ký tự.")]
        public string MoTaAnh { get; set; }

        [Display(Name = "Đặt làm ảnh đại diện")]
        public bool AnhDaiDien { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        [Required(
            ErrorMessage = "Vui lòng nhập thứ tự hiển thị.")]
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Thứ tự hiển thị phải là số nguyên không âm.")]
        public int? ThuTu { get; set; }

        public ThemHinhAnhViewModel()
        {
            ThuTu = 0;
        }
    }
}