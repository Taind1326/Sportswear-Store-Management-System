using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Do_an.ViewModels
{
    public class BoSungHangViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Mã sản phẩm không hợp lệ.")]
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        [Display(Name = "Loại cần bổ sung")]
        [Required(
            ErrorMessage = "Vui lòng chọn loại cần bổ sung.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Loại cần bổ sung không hợp lệ.")]
        public int? MaBienThe { get; set; }

        [Display(Name = "Số lượng bổ sung")]
        [Required(
            ErrorMessage = "Vui lòng nhập số lượng cần bổ sung.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Số lượng bổ sung phải là số nguyên từ 1 đến 2.147.483.647.")]
        public int? SoLuongThem { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(
            500,
            ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string LyDo { get; set; }

        public Guid YeuCauId { get; set; }

        public List<SelectListItem> DanhSachPhanLoai { get; set; }

        public BoSungHangViewModel()
        {
            DanhSachPhanLoai = new List<SelectListItem>();
        }
    }
}