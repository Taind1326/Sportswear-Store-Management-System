
using System.Collections.Generic;

namespace Do_an.ViewModels
{
    public class GioHangViewModel
    {
        public GioHangViewModel()
        {
            DongHang = new List<GioHangDongViewModel>();
        }

        public List<GioHangDongViewModel> DongHang { get; set; }

        public int TongSoLuong { get; set; }

        public decimal TienHang { get; set; }
    }

    public class GioHangDongViewModel
    {
        public int MaChiTietGioHang { get; set; }

        public int MaBienThe { get; set; }

        public string TenSanPham { get; set; }

        public string SKU { get; set; }

        public string MauSac { get; set; }

        public string KichThuoc { get; set; }

        public string DuongDanAnh { get; set; }

        public decimal GiaGoc { get; set; }

        public decimal DonGia { get; set; }

        public int PhanTramGiam { get; set; }

        public int SoLuong { get; set; }

        public int SoLuongTon { get; set; }

        public bool ConTheMua { get; set; }

        public decimal ThanhTien
        {
            get
            {
                return DonGia * SoLuong;
            }
        }
    }
}
