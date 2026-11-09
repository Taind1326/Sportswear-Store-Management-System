(function () {
    "use strict";

    function khoiTao() {
        var popup = document.getElementById("sportixPromotion");

        if (!popup ||
            !window.bootstrap ||
            !window.bootstrap.Modal) {
            return;
        }

        var khoa = "sportix-promotion:" +
            popup.getAttribute("data-notice-key");

        try {
            if (window.sessionStorage.getItem(khoa) === "1") {
                return;
            }
        } catch (error) {
            // Vẫn cho xem khi trình duyệt chặn sessionStorage.
        }

        var modal = window.bootstrap.Modal
            .getOrCreateInstance(popup);

        popup.addEventListener("shown.bs.modal", function () {
            try {
                window.sessionStorage.setItem(khoa, "1");
            } catch (error) {
                // Không ảnh hưởng thao tác đóng popup.
            }
        }, { once: true });

        var soLanCho = 0;

        function hienKhiSanSang() {
            var splash = document.getElementById("sportixSplash");

            var splashDangHien =
                splash &&
                !splash.hidden &&
                splash.getClientRects().length > 0;

            var modalKhac = document.querySelector(".modal.show");

            if (splashDangHien || modalKhac) {
                soLanCho += 1;

                if (soLanCho < 100) {
                    window.setTimeout(hienKhiSanSang, 100);
                }

                return;
            }

            modal.show();
        }

        hienKhiSanSang();
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            khoiTao,
            { once: true }
        );
    } else {
        khoiTao();
    }
})();