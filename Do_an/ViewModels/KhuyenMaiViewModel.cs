using System;
using System.ComponentModel.DataAnnotations;

namespace Do_an.ViewModels
{
    public class KhuyenMaiFormViewModel : IValidatableObject
    {
        public int MaGiamGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá.")]
        [StringLength(30, ErrorMessage = "Mã tối đa 30 ký tự.")]
        [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Mã chỉ gồm chữ không dấu, số, - và _.")]
        [Display(Name = "Mã giảm giá")]
        public string MaCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi.")]
        [StringLength(150)]
        [Display(Name = "Tên khuyến mãi")]
        public string TenKhuyenMai { get; set; }

        [Required]
        [Display(Name = "Loại giảm")]
        public string LoaiGiam { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        [Display(Name = "Giá trị giảm")]
        public decimal GiaTriGiam { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Đơn tối thiểu không âm.")]
        [Display(Name = "Đơn tối thiểu")]
        public decimal DonToiThieu { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "Mức giảm tối đa phải lớn hơn 0 (để trống nếu không giới hạn).")]
        [Display(Name = "Giảm tối đa")]
        public decimal? GiamToiDa { get; set; }

        [Required(ErrorMessage = "Chọn ngày bắt đầu.")]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime? NgayBatDau { get; set; }

        [Required(ErrorMessage = "Chọn ngày kết thúc.")]
        [Display(Name = "Ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [Range(1, 1000000, ErrorMessage = "Số lượt tối đa từ 1 trở lên.")]
        [Display(Name = "Số lượt tối đa")]
        public int SoLuotToiDa { get; set; }

        public int SoLuotDaDung { get; set; }

        [Display(Name = "Đang bật")]
        public bool HoatDong { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            if (LoaiGiam == Services.LoaiGiamGia.PhanTram && (GiaTriGiam <= 0 || GiaTriGiam > 100))
                yield return new ValidationResult("Giảm theo % phải trong khoảng 0–100.", new[] { "GiaTriGiam" });

            if (LoaiGiam != Services.LoaiGiamGia.PhanTram && LoaiGiam != Services.LoaiGiamGia.SoTien)
                yield return new ValidationResult("Loại giảm không hợp lệ.", new[] { "LoaiGiam" });

            if (LoaiGiam == Services.LoaiGiamGia.SoTien && GiamToiDa.HasValue)
                yield return new ValidationResult("Chỉ mã giảm theo % mới có mức giảm tối đa.", new[] { "GiamToiDa" });

            if (NgayBatDau.HasValue && NgayKetThuc.HasValue && NgayKetThuc < NgayBatDau)
                yield return new ValidationResult("Ngày kết thúc phải sau ngày bắt đầu.", new[] { "NgayKetThuc" });

            if (SoLuotToiDa < SoLuotDaDung)
                yield return new ValidationResult("Số lượt tối đa không được nhỏ hơn số lượt đã dùng (" + SoLuotDaDung + ").", new[] { "SoLuotToiDa" });
        }
    }
}
