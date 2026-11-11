(function () {
    "use strict";

    // ===== Hiện/ẩn mật khẩu (đăng nhập + đăng ký) =====
    document.querySelectorAll("[data-toggle-password]").forEach(function (nut) {
        nut.addEventListener("click", function () {
            var o = nut.closest(".auth-input").querySelector("input");
            var hien = o.type === "password";
            o.type = hien ? "text" : "password";
            nut.querySelector("i").className = hien ? "bi bi-eye-slash" : "bi bi-eye";
            nut.setAttribute("aria-label", hien ? "Ẩn mật khẩu" : "Hiện mật khẩu");
        });
    });

    // ===== Nút Google / Facebook: chưa hỗ trợ =====
    document.querySelectorAll("[data-social]").forEach(function (nut) {
        nut.addEventListener("click", function () {
            var ghiChu = document.getElementById("authSocialNote");
            if (!ghiChu) return;
            ghiChu.textContent = "Đăng nhập bằng " + nut.getAttribute("data-social") +
                " chưa được hỗ trợ. Vui lòng dùng email.";
            ghiChu.hidden = false;
        });
    });

    // ===== Link chưa có trang (điều khoản, chính sách) =====
    document.querySelectorAll("[data-chua-co]").forEach(function (a) {
        a.addEventListener("click", function (e) { e.preventDefault(); });
    });

    // ===== Chống bấm đúp nút gửi ở trang đăng nhập =====
    var formDangNhap = document.getElementById("formDangNhap");
    if (formDangNhap) {
        formDangNhap.addEventListener("submit", function () {
            var nut = formDangNhap.querySelector(".auth-submit");
            if (nut) setTimeout(function () { nut.disabled = true; }, 0);
        });
    }

    // ===== Wizard đăng ký 3 bước =====
    var form = document.getElementById("formDangKy");
    if (!form) return;

    var panes = form.querySelectorAll("[data-pane]");
    var steps = document.querySelectorAll(".auth-step");
    var lines = document.querySelectorAll(".auth-line");
    var thanh = document.getElementById("meterBar");
    var chu = document.getElementById("meterText");
    var hienTai = 1;

    var NHAN_DO_MANH = ["", "Yếu", "Trung bình", "Mạnh", "Rất mạnh \uD83D\uDCAA"];

    var QUY_TAC_MK = {
        len: function (s) { return s.length >= 8; },
        upper: function (s) { return /[A-Z]/.test(s); },
        digit: function (s) { return /\d/.test(s); },
        special: function (s) { return /[^A-Za-z0-9]/.test(s); }
    };

    function o(ten) { return form.elements[ten]; }

    function datLoi(ten, thongBao) {
        var span = form.querySelector('[data-valmsg-for="' + ten + '"]');
        if (span) {
            span.textContent = thongBao || "";
            span.className = "auth-error " + (thongBao ? "field-validation-error" : "field-validation-valid");
        }
        var phanTu = o(ten);
        var khung = phanTu && phanTu.closest ? phanTu.closest(".auth-input") : null;
        if (khung) khung.classList.toggle("is-invalid", !!thongBao);
        return !thongBao;
    }

    function demQuyTac(mk) {
        var d = 0;
        for (var k in QUY_TAC_MK) { if (QUY_TAC_MK[k](mk)) d++; }
        return d;
    }

    function capNhatDoManh() {
        var mk = o("MatKhau").value;
        var dat = 0;

        document.querySelectorAll("[data-rule]").forEach(function (li) {
            var ok = QUY_TAC_MK[li.getAttribute("data-rule")](mk);
            li.classList.toggle("is-met", ok);
            li.querySelector("i").className = ok ? "bi bi-check-circle-fill" : "bi bi-circle";
            if (ok) dat++;
        });

        thanh.style.width = (dat * 25) + "%";
        thanh.className = "auth-meter-bar level-" + dat;
        chu.className = "auth-meter-text level-" + dat;
        chu.textContent = NHAN_DO_MANH[dat];
    }

    function kiemTraBuoc1() {
        var ho = o("Ho").value.trim();
        var ten = o("Ten").value.trim();
        var email = o("Email").value.trim();
        var sdt = o("SoDienThoai").value.trim();

        var a = datLoi("Ho", !ho ? "Vui lòng nhập họ." : (ho.length > 45 ? "Họ tối đa 45 ký tự." : ""));
        var b = datLoi("Ten", !ten ? "Vui lòng nhập tên." : (ten.length > 45 ? "Tên tối đa 45 ký tự." : ""));
        var c = datLoi("Email", !email ? "Vui lòng nhập email."
            : (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) ? "Email không hợp lệ." : ""));
        var d = datLoi("SoDienThoai", !sdt ? "Vui lòng nhập số điện thoại."
            : (!/^(0\d{9}|\+84\d{9})$/.test(sdt) ? "Số điện thoại không hợp lệ (ví dụ 0987654321)." : ""));

        return a && b && c && d;
    }

    function kiemTraBuoc2() {
        var mk = o("MatKhau").value;
        var xn = o("XacNhanMatKhau").value;

        var a = datLoi("MatKhau", !mk ? "Vui lòng nhập mật khẩu."
            : (demQuyTac(mk) < 4 ? "Mật khẩu chưa đủ mạnh. Hãy đáp ứng đủ các yêu cầu bên dưới." : ""));
        var b = datLoi("XacNhanMatKhau", !xn ? "Vui lòng nhập lại mật khẩu."
            : (xn !== mk ? "Mật khẩu nhập lại không khớp." : ""));

        return a && b;
    }

    function kiemTraBuoc3() {
        return datLoi("DongYDieuKhoan",
            o("DongYDieuKhoan").checked ? "" : "Bạn cần đồng ý với điều khoản để tiếp tục.");
    }

    var kiemTra = { 1: kiemTraBuoc1, 2: kiemTraBuoc2, 3: kiemTraBuoc3 };

    function dienTomTat() {
        var hoTen = (o("Ho").value.trim() + " " + o("Ten").value.trim()).trim();
        document.getElementById("sumTen").textContent = hoTen;
        document.getElementById("sumEmail").textContent = o("Email").value.trim();
        document.getElementById("sumSdt").textContent = o("SoDienThoai").value.trim();
    }

    function hienBuoc(n, khoiTao) {
        hienTai = n;

        panes.forEach(function (p) {
            p.hidden = Number(p.getAttribute("data-pane")) !== n;
        });

        steps.forEach(function (s) {
            var i = Number(s.getAttribute("data-step"));
            s.classList.toggle("is-done", i < n);
            s.classList.toggle("is-active", i === n);
        });

        lines.forEach(function (l, idx) {
            l.classList.toggle("is-done", idx + 1 < n);
        });

        if (n === 3) dienTomTat();

        if (!khoiTao) {
            var pane = form.querySelector('[data-pane="' + n + '"]');
            var dau = pane ? pane.querySelector("input:not([type=hidden])") : null;
            if (dau) dau.focus();
        }
    }

    function tiep() {
        if (kiemTra[hienTai]() && hienTai < 3) hienBuoc(hienTai + 1);
    }

    form.querySelectorAll("[data-next]").forEach(function (nut) {
        nut.addEventListener("click", tiep);
    });

    form.querySelectorAll("[data-back]").forEach(function (nut) {
        nut.addEventListener("click", function () {
            if (hienTai > 1) hienBuoc(hienTai - 1);
        });
    });

    // Enter ở bước 1, 2: chuyển bước thay vì gửi form
    form.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && hienTai < 3 && e.target.tagName === "INPUT") {
            e.preventDefault();
            tiep();
        }
    });

    // Xóa lỗi khi người dùng sửa lại
    form.addEventListener("input", function (e) {
        if (e.target && e.target.name) datLoi(e.target.name, "");
    });
    form.addEventListener("change", function (e) {
        if (e.target && e.target.name) datLoi(e.target.name, "");
    });

    o("MatKhau").addEventListener("input", capNhatDoManh);

    form.addEventListener("submit", function (e) {
        var ok1 = kiemTraBuoc1();
        var ok2 = kiemTraBuoc2();
        var ok3 = kiemTraBuoc3();

        if (!ok1) { e.preventDefault(); hienBuoc(1); return; }
        if (!ok2) { e.preventDefault(); hienBuoc(2); return; }
        if (!ok3) { e.preventDefault(); hienBuoc(3); return; }

        var nut = form.querySelector(".auth-submit[type=submit]");
        if (nut) setTimeout(function () { nut.disabled = true; }, 0);
    });

    // Khởi tạo: nếu server trả lỗi thì mở đúng bước chứa lỗi
    capNhatDoManh();

    var buoc = 1;
    var loiServer = form.querySelector(".field-validation-error");
    if (loiServer) {
        var pane = loiServer.closest("[data-pane]");
        if (pane) buoc = Number(pane.getAttribute("data-pane"));
    }
    hienBuoc(buoc, true);
})();