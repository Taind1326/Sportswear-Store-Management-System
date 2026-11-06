(function () {
    "use strict";

    function khoiTao() {
        const trang = document.querySelector(".sportix-catalog-page");

        if (!trang) {
            return;
        }

        // Cập nhật nhãn giá.
        const thanhGia = trang.querySelector("#giaDen");
        const nhanGia = trang.querySelector("#nhanGiaToiDa");

        if (thanhGia && nhanGia) {
            thanhGia.addEventListener("input", function () {
                nhanGia.textContent = Number(thanhGia.value)
                    .toLocaleString("vi-VN") + " đ";
            });
        }

        // Thay ảnh bị lỗi bằng thông báo.
        trang.querySelectorAll(".catalog-card-media img")
            .forEach(function (anh) {
                function thayAnhLoi() {
                    const thongBao = document.createElement("span");

                    thongBao.className = "catalog-card-placeholder";
                    thongBao.textContent = "Chưa có ảnh";

                    anh.replaceWith(thongBao);
                }

                anh.addEventListener("error", thayAnhLoi, {
                    once: true
                });

                if (anh.complete && anh.naturalWidth === 0) {
                    thayAnhLoi();
                }
            });

        // Điều khiển hàng sản phẩm mới.
        const danhSach = trang.querySelector("#sanPhamMoi");
        const nutTruoc = trang.querySelector("#sanPhamMoiTruoc");
        const nutSau = trang.querySelector("#sanPhamMoiSau");

        if (!danhSach || !nutTruoc || !nutSau) {
            return;
        }

        const giamChuyenDong = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        );

        function capNhatNut() {
            const gioiHan = danhSach.scrollWidth
                - danhSach.clientWidth;

            nutTruoc.disabled = danhSach.scrollLeft <= 1;
            nutSau.disabled = danhSach.scrollLeft >= gioiHan - 1;
        }

        function chuyenThe(huong) {
            const the = danhSach.querySelector(
                ".catalog-arrival-item"
            );

            if (!the) {
                return;
            }

            const khoangCach = parseFloat(
                window.getComputedStyle(danhSach).columnGap
            ) || 0;

            danhSach.scrollBy({
                left: huong * (
                    the.getBoundingClientRect().width + khoangCach
                ),
                behavior: giamChuyenDong.matches
                    ? "auto"
                    : "smooth"
            });
        }

        nutTruoc.addEventListener("click", function () {
            chuyenThe(-1);
        });

        nutSau.addEventListener("click", function () {
            chuyenThe(1);
        });

        danhSach.addEventListener("scroll", capNhatNut, {
            passive: true
        });

        window.addEventListener("resize", capNhatNut);
        window.addEventListener("pageshow", capNhatNut);

        capNhatNut();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();