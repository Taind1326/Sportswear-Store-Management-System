using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class ThongBaoWebsiteFormViewModel
    {
        public int MaThongBaoWebsite { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [StringLength(2000)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Hiển thị cho khách")]
        public bool HienThi { get; set; }
    }
}
