using Do_an.Models;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Do_an.Services
{
    public class CanhBaoTonKhoService
    {
        public const int NguongSapHet = 5;

        [DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 3)]
        public void KiemTra()
        {
            using (var db = new SPORTIXEntities())
            {
                var thoiDiem = DateTime.Now;

                var phanLoaiCanNhap = db.BienTheSanPhams
                    .AsNoTracking()
                    .Where(x =>
                        x.DangBan &&
                        x.SanPham.DangBan &&
                        x.SoLuongTon <= NguongSapHet)
                    .Select(x => new
                    {
                        x.MaBienThe,
                        x.SoLuongTon
                    })
                    .ToList();

                var canhBaos = db.CanhBaoTonKhoes
                    .ToDictionary(x => x.MaBienThe);

                var cacMaCanNhap = new HashSet<int>(
                    phanLoaiCanNhap.Select(x => x.MaBienThe));

                foreach (var phanLoai in phanLoaiCanNhap)
                {
                    CanhBaoTonKho canhBao;

                    if (!canhBaos.TryGetValue(
                        phanLoai.MaBienThe,
                        out canhBao))
                    {
                        canhBao = new CanhBaoTonKho
                        {
                            MaBienThe = phanLoai.MaBienThe
                        };

                        db.CanhBaoTonKhoes.Add(canhBao);

                        canhBaos.Add(
                            phanLoai.MaBienThe,
                            canhBao);
                    }

                    string mucDo = phanLoai.SoLuongTon == 0
                        ? "HetHang"
                        : "SapHet";

                    bool dotCanhBaoMoi =
                        !canhBao.DangCanhBao ||
                        canhBao.MucDo != mucDo;

                    bool coThayDoi =
                        dotCanhBaoMoi ||
                        canhBao.SoLuong != phanLoai.SoLuongTon;

                    if (!coThayDoi)
                    {
                        continue;
                    }

                    if (dotCanhBaoMoi)
                    {
                        canhBao.NgayPhatHien = thoiDiem;
                    }

                    canhBao.MucDo = mucDo;
                    canhBao.SoLuong = phanLoai.SoLuongTon;
                    canhBao.DangCanhBao = true;
                    canhBao.NgayCapNhat = thoiDiem;
                }

                foreach (var canhBao in canhBaos.Values)
                {
                    if (canhBao.DangCanhBao &&
                        !cacMaCanNhap.Contains(canhBao.MaBienThe))
                    {
                        canhBao.DangCanhBao = false;
                        canhBao.NgayCapNhat = thoiDiem;
                    }
                }

                db.SaveChanges();
            }
        }
    }
}