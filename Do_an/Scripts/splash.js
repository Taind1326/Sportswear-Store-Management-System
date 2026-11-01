(function () {
    "use strict";

    function initSplash() {
        var splash = document.getElementById("sportixSplash");

        if (!splash) {
            return;
        }

        // Người dùng chọn giảm chuyển động thì bỏ qua splash.
        var reduceMotion = window.matchMedia &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        if (reduceMotion) {
            return;
        }

        splash.hidden = false;

        // Tự ẩn sau khi hiệu ứng hoàn tất.
        window.setTimeout(function () {
            splash.hidden = true;
        }, 1450);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initSplash);
    } else {
        initSplash();
    }
})();