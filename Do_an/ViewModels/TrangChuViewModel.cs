using Do_an.Models;
using System.Collections.Generic;

namespace Do_an.ViewModels
{
    public class TrangChuViewModel
    {
        public TrangChuViewModel()
        {
            DanhMuc = new List<LoaiHang>();
            SanPhamNoiBat = new List<SanPham>();
        }

        public List<LoaiHang> DanhMuc { get; set; }

        public List<SanPham> SanPhamNoiBat { get; set; }

        public int? MaLoaiHangBongDa { get; set; }

        public int TongSanPham { get; set; }

        public int TongLoaiHang { get; set; }

        public int TongThuongHieu { get; set; }

        public int TongKhachHang { get; set; }

        public int MucGiamCaoNhat { get; set; }
    }
}