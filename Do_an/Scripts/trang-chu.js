(function () {
    "use strict";

    function khoiTao() {
        const trangChu = document.querySelector(".sportix-home");

        if (!trangChu) {
            return;
        }

        // Hiển thị nội dung thay thế khi ảnh sản phẩm bị lỗi.
        trangChu.querySelectorAll(".catalog-card-media img")
            .forEach(function (anh) {
                function thayAnhLoi() {
                    const thongBao = document.createElement("span");

                    thongBao.className = "home-image-placeholder";
                    thongBao.textContent = "Chưa có ảnh";

                    anh.replaceWith(thongBao);
                }

                anh.addEventListener("error", thayAnhLoi, { once: true });

                if (anh.complete && anh.naturalWidth === 0) {
                    thayAnhLoi();
                }
            });

        // Nút trở về đầu trang.
        const nutLenDau = document.getElementById("homeBackTop");

        if (nutLenDau) {
            function capNhatNutLenDau() {
                nutLenDau.hidden = window.scrollY < 500;
            }

            window.addEventListener("scroll", capNhatNutLenDau, {
                passive: true
            });

            capNhatNutLenDau();
        }

        // Banner tự chuyển ảnh.
        const banner = document.getElementById("homeCarousel");
        const nutTamDung = document.getElementById("homeCarouselToggle");

        if (!banner ||
            !window.bootstrap ||
            !window.bootstrap.Carousel) {
            return;
        }

        const carouselCu = window.bootstrap.Carousel.getInstance(banner);

        if (carouselCu) {
            carouselCu.dispose();
        }

        const carousel = new window.bootstrap.Carousel(banner, {
            interval: 4000,
            ride: false,
            pause: false,
            keyboard: true,
            touch: true,
            wrap: true
        });

        const giamChuyenDong = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        );

        let tamDung = false;
        let dangReChuot = banner.matches(":hover");
        let dangCoTieuDiem = banner.contains(document.activeElement);

        function capNhatTrangThai() {
            carousel.pause();

            if (nutTamDung) {
                nutTamDung.hidden = giamChuyenDong.matches;
                nutTamDung.textContent = tamDung
                    ? "Tiếp tục"
                    : "Tạm dừng";

                nutTamDung.setAttribute(
                    "aria-label",
                    tamDung
                        ? "Tiếp tục tự chuyển ảnh"
                        : "Tạm dừng tự chuyển ảnh"
                );
            }

            if (!tamDung &&
                !dangReChuot &&
                !dangCoTieuDiem &&
                !document.hidden &&
                !giamChuyenDong.matches) {
                carousel.cycle();
            }
        }

        if (nutTamDung) {
            nutTamDung.addEventListener("click", function () {
                tamDung = !tamDung;
                capNhatTrangThai();
            });
        }

        banner.addEventListener("mouseenter", function () {
            dangReChuot = true;
            capNhatTrangThai();
        });

        banner.addEventListener("mouseleave", function () {
            dangReChuot = false;
            capNhatTrangThai();
        });

        banner.addEventListener("focusin", function () {
            dangCoTieuDiem = true;
            capNhatTrangThai();
        });

        banner.addEventListener("focusout", function (event) {
            dangCoTieuDiem = banner.contains(event.relatedTarget);
            capNhatTrangThai();
        });

        document.addEventListener(
            "visibilitychange",
            capNhatTrangThai
        );

        if (giamChuyenDong.addEventListener) {
            giamChuyenDong.addEventListener(
                "change",
                capNhatTrangThai
            );
        }

        window.addEventListener("pagehide", function () {
            carousel.pause();
        });

        window.addEventListener("pageshow", capNhatTrangThai);

        capNhatTrangThai();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();