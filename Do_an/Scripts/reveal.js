(function () {
    "use strict";

    function khoiTao() {
        const noiDung = document.querySelector(
            ".sportix-public #mainContent"
        );

        if (!noiDung) {
            return;
        }

        const duongDan = window.location.pathname
            .replace(/\/+$/, "")
            .replace(/\/index$/i, "")
            .toLowerCase() || "/";

        try {
            const trangTruoc = sessionStorage.getItem(
                "sportixTrangReveal"
            );

            sessionStorage.setItem(
                "sportixTrangReveal",
                duongDan
            );

            if (trangTruoc === duongDan) {
                return;
            }
        } catch (error) {
            return;
        }

        if (!("IntersectionObserver" in window) ||
            window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return;
        }

        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) {
                    return;
                }

                entry.target.classList.remove("reveal-pending");
                observer.unobserve(entry.target);
            });
        }, {
            threshold: 0.1,
            rootMargin: "0px 0px -24px 0px"
        });

        noiDung.querySelectorAll("[data-reveal]")
            .forEach(function (phanTu) {
                phanTu.classList.add("reveal-pending");
                observer.observe(phanTu);
            });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();