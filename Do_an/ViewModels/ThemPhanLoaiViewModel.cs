using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Do_an.ViewModels
{
    public class ThemPhanLoaiViewModel : PhanLoaiSanPhamInput
    {
        [Range(1, int.MaxValue)]
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        [Display(Name = "Ảnh theo màu")]
        public HttpPostedFileBase TepAnhTheoMau { get; set; }
    }
}