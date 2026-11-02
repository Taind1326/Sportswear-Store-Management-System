(function () {
    "use strict";

    const SO_DONG_TOI_DA = 100;

    function khoiTaoFormSanPham() {
        const form = document.getElementById("formSanPham");
        const danhSach = document.getElementById("danhSachPhanLoai");
        const mauDong = document.getElementById("mauDong");
        const nutThem = document.getElementById("themDong");

        if (!form || !danhSach || !mauDong || !nutThem) {
            return;
        }

        function layCacDong() {
            return Array.from(danhSach.rows);
        }

        function capNhatCacDong() {
            const cacDong = layCacDong();

            cacDong.forEach(function (dong, viTri) {
                const cacTruong = dong.querySelectorAll("[data-field]");

                cacTruong.forEach(function (truong) {
                    const tenTruong = truong.dataset.field;

                    truong.name =
                        "PhanLoai[" + viTri + "]." + tenTruong;

                    truong.id =
                        "PhanLoai_" + viTri + "__" + tenTruong;
                });

                const nutXoa = dong.querySelector(".xoa-dong");

                if (nutXoa) {
                    nutXoa.disabled = cacDong.length <= 1;
                }
            });

            nutThem.disabled = cacDong.length >= SO_DONG_TOI_DA;
        }

        function themDong() {
            if (layCacDong().length >= SO_DONG_TOI_DA) {
                return;
            }

            const dongMoi = mauDong.content.cloneNode(true);

            danhSach.appendChild(dongMoi);
            capNhatCacDong();
        }

        function xuLyXoaDong(event) {
            const nutXoa = event.target.closest(".xoa-dong");

            if (!nutXoa || !danhSach.contains(nutXoa)) {
                return;
            }

            if (layCacDong().length <= 1) {
                return;
            }

            const dong = nutXoa.closest("tr");

            if (dong) {
                dong.remove();
                capNhatCacDong();
            }
        }

        nutThem.addEventListener("click", themDong);
        danhSach.addEventListener("click", xuLyXoaDong);
        form.addEventListener("submit", capNhatCacDong);

        if (layCacDong().length === 0) {
            themDong();
        } else {
            capNhatCacDong();
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            khoiTaoFormSanPham
        );
    } else {
        khoiTaoFormSanPham();
    }
})();