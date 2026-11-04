(function () {
    "use strict";

    if (!("IntersectionObserver" in window) ||
        window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        return;
    }

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.remove("reveal-pending");
                observer.unobserve(entry.target);
            }
        });
    });

    document.querySelectorAll(".sportix-public [data-reveal]")
        .forEach(function (section) {
            section.classList.add("reveal-pending");
            observer.observe(section);
        });
})();