using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Do_an.Models;

namespace Do_an.ViewModels
{
    public class ThongBaoWebsiteFormViewModel : IValidatableObject
    {
        public int MaThongBaoWebsite { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [StringLength(2000, ErrorMessage = "Nội dung tối đa 2.000 ký tự.")]
        [Display(Name = "Nội dung / điều kiện chung")]
        public string NoiDung { get; set; }

        [StringLength(40, ErrorMessage = "Nhãn tối đa 40 ký tự.")]
        [Display(Name = "Nhãn cột trái")]
        public string NhanTrai { get; set; }

        [StringLength(30, ErrorMessage = "Giá trị tối đa 30 ký tự.")]
        [Display(Name = "Giá trị cột trái")]
        public string GiaTriTrai { get; set; }

        [StringLength(160, ErrorMessage = "Mô tả tối đa 160 ký tự.")]
        [Display(Name = "Mô tả cột trái")]
        public string MoTaTrai { get; set; }

        [StringLength(40, ErrorMessage = "Nhãn tối đa 40 ký tự.")]
        [Display(Name = "Nhãn cột phải")]
        public string NhanPhai { get; set; }

        [StringLength(30, ErrorMessage = "Giá trị tối đa 30 ký tự.")]
        [Display(Name = "Giá trị cột phải")]
        public string GiaTriPhai { get; set; }

        [StringLength(160, ErrorMessage = "Mô tả tối đa 160 ký tự.")]
        [Display(Name = "Mô tả cột phải")]
        public string MoTaPhai { get; set; }

        [Display(Name = "Hiển thị cho khách hàng")]
        public bool HienThi { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            string[] noiDungHaiCot =
            {
                NhanTrai, GiaTriTrai, MoTaTrai,
                NhanPhai, GiaTriPhai, MoTaPhai
            };

            bool daNhap = noiDungHaiCot.Any(
                x => !string.IsNullOrWhiteSpace(x));

            if (!daNhap)
                yield break;

            string[] tenTruong =
            {
                "NhanTrai", "GiaTriTrai", "MoTaTrai",
                "NhanPhai", "GiaTriPhai", "MoTaPhai"
            };

            for (int i = 0; i < noiDungHaiCot.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(noiDungHaiCot[i]))
                {
                    yield return new ValidationResult(
                        "Vui lòng nhập đầy đủ nội dung của hai cột.",
                        new[] { tenTruong[i] });
                }
            }
        }
    }

    public class ThongBaoWebsitePopupViewModel
    {
        public ThongBaoWebsite ThongBao { get; set; }

        public int? MaTaiKhoan { get; set; }

        public bool CoHaiCot
        {
            get
            {
                if (ThongBao == null)
                    return false;

                return !string.IsNullOrWhiteSpace(ThongBao.NhanTrai)
                    && !string.IsNullOrWhiteSpace(ThongBao.GiaTriTrai)
                    && !string.IsNullOrWhiteSpace(ThongBao.MoTaTrai)
                    && !string.IsNullOrWhiteSpace(ThongBao.NhanPhai)
                    && !string.IsNullOrWhiteSpace(ThongBao.GiaTriPhai)
                    && !string.IsNullOrWhiteSpace(ThongBao.MoTaPhai);
            }
        }
    }
}