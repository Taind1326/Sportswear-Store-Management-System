(function () {
    "use strict";

    const khoa = "sportixChuyenTrang";

    function duongDan(url) {
        return url.pathname
            .replace(/\/+$/, "")
            .replace(/\/index$/i, "")
            .toLowerCase() || "/";
    }

    function khoiTao() {
        const splash = document.getElementById("sportixSplash");

        if (!splash ||
            !document.body.classList.contains("sportix-public")) {
            return;
        }

        splash.hidden = true;

        const hienTai = new URL(window.location.href);
        let chuyenTrang = null;

        try {
            chuyenTrang = JSON.parse(
                sessionStorage.getItem(khoa)
            );

            sessionStorage.removeItem(khoa);
        } catch (error) {
            chuyenTrang = null;
        }

        function xoaGhiNhan() {
            try {
                sessionStorage.removeItem(khoa);
            } catch (error) {
                return;
            }
        }

        // Gửi form lọc hoặc tìm kiếm thì bỏ qua splash.
        document.addEventListener("submit", xoaGhiNhan, true);

        document.addEventListener("click", function (event) {
            if (event.defaultPrevented ||
                event.button !== 0 ||
                event.ctrlKey ||
                event.metaKey ||
                event.shiftKey ||
                event.altKey) {
                return;
            }

            const link = event.target.closest("a[href]");

            if (!link ||
                link.hasAttribute("download") ||
                (link.target && link.target !== "_self")) {
                return;
            }

            xoaGhiNhan();

            const dich = new URL(link.href, hienTai);

            if (link.hasAttribute("data-skip-splash") ||
                dich.origin !== hienTai.origin ||
                duongDan(dich) === duongDan(hienTai)) {
                return;
            }

            // Ghi nhận đúng trang sắp chuyển đến.
            try {
                sessionStorage.setItem(
                    khoa,
                    JSON.stringify({
                        url: dich.href,
                        thoiDiem: Date.now()
                    })
                );
            } catch (error) {
                return;
            }
        });

        window.addEventListener("pagehide", function () {
            splash.hidden = true;
        });

        const giamChuyenDong = window.matchMedia &&
            window.matchMedia(
                "(prefers-reduced-motion: reduce)"
            ).matches;

        const tuoi = chuyenTrang
            ? Date.now() - chuyenTrang.thoiDiem
            : -1;

        if (!chuyenTrang ||
            chuyenTrang.url !== hienTai.href ||
            tuoi < 0 ||
            tuoi > 15000 ||
            giamChuyenDong) {
            return;
        }

        splash.hidden = false;

        window.setTimeout(function () {
            splash.hidden = true;
        }, 1450);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();