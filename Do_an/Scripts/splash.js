(function () {
    "use strict";

    function khoiTao() {
        const splash = document.getElementById("sportixSplash");

        if (!splash ||
            !document.body.classList.contains("sportix-public")) {
            return;
        }

        const duongDan = window.location.pathname
            .replace(/\/+$/, "")
            .toLowerCase() || "/";

        let canHienSplash;

        try {
            const trangTruoc = sessionStorage.getItem(
                "sportixTrangHienTai"
            );

            canHienSplash = trangTruoc !== duongDan;

            sessionStorage.setItem(
                "sportixTrangHienTai",
                duongDan
            );
        } catch (error) {
            return;
        }

        const giamChuyenDong = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches;

        if (!canHienSplash || giamChuyenDong) {
            return;
        }

        splash.hidden = false;

        window.setTimeout(function () {
            splash.hidden = true;
        }, 1450);

        window.addEventListener("pagehide", function () {
            splash.hidden = true;
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();