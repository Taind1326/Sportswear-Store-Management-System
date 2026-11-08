// Chuông thông báo Admin: tải danh sách từ DB, nhận thông báo mới qua SignalR.
(function ($) {
    "use strict";

    var goc = document.getElementById("chuongThongBao");
    if (!goc || !$.connection || !$.connection.thongBaoHub) { return; }

    var cfg = goc.dataset;
    var elSo = document.getElementById("soChuaDoc");
    var elDs = document.getElementById("danhSachThongBao");
    var csrf = $("#bieuMauCsrf input[name='__RequestVerificationToken']").val();

    // Bundle bootstrap của dự án không kèm Popper nên tự bật/tắt menu chuông.
    var nutChuong = document.getElementById("nutChuong");
    var khung = document.getElementById("khungThongBao");

    function moMenu(mo) {
        khung.classList.toggle("show", mo);
        nutChuong.setAttribute("aria-expanded", mo ? "true" : "false");
    }

    nutChuong.addEventListener("click", function () {
        var dangMo = khung.classList.contains("show");
        moMenu(!dangMo);
        if (!dangMo) { taiDanhSach(); }
    });

    document.addEventListener("click", function (e) {
        if (!goc.contains(e.target)) { moMenu(false); }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") { moMenu(false); }
    });

    function datSo(n) {
        elSo.textContent = n > 99 ? "99+" : n;
        elSo.hidden = !(n > 0);
    }

    function dongHtml(tb) {
        var a = $("<a/>", { href: "#", "class": "dropdown-item border-bottom py-2 text-wrap" })
            .data("tb", tb);
        if (!tb.DaDoc) { a.addClass("fw-semibold bg-light"); }
        $("<div/>").text(tb.TieuDe).appendTo(a);
        $("<div/>", { "class": "small text-secondary" }).text(tb.NoiDung).appendTo(a);
        $("<div/>", { "class": "small text-secondary" }).text(tb.NgayTao).appendTo(a);
        return a;
    }

    function veDanhSach(ds) {
        elDs.innerHTML = "";
        if (!ds.length) {
            $(elDs).append($("<p/>", { "class": "text-secondary text-center py-3 mb-0" }).text("Chưa có thông báo."));
            return;
        }
        ds.forEach(function (tb) { $(elDs).append(dongHtml(tb)); });
    }

    // Luôn đọc từ DB: mất kết nối SignalR vẫn xem được thông báo cũ.
    function taiDanhSach() {
        return $.getJSON(cfg.urlDanhSach).done(function (r) {
            datSo(r.chuaDoc);
            veDanhSach(r.danhSach);
        }).fail(function () {
            elDs.innerHTML = "";
            $(elDs).append($("<p/>", { "class": "text-danger text-center py-3 mb-0" }).text("Không tải được thông báo."));
        });
    }

    $(elDs).on("click", "a.dropdown-item", function (e) {
        e.preventDefault();
        var tb = $(this).data("tb");
        var dich = tb.MaDonHang ? cfg.urlDon + "/" + tb.MaDonHang : null;
        $.post(cfg.urlDaDoc, { id: tb.MaThongBao, __RequestVerificationToken: csrf })
            .always(function () { if (dich) { window.location.href = dich; } else { taiDanhSach(); } });
    });

    $("#nutDocTatCa").on("click", function () {
        $.post(cfg.urlDocTatCa, { __RequestVerificationToken: csrf }).done(taiDanhSach);
    });

    // Nhận thông báo mới
    $.connection.thongBaoHub.client.nhanThongBao = function (tb, chuaDoc) {
        datSo(chuaDoc);
        var rong = $(elDs).find("p.text-secondary");
        if (rong.length) { rong.remove(); }
        $(elDs).prepend(dongHtml(tb));
    };

    // Token được server cấp (Session không có sẵn trong request SignalR).
    $.getJSON(cfg.urlToken).done(function (r) {
        $.connection.hub.qs = { token: r.token };
        $.connection.hub.start().done(taiDanhSach);

        // Kết nối lại xong thì đồng bộ lại từ DB để không sót thông báo.
        $.connection.hub.reconnected(taiDanhSach);

        // Token hết hạn khi cache bị xóa (vd. app pool recycle): xin token mới rồi nối lại.
        $.connection.hub.disconnected(function () {
            setTimeout(function () {
                $.getJSON(cfg.urlToken).done(function (r2) {
                    $.connection.hub.qs = { token: r2.token };
                    $.connection.hub.start().done(taiDanhSach);
                });
            }, 5000);
        });
    }).fail(taiDanhSach);

    taiDanhSach();
})(jQuery);
