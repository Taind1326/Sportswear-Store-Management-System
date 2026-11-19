using System;
using System.Collections.Generic;
using System.Linq;

namespace Do_an.ViewModels
{
    public class ThongKeViewModel
    {
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public string Thang { get; set; }
        public bool TheoNgay { get; set; }
        public List<ThongKeKyViewModel> CacKy { get; } = new List<ThongKeKyViewModel>();
        public List<ThongKeTrangThaiViewModel> TrangThaiDon { get; set; }
        public List<ThongKeSanPhamViewModel> SanPhamBanChay { get; set; }

        public decimal TienThu => CacKy.Sum(x => x.TienThu);
        public decimal TienHoan => CacKy.Sum(x => x.TienHoan);
        public decimal ThucThu => TienThu - TienHoan;
    }

    public class ThongKeKyViewModel
    {
        public DateTime BatDau { get; set; }
        public decimal TienThu { get; set; }
        public decimal TienHoan { get; set; }
        public decimal ThucThu => TienThu - TienHoan;
    }

    public class ThongKeTrangThaiViewModel
    {
        public string TrangThai { get; set; }
        public int SoDon { get; set; }
    }

    public class ThongKeSanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public long SoLuong { get; set; }
    }
}
