using Do_an.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Do_an.Areas.Admin.Controllers
{
    public class XoaSanPhamController : Controller
    {
        private readonly SPORTIXEntities db = new SPORTIXEntities();

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            int maTaiKhoan;

            bool daDangNhap = int.TryParse(
                Convert.ToString(Session["MaTaiKhoan"]),
                out maTaiKhoan);

            if (!daDangNhap || maTaiKhoan <= 0)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Unauthorized);

                return;
            }

            bool coQuyen = db.TaiKhoans
                .AsNoTracking()
                .Any(x =>
                    x.MaTaiKhoan == maTaiKhoan &&
                    x.VaiTro == "Admin" &&
                    x.HoatDong);

            if (!coQuyen)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden);

                return;
            }

            base.OnActionExecuting(filterContext);
        }

        [HttpGet]
        public ActionResult Index(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var sanPham = db.SanPhams
                .AsNoTracking()
                .SingleOrDefault(x =>
                    x.MaSanPham == id.Value);

            if (sanPham == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm.");
            }

            ViewBag.CoDuLieuLienQuan = CoDuLieuLienQuan(
                sanPham.MaSanPham);

            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhan(int id, bool xoaVinhVien)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var objectContext =
                ((IObjectContextAdapter)db).ObjectContext;

            var ketNoi = objectContext.Connection;
            bool canDongKetNoi = ketNoi.State == ConnectionState.Closed;

            try
            {
                if (canDongKetNoi)
                {
                    ketNoi.Open();
                }

                using (var giaoDich = ketNoi.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    var sanPham = db.SanPhams
                        .SingleOrDefault(x => x.MaSanPham == id);

                    if (sanPham == null)
                    {
                        return HttpNotFound(
                            "Sản phẩm không còn tồn tại.");
                    }

                    bool coLienQuan = CoDuLieuLienQuan(id);

                    if (xoaVinhVien && coLienQuan)
                    {
                        TempData["Loi"] =
                            "Sản phẩm đã phát sinh dữ liệu liên quan. "
                            + "Vui lòng xác nhận ngừng bán.";

                        return RedirectToAction("Index", new { id });
                    }

                    if (!xoaVinhVien)
                    {
                        sanPham.DangBan = false;
                        sanPham.NgayCapNhat = DateTime.Now;

                        db.SaveChanges();
                        giaoDich.Commit();

                        TempData["ThanhCong"] =
                            "Đã ngừng bán sản phẩm. "
                            + "Dữ liệu và tồn kho được giữ nguyên.";

                        return RedirectToAction(
                            "ChiTiet",
                            "SanPham",
                            new
                            {
                                area = "Admin",
                                id = id
                            });
                    }

                    var canhBaos = db.CanhBaoTonKhoes
                        .Where(x =>
                            x.BienTheSanPham.MaSanPham == id)
                        .ToList();

                    var hinhAnhs = db.HinhAnhSanPhams
                        .Where(x => x.MaSanPham == id)
                        .ToList();

                    var bienThes = db.BienTheSanPhams
                        .Where(x => x.MaSanPham == id)
                        .ToList();

                    foreach (var canhBao in canhBaos)
                    {
                        db.CanhBaoTonKhoes.Remove(canhBao);
                    }

                    foreach (var hinhAnh in hinhAnhs)
                    {
                        db.HinhAnhSanPhams.Remove(hinhAnh);
                    }

                    foreach (var bienThe in bienThes)
                    {
                        db.BienTheSanPhams.Remove(bienThe);
                    }

                    db.SanPhams.Remove(sanPham);

                    db.SaveChanges();
                    giaoDich.Commit();

                    TempData["ThanhCong"] =
                        "Đã xóa sản phẩm chưa có dữ liệu liên quan.";

                    return RedirectToAction(
                        "Index",
                        "SanPham",
                        new { area = "Admin" });
                }
            }
            catch (DbUpdateException ex)
            {
                GhiNhanLoi(ex);
            }
            catch (EntityException ex)
            {
                GhiNhanLoi(ex);
            }
            catch (SqlException ex)
            {
                GhiNhanLoi(ex);
            }
            finally
            {
                if (canDongKetNoi &&
                    ketNoi.State != ConnectionState.Closed)
                {
                    ketNoi.Close();
                }
            }

            return RedirectToAction("Index", new { id });
        }

        private bool CoDuLieuLienQuan(int maSanPham)
        {
            bool coDonHang = db.ChiTietDonHangs.Any(x =>
                x.BienTheSanPham.MaSanPham == maSanPham);

            if (coDonHang)
            {
                return true;
            }

            bool coGioHang = db.ChiTietGioHangs.Any(x =>
                x.BienTheSanPham.MaSanPham == maSanPham);

            if (coGioHang)
            {
                return true;
            }

            return db.LichSuTonKhoes.Any(x =>
                x.BienTheSanPham.MaSanPham == maSanPham);
        }

        private void GhiNhanLoi(Exception ex)
        {
            Trace.TraceError(
                "Lỗi xóa hoặc ngừng bán sản phẩm: {0}",
                ex);

            TempData["Loi"] =
                "Chưa thực hiện được thao tác. "
                + "Dữ liệu có thể vừa thay đổi hoặc có liên kết khác. "
                + "Vui lòng kiểm tra và thử lại.";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}