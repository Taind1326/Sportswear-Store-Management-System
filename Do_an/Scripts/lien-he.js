(function () {
    "use strict";

    function khoiTao() {
        const form = document.getElementById("formLienHe");

        if (!form) {
            return;
        }

        form.noValidate = true;

        const cacTruong = Array.from(
            form.querySelectorAll("[data-contact-field]")
        );

        const noiDung = document.getElementById("NoiDung");
        const demKyTu = document.getElementById("demKyTuLienHe");
        const thongBao = document.getElementById("thongBaoLienHe");

        const cacSao = Array.from(
            form.querySelectorAll('input[name="DanhGia"]')
        );

        function capNhatDem() {
            demKyTu.textContent =
                noiDung.value.length + " / 500 ký tự";
        }

        function kiemTra(truong) {
            const giaTri = truong.value.trim();
            let loi = "";

            if (!giaTri) {
                loi = truong.dataset.requiredMessage;
            } else if (
                truong.type === "email" &&
                truong.validity.typeMismatch
            ) {
                loi = "Vui lòng nhập email hợp lệ.";
            } else if (
                truong.id === "SoDienThoai" &&
                !/^0[0-9]{9}$/.test(giaTri.replace(/\s/g, ""))
            ) {
                loi =
                    "Số điện thoại phải bắt đầu bằng 0 và có 10 số.";
            } else if (
                truong.maxLength > 0 &&
                giaTri.length > truong.maxLength
            ) {
                loi = "Nội dung không được vượt quá "
                    + truong.maxLength
                    + " ký tự.";
            } else if (
                truong.id === "NoiDung" &&
                giaTri.length < 20
            ) {
                loi = "Vui lòng nhập nội dung ít nhất 20 ký tự.";
            }

            truong.classList.toggle(
                "is-invalid",
                loi.length > 0
            );

            truong.setAttribute(
                "aria-invalid",
                loi ? "true" : "false"
            );

            document.getElementById(
                truong.id + "Loi"
            ).textContent = loi;

            return loi.length === 0;
        }

        cacTruong.forEach(function (truong) {
            truong.addEventListener("blur", function () {
                kiemTra(truong);
            });

            truong.addEventListener("input", function () {
                thongBao.hidden = true;

                if (truong.classList.contains("is-invalid")) {
                    kiemTra(truong);
                }
            });
        });

        noiDung.addEventListener("input", capNhatDem);
        capNhatDem();

        cacSao.forEach(function (sao) {
            sao.addEventListener("change", function () {
                const diem = Number(sao.value);

                cacSao.forEach(function (item) {
                    item.nextElementSibling.classList.toggle(
                        "is-active",
                        Number(item.value) <= diem
                    );
                });
            });
        });

        form.addEventListener("submit", function (event) {
            event.preventDefault();

            let hopLe = true;

            cacTruong.forEach(function (truong) {
                if (!kiemTra(truong)) {
                    hopLe = false;
                }
            });

            if (!hopLe) {
                form.querySelector(".is-invalid").focus();
                return;
            }

            const chuDe = form.querySelector(
                'input[name="ChuDe"]:checked'
            ).value;

            const danhGia = form.querySelector(
                'input[name="DanhGia"]:checked'
            );

            const tieuDe = "[SPORTIX - " + chuDe + "] "
                + document.getElementById("TieuDe").value.trim();

            const noiDungThu = [
                "Họ tên: "
                + document.getElementById("HoTen").value.trim(),
                "Email: "
                + document.getElementById("Email").value.trim(),
                "Điện thoại: "
                + document.getElementById("SoDienThoai").value.trim(),
                "Chủ đề: " + chuDe,
                "Đánh giá: "
                + (danhGia
                    ? danhGia.value + "/5"
                    : "Chưa đánh giá"),
                "",
                noiDung.value.trim()
            ].join("\r\n");

            thongBao.hidden = false;

            window.location.href = form.action
                + "?subject=" + encodeURIComponent(tieuDe)
                + "&body=" + encodeURIComponent(noiDungThu);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();