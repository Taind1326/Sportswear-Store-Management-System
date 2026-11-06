(function () {
    "use strict";

    function khoiTao() {
        const trang = document.querySelector(".sportix-home");

        if (!trang) {
            return;
        }

        trang.querySelectorAll(".catalog-card-media img")
            .forEach(function (anh) {
                function hienThayThe() {
                    const thayThe = document.createElement("span");
                    thayThe.className = "home-image-placeholder";
                    thayThe.textContent = "Chưa có ảnh";
                    anh.replaceWith(thayThe);
                }

                anh.addEventListener(
                    "error",
                    hienThayThe,
                    { once: true }
                );

                if (anh.complete && anh.naturalWidth === 0) {
                    hienThayThe();
                }
            });

        const nutLenDau = document.getElementById("homeBackTop");

        if (nutLenDau) {
            function capNhatNut() {
                nutLenDau.hidden = window.scrollY < 500;
            }

            window.addEventListener(
                "scroll",
                capNhatNut,
                { passive: true }
            );

            capNhatNut();
        }

        const banner = document.getElementById("homeCarousel");
        const nutDung = document.getElementById(
            "homeCarouselToggle"
        );

        if (!banner ||
            !nutDung ||
            !window.bootstrap ||
            !window.bootstrap.Carousel) {
            return;
        }

        const chuyenDong = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        );

        const carousel = new window.bootstrap.Carousel(
            banner,
            {
                interval: 5000,
                ride: false,
                pause: false,
                keyboard: true,
                touch: true
            }
        );

        let tamDung = false;
        let coChuot = false;
        let coTieuDiem = false;

        function capNhatBanner() {
            carousel.pause();

            nutDung.hidden = chuyenDong.matches;
            nutDung.textContent = tamDung
                ? "Tiếp tục"
                : "Tạm dừng";

            if (!tamDung &&
                !coChuot &&
                !coTieuDiem &&
                !document.hidden &&
                !chuyenDong.matches) {
                carousel.cycle();
            }
        }

        nutDung.addEventListener("click", function () {
            tamDung = !tamDung;
            capNhatBanner();
        });

        banner.addEventListener("mouseenter", function () {
            coChuot = true;
            capNhatBanner();
        });

        banner.addEventListener("mouseleave", function () {
            coChuot = false;
            capNhatBanner();
        });

        banner.addEventListener("focusin", function () {
            coTieuDiem = true;
            capNhatBanner();
        });

        banner.addEventListener("focusout", function (event) {
            coTieuDiem = banner.contains(event.relatedTarget);
            capNhatBanner();
        });

        document.addEventListener(
            "visibilitychange",
            capNhatBanner
        );

        if (chuyenDong.addEventListener) {
            chuyenDong.addEventListener(
                "change",
                capNhatBanner
            );
        }

        window.addEventListener("pagehide", function () {
            carousel.pause();
        });

        window.addEventListener("pageshow", capNhatBanner);

        capNhatBanner();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();