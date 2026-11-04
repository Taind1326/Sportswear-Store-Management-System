using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace Do_an.Services
{
    public class HinhAnhSanPhamService
    {
        private const int GioiHanDungLuong = 5 * 1024 * 1024;
        private const int GioiHanCanhAnh = 6000;
        private const long GioiHanDiemAnh = 20000000;

        public string LuuAnh(HttpPostedFileBase tepAnh, string thuMucVatLy)
        {
            if (tepAnh == null || tepAnh.ContentLength <= 0)
            {
                throw new ArgumentException("Vui lòng chọn tệp ảnh.");
            }

            if (tepAnh.ContentLength > GioiHanDungLuong)
            {
                throw new InvalidDataException("Ảnh không được vượt quá 5 MB.");
            }

            string duoiTep = Path.GetExtension(tepAnh.FileName);
            bool laPng = string.Equals(
                duoiTep, ".png", StringComparison.OrdinalIgnoreCase);
            bool laJpeg = string.Equals(
                duoiTep, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    duoiTep, ".jpeg", StringComparison.OrdinalIgnoreCase);

            if (!laPng && !laJpeg)
            {
                throw new InvalidDataException("Chỉ chấp nhận ảnh JPG hoặc PNG.");
            }

            ImageFormat dinhDang = laPng ? ImageFormat.Png : ImageFormat.Jpeg;

            using (var duLieuAnh = new MemoryStream())
            using (var anhDaMaHoa = new MemoryStream())
            {
                var boDem = new byte[81920];
                int soByteDoc;

                while ((soByteDoc = tepAnh.InputStream.Read(
                    boDem, 0, boDem.Length)) > 0)
                {
                    if (duLieuAnh.Length + soByteDoc > GioiHanDungLuong)
                    {
                        throw new InvalidDataException("Ảnh không được vượt quá 5 MB.");
                    }

                    duLieuAnh.Write(boDem, 0, soByteDoc);
                }

                duLieuAnh.Position = 0;

                using (var anhGoc = Image.FromStream(duLieuAnh, false, true))
                {
                    if (anhGoc.RawFormat.Guid != dinhDang.Guid)
                    {
                        throw new InvalidDataException(
                            "Nội dung ảnh không khớp với đuôi tệp.");
                    }

                    if (anhGoc.Width > GioiHanCanhAnh
                        || anhGoc.Height > GioiHanCanhAnh
                        || (long)anhGoc.Width * anhGoc.Height > GioiHanDiemAnh)
                    {
                        throw new InvalidDataException(
                            "Kích thước ảnh vượt quá giới hạn cho phép.");
                    }

                    using (var anhMoi = new Bitmap(anhGoc))
                    {
                        anhMoi.Save(anhDaMaHoa, dinhDang);
                    }
                }

                if (anhDaMaHoa.Length > GioiHanDungLuong)
                {
                    throw new InvalidDataException(
                        "Ảnh sau khi xử lý vượt quá 5 MB.");
                }

                Directory.CreateDirectory(thuMucVatLy);

                string tenTep = Guid.NewGuid().ToString("N")
                    + (laPng ? ".png" : ".jpg");
                string duongDan = Path.Combine(thuMucVatLy, tenTep);

                bool daTaoTep = false;

                try
                {
                    using (var tepDich = new FileStream(
                        duongDan, FileMode.CreateNew, FileAccess.Write))
                    {
                        daTaoTep = true;
                        anhDaMaHoa.WriteTo(tepDich);
                    }
                }
                catch
                {
                    if (daTaoTep)
                    {
                        try
                        {
                            File.Delete(duongDan);
                        }
                        catch (Exception ex) when (
                            ex is IOException || ex is UnauthorizedAccessException)
                        {
                            System.Diagnostics.Trace.TraceError(
                                "Lỗi dọn tệp ảnh {0}: {1}", tenTep, ex);
                        }
                    }

                    throw;
                }

                return tenTep;
            }
        }
    }
}