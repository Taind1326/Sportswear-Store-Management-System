(function () {
    "use strict";

    function khoiTao() {
        var popup = document.getElementById("sportixPromotion");

        if (!popup || !window.bootstrap || !window.bootstrap.Modal) {
            return;
        }

        var ma = popup.getAttribute("data-notice-id");
        var taiKhoan = popup.getAttribute("data-account-id");
        var khoa = "sportix-promotion:" + (taiKhoan || "guest") + ":" + ma;

        try {
            if (window.sessionStorage.getItem(khoa) === "1") {
                return;
            }
        } catch (loi) {
            // Vẫn cho xem nếu trình duyệt chặn lưu trữ.
        }

        function ghiNhanTaiKhoan(lanThu) {
            var form = popup.querySelector(".promotion-seen-form");

            if (!taiKhoan || !form || !window.jQuery) {
                return;
            }

            window.jQuery.ajax({
                url: form.action,
                type: "POST",
                data: window.jQuery(form).serialize(),
                error: function () {
                    if (lanThu < 2) {
                        window.setTimeout(function () {
                            ghiNhanTaiKhoan(lanThu + 1);
                        }, 1000);
                    }
                }
            });
        }

        popup.addEventListener("shown.bs.modal", function () {
            try {
                window.sessionStorage.setItem(khoa, "1");
            } catch (loi) {
                // Việc ghi nhận tài khoản vẫn thực hiện ở server.
            }

            ghiNhanTaiKhoan(0);
        });

        var soLanCho = 0;

        function hienKhiSanSang() {
            var splash = document.getElementById("sportixSplash");

            var dangSplash = splash &&
                !splash.hidden &&
                splash.getClientRects().length > 0;

            if (dangSplash || document.querySelector(".modal.show")) {
                if (++soLanCho < 100) {
                    window.setTimeout(hienKhiSanSang, 100);
                }

                return;
            }

            window.bootstrap.Modal
                .getOrCreateInstance(popup)
                .show();
        }

        hienKhiSanSang();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();