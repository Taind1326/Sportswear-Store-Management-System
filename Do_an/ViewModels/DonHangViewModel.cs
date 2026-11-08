using System.Collections.Generic;
using Do_an.Models;

namespace Do_an.ViewModels
{
    public class DanhSachDonHangViewModel
    {
        public List<DonHang> DonHangs { get; set; }
        public string TrangThai { get; set; }
        public string TuKhoa { get; set; }
        public int Trang { get; set; }
        public int TongTrang { get; set; }
        public int TongSo { get; set; }
    }

    public class ChiTietDonHangViewModel
    {
        public DonHang Don { get; set; }
        public List<ChiTietDonHang> Dong { get; set; }
        public List<ThanhToan> ThanhToans { get; set; }
    }
}
