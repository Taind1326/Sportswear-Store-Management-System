(function () {
    "use strict";

    function khoiTao() {
        const form = document.getElementById("formSanPham");
        const danhSach = document.getElementById("danhSachPhanLoai");
        const mauDong = document.getElementById("mauDong");
        const nutThem = document.getElementById("themDong");
        const thongBao = document.getElementById("thongBaoPhanLoai");
        const danhSachAnh = document.getElementById("danhSachAnhTheoMau");
        const mauDongAnh = document.getElementById("mauDongAnhTheoMau");
        const $ = window.jQuery;
        const soDongToiDa = 100;
        const boChonMau = '[data-field="MauSac"]';
        const cacDongAnh = new Map();

        if (!form || !danhSach || !mauDong || !nutThem) {
            return;
        }

        if (form.dataset.daKhoiTao === "true") {
            return;
        }

        form.dataset.daKhoiTao = "true";

        const coValidation = !!(
            $ && $.validator && $.validator.unobtrusive
        );

        function layCacDong() {
            return Array.from(danhSach.rows);
        }

        function doiTenCacTruong() {
            const cacDong = layCacDong();

            cacDong.forEach(function (dong, viTri) {
                dong.querySelectorAll("[data-field]").forEach(function (truong) {
                    const ten = truong.dataset.field;
                    const vungLoi = dong.querySelector(
                        '[data-error-field="' + ten + '"]'
                    );

                    truong.name = "PhanLoai[" + viTri + "]." + ten;
                    truong.id = "PhanLoai_" + viTri + "__" + ten;

                    if (vungLoi) {
                        vungLoi.id = truong.id + "_loi";
                        vungLoi.setAttribute("data-valmsg-for", truong.name);
                        vungLoi.setAttribute("data-valmsg-replace", "true");
                        truong.setAttribute("aria-describedby", vungLoi.id);
                    }
                });

                const nutXoa = dong.querySelector(".xoa-dong");

                if (nutXoa) {
                    nutXoa.disabled = cacDong.length <= 1;
                    nutXoa.setAttribute("aria-label", "Xóa phân loại " + (viTri + 1));
                }
            });

            nutThem.disabled = cacDong.length >= soDongToiDa;
        }

        function capNhatAnhTheoMau() {
            if (!danhSachAnh || !mauDongAnh) {
                return;
            }

            const cacMau = new Map();

            danhSach.querySelectorAll(boChonMau).forEach(function (oMau) {
                const mau = oMau.value.trim().replace(/\s+/g, " ");
                const khoa = mau.toUpperCase();

                if (mau && !cacMau.has(khoa)) {
                    cacMau.set(khoa, mau);
                }
            });

            cacDongAnh.forEach(function (dong, khoa) {
                if (!cacMau.has(khoa)) {
                    dong.remove();
                    cacDongAnh.delete(khoa);
                }
            });

            cacMau.forEach(function (mau, khoa) {
                let dong = cacDongAnh.get(khoa);

                if (!dong) {
                    dong = mauDongAnh.content.firstElementChild.cloneNode(true);
                    danhSachAnh.appendChild(dong);
                    cacDongAnh.set(khoa, dong);
                }

                dong.querySelector("[data-mau-anh]").value = mau;
                dong.querySelector("[data-nhan-anh-theo-mau]").textContent =
                    "Ảnh màu " + mau;
            });

            Array.from(danhSachAnh.children).forEach(function (dong, viTri) {
                const tenNhom = "AnhTheoMau[" + viTri + "]";
                const oMau = dong.querySelector("[data-mau-anh]");
                const oTep = dong.querySelector("[data-tep-anh-theo-mau]");
                const nhan = dong.querySelector("[data-nhan-anh-theo-mau]");
                const vungLoi = dong.querySelector("[data-loi-anh-theo-mau]");

                oMau.name = tenNhom + ".MauSac";
                oTep.name = tenNhom + ".TepAnh";
                oTep.id = "AnhTheoMau_" + viTri + "__TepAnh";
                nhan.htmlFor = oTep.id;
                vungLoi.setAttribute("data-valmsg-for", oTep.name);
            });
        }

        function layVungLoi(truong) {
            return $(form)
                .find("[data-valmsg-for]")
                .filter(function () {
                    return truong &&
                        this.getAttribute("data-valmsg-for") === truong.name;
                })
                .first();
        }

        function themQuyTac(truong, quyTac) {
            if (truong) {
                $(truong).rules("add", quyTac);
            }
        }

        function themQuyTacValidation() {
            themQuyTac(form.elements["TenSanPham"], {
                required: true,
                maxlength: 200,
                normalizer: function (value) {
                    return value.trim();
                },
                messages: {
                    required: "Vui lòng nhập tên sản phẩm.",
                    maxlength: "Tên sản phẩm không được vượt quá 200 ký tự."
                }
            });

            themQuyTac(form.elements["TepAnhDaiDien"], {
                required: true,
                messages: {
                    required: "Vui lòng chọn ảnh đại diện cho sản phẩm."
                }
            });

            ["MaLoaiHang", "MaThuongHieu"].forEach(function (ten) {
                const loi = ten === "MaLoaiHang"
                    ? "Vui lòng chọn loại hàng."
                    : "Vui lòng chọn thương hiệu.";

                themQuyTac(form.elements[ten], {
                    required: true,
                    number: true,
                    range: [1, 2147483647],
                    messages: {
                        required: loi,
                        number: loi,
                        range: loi
                    }
                });
            });

            layCacDong().forEach(function (dong) {
                themQuyTac(dong.querySelector(boChonMau), {
                    maxlength: 50,
                    messages: {
                        maxlength: "Màu không được vượt quá 50 ký tự."
                    }
                });

                themQuyTac(dong.querySelector('[data-field="KichThuoc"]'), {
                    maxlength: 30,
                    messages: {
                        maxlength: "Kích cỡ không được vượt quá 30 ký tự."
                    }
                });

                themQuyTac(dong.querySelector('[data-field="GiaBan"]'), {
                    required: true,
                    number: true,
                    min: 0,
                    step: 0.01,
                    messages: {
                        required: "Vui lòng nhập giá gốc.",
                        number: "Vui lòng nhập giá gốc bằng số hợp lệ.",
                        min: "Giá gốc không được nhỏ hơn 0.",
                        step: "Giá gốc chỉ được có tối đa 2 chữ số thập phân.",
                        range: "Giá gốc vượt quá giới hạn cho phép.",
                        max: "Giá gốc vượt quá giới hạn cho phép."
                    }
                });

                themQuyTac(dong.querySelector('[data-field="SoLuongTon"]'), {
                    required: true,
                    number: true,
                    digits: true,
                    min: 0,
                    max: 2147483647,
                    step: 1,
                    messages: {
                        required: "Vui lòng nhập số lượng tồn.",
                        number: "Số lượng tồn phải là số nguyên hợp lệ.",
                        digits: "Số lượng tồn phải là số nguyên không âm.",
                        min: "Số lượng tồn không được nhỏ hơn 0.",
                        max: "Số lượng tồn không được vượt quá 2.147.483.647.",
                        range: "Số lượng tồn phải từ 0 đến 2.147.483.647.",
                        step: "Số lượng tồn phải là số nguyên."
                    }
                });
            });
        }

        function dangKyValidation() {
            if (!coValidation) {
                return;
            }

            $.validator.unobtrusive.parse(form);

            const validator = $(form).validate();

            validator.settings.errorElement = "span";
            validator.settings.errorClass = "input-validation-error";
            validator.settings.focusInvalid = true;

            validator.settings.highlight = function (element) {
                $(element)
                    .addClass("input-validation-error is-invalid")
                    .attr("aria-invalid", "true");
            };

            validator.settings.unhighlight = function (element) {
                $(element)
                    .removeClass("input-validation-error is-invalid")
                    .attr("aria-invalid", "false");
            };

            validator.settings.errorPlacement = function (error, element) {
                const vungLoi = layVungLoi(element[0]);

                error
                    .removeClass("input-validation-error")
                    .addClass("text-danger d-block mt-1");

                if (vungLoi.length) {
                    vungLoi
                        .removeClass("field-validation-valid")
                        .addClass("field-validation-error text-danger")
                        .empty()
                        .append(error);
                } else {
                    error.insertAfter(element);
                }
            };

            validator.settings.success = function (label, element) {
                const vungLoi = layVungLoi(element);

                if (vungLoi.length) {
                    vungLoi
                        .empty()
                        .removeClass("field-validation-error text-danger")
                        .addClass("field-validation-valid");
                }

                $(label).remove();
            };

            themQuyTacValidation();
        }

        function dangKyLaiValidation() {
            if (!coValidation) {
                return;
            }

            const $form = $(form);
            const validator = $form.data("validator");

            if (validator) {
                validator.destroy();
            }

            $form.removeData("unobtrusiveValidation");

            $form.find("[data-valmsg-for]")
                .empty()
                .removeClass("field-validation-error text-danger")
                .addClass("field-validation-valid");

            $form.find("input, select, textarea")
                .removeClass("input-validation-error is-invalid")
                .removeAttr("aria-invalid");

            dangKyValidation();
        }

        function capNhatSauThayDoi() {
            doiTenCacTruong();
            capNhatAnhTheoMau();
            dangKyLaiValidation();

            if (thongBao) {
                thongBao.textContent = "";
            }
        }

        nutThem.addEventListener("click", function () {
            if (layCacDong().length >= soDongToiDa) {
                return;
            }

            danhSach.appendChild(mauDong.content.cloneNode(true));
            capNhatSauThayDoi();

            const oDau = danhSach.lastElementChild.querySelector("input");

            if (oDau) {
                oDau.focus();
            }
        });

        danhSach.addEventListener("click", function (event) {
            const nutXoa = event.target.closest(".xoa-dong");

            if (!nutXoa || !danhSach.contains(nutXoa)) {
                return;
            }

            if (layCacDong().length <= 1) {
                if (thongBao) {
                    thongBao.textContent =
                        "Sản phẩm phải có ít nhất một phân loại.";
                }

                return;
            }

            const dong = nutXoa.closest("tr");

            if (dong) {
                dong.remove();
                capNhatSauThayDoi();
            }
        });

        danhSach.addEventListener("change", function (event) {
            if (event.target.matches(boChonMau)) {
                capNhatAnhTheoMau();
            }
        });

        form.addEventListener("submit", capNhatAnhTheoMau, true);

        if (layCacDong().length === 0) {
            danhSach.appendChild(mauDong.content.cloneNode(true));
        }

        doiTenCacTruong();
        capNhatAnhTheoMau();
        dangKyValidation();
    }

    if (window.jQuery) {
        window.jQuery(khoiTao);
    } else if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", khoiTao);
    } else {
        khoiTao();
    }
})();