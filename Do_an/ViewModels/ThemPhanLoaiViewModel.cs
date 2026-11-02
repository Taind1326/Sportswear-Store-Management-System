using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class ThemPhanLoaiViewModel : PhanLoaiSanPhamInput
    {
        [Range(1, int.MaxValue)]
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }
    }
}