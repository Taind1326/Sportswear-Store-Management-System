const $ = (id) => document.getElementById(id);

let selectedCategory = "all";
let currentSlide = 0;
let sliderTimer = null;
let editingProductId = null;

/* ───────── DATA ───────── */

let products =
    JSON.parse(localStorage.getItem("sportix_products")) || [];

const orders = [
    { id: "#SX1024", customer: "Nguyễn Hải Yến", product: "Nike Air Zoom", total: 2490000, payment: "MoMo", status: "Đang giao" },
    { id: "#SX1023", customer: "Trần Minh Anh", product: "Adidas Training", total: 690000, payment: "COD", status: "Chờ xử lý" },
    { id: "#SX1022", customer: "Lê Hoàng Nam", product: "Balo SPORTIX", total: 450000, payment: "Banking", status: "Đã giao" },
    { id: "#SX1021", customer: "Phạm Thị Thu", product: "Bình nước Active", total: 150000, payment: "MoMo", status: "Đã giao" },
    { id: "#SX1020", customer: "Võ Văn Đức", product: "Áo khoác Runner", total: 890000, payment: "COD", status: "Đã huỷ" }
];

const allOrders = [...orders];

const customers = [
    { id: "KH001", name: "Nguyễn Văn A", email: "vana@gmail.com", orders: 12, spend: 8500000 },
    { id: "KH002", name: "Trần Thị B", email: "thib@gmail.com", orders: 7, spend: 4200000 },
    { id: "KH003", name: "Lê Văn C", email: "vanc@gmail.com", orders: 15, spend: 11300000 },
    { id: "KH004", name: "Phạm Thị D", email: "thid@gmail.com", orders: 4, spend: 2100000 },
    { id: "KH005", name: "Hoàng Văn E", email: "vane@gmail.com", orders: 9, spend: 6700000 }
];

const sales = [
    { day: "T2", value: 12 },
    { day: "T3", value: 18 },
    { day: "T4", value: 10 },
    { day: "T5", value: 24 },
    { day: "T6", value: 20 },
    { day: "T7", value: 16 },
    { day: "CN", value: 14 }
];

const activities = [
    { icon: "bi-bag-check-fill", title: "Đơn hàng mới", text: "#SX1024 vừa được tạo thành công." },
    { icon: "bi-box-seam-fill", title: "Cập nhật sản phẩm", text: "Áo khoác Runner đã được cập nhật tồn kho." },
    { icon: "bi-person-check-fill", title: "Khách hàng mới", text: "Nguyễn Hải Yến vừa đăng ký tài khoản." }
];

const notifications = [
    { icon: "bi-bag-check-fill", color: "blue", title: "Đơn hàng mới #SX1024", time: "2 phút trước", read: false },
    { icon: "bi-exclamation-triangle-fill", color: "yellow", title: "Bình nước Active sắp hết hàng", time: "15 phút trước", read: false },
    { icon: "bi-person-plus-fill", color: "green", title: "Khách hàng mới đăng ký", time: "1 giờ trước", read: true },
    { icon: "bi-arrow-repeat", color: "blue", title: "Đồng bộ tồn kho thành công", time: "3 giờ trước", read: true }
];

const settings = [
    { icon: "bi-shop", title: "Thông tin cửa hàng", text: "Quản lý tên cửa hàng, logo và thông tin liên hệ." },
    { icon: "bi-shield-lock-fill", title: "Tài khoản quản trị", text: "Đổi mật khẩu và bảo mật tài khoản admin." },
    { icon: "bi-bell-fill", title: "Thông báo", text: "Quản lý email và thông báo hệ thống." }
];

/* ───────── HELPERS ───────── */

function formatMoney(value) {
    return value.toLocaleString("vi-VN") + "đ";
}

function getProductStatus(stock) {
    if (stock <= 5) return { text: "Sắp hết", className: "cancel" };
    if (stock <= 12) return { text: "Cần nhập", className: "pending" };
    return { text: "Đang bán", className: "done" };
}

function getOrderStatusClass(status) {
    if (status === "Đã giao") return "done";
    if (status === "Đang giao") return "shipping";
    if (status === "Chờ xử lý") return "pending";
    return "cancel";
}

function getAdminName() {
    const user = localStorage.getItem("sportix_user") || localStorage.getItem("admin_user");
    if (!user) return "Admin";
    try { const d = JSON.parse(user); return d.name || d.email || "Admin"; }
    catch { return "Admin"; }
}

/* ───────── TOAST ───────── */

function showAdminToast(message, type = "success") {
    const toast = $("adminToast");
    if (!toast) return;
    const icon = toast.querySelector("i");
    icon.className = type === "error" ? "bi bi-x-circle-fill" : "bi bi-check-circle-fill";
    icon.style.color = type === "error" ? "var(--danger)" : "var(--success)";
    toast.querySelector("span").textContent = message;
    toast.classList.add("show");
    clearTimeout(showAdminToast.timer);
    showAdminToast.timer = setTimeout(() => toast.classList.remove("show"), 2200);
}

/* ───────── STATS ───────── */

function renderStats() {
    const statsBox = document.querySelector(".stats-grid");
    if (!statsBox) return;

    const revenue = orders.reduce((s, o) => s + o.total, 0);
    const lowStock = products.filter(p => p.stock <= 12).length;
    const unread = notifications.filter(n => !n.read).length;

    statsBox.innerHTML = `
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-cash-stack"></i></div><div><p>Doanh thu</p><h3>${(revenue / 1000000).toFixed(1)}M</h3><span>+12.4%</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-bag-check-fill"></i></div><div><p>Đơn hàng</p><h3>${orders.length}</h3><span>Hôm nay</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-box-seam-fill"></i></div><div><p>Sản phẩm</p><h3>${products.length}</h3><span>Đang bán</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-exclamation-triangle-fill"></i></div><div><p>Cảnh báo</p><h3>${lowStock}</h3><span class="danger">Tồn kho thấp</span></div></div>
  `;
}

/* ───────── SALES CHART ───────── */

function renderSalesChart() {
    const chart = $("salesChart");
    if (!chart) return;
    const max = Math.max(...sales.map(s => s.value));
    chart.innerHTML = sales.map((item, i) => `
    <div class="chart-bar ${i === 4 ? "today" : ""}" style="height:${(item.value / max) * 100}%" title="${item.day}: ${item.value}M đ">
      <strong>${item.value}M</strong>
      <span>${item.day}</span>
    </div>
  `).join("");
}

/* ───────── BEST PRODUCTS ───────── */

function renderBestProducts() {
    const list = $("bestList");
    if (!list) return;
    list.innerHTML = [...products]
        .sort((a, b) => b.sold - a.sold)
        .slice(0, 4)
        .map((item, i) => `
      <div class="best-item">
        <span class="best-rank">${i + 1}</span>
        <img src="${item.img}" class="product-thumb" alt="${item.name}">
        <div class="item-info"><h4>${item.name}</h4><p>Đã bán <b>${item.sold}</b> · ${formatMoney(item.price)}</p></div>
      </div>
    `).join("");
}

/* ───────── VOUCHER MANAGEMENT ───────── */

// Khởi tạo danh sách mã giảm giá mặc định nếu chưa có trong hệ thống
function initVouchersLocalStorage() {
    if (!localStorage.getItem("sportix_vouchers")) {
        const defaultVouchers = [
            { code: "SPORTIX10", type: "percent", value: 10, minOrder: 0 },
            { code: "GIAM30K", type: "fixed", value: 30000, minOrder: 100000 }
        ];
        localStorage.setItem("sportix_vouchers", JSON.stringify(defaultVouchers));
    }
}

function getVouchers() {
    initVouchersLocalStorage();
    return JSON.parse(localStorage.getItem("sportix_vouchers"));
}

function saveVouchers(vouchers) {
    localStorage.setItem("sportix_vouchers", JSON.stringify(vouchers));
    renderVouchers();
}

function renderVouchers() {
    const table = $("voucherTable");
    if (!table) return;

    const vouchers = getVouchers();

    if (!vouchers.length) {
        table.innerHTML = `<tr><td colspan="5" style="text-align:center;color:var(--muted);padding:28px">Chưa có mã giảm giá nào được tạo.</td></tr>`;
        return;
    }

    table.innerHTML = vouchers.map(v => `
      <tr>
        <td><b class="text-uppercase text-primary">${v.code}</b></td>
        <td>${v.type === "percent" ? "Giảm theo %" : "Giảm số tiền cố định"}</td>
        <td>${v.type === "percent" ? `${v.value}%` : formatMoney(v.value)}</td>
        <td>${formatMoney(v.minOrder || 0)}</td>
        <td>
          <div class="action-group">
            <button class="action-btn action-btn-del" type="button" title="Xoá" onclick="deleteVoucher('${v.code}')">
              <i class="bi bi-trash-fill"></i>
            </button>
          </div>
        </td>
      </tr>
    `).join("");
}

function buildVoucherModal() {
    if ($("voucherModal")) return;
    document.body.insertAdjacentHTML("beforeend", `
    <div class="admin-modal" id="voucherModal">
      <div class="admin-modal-card" style="max-width:450px;text-align:left">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:18px">
          <div><span class="eyebrow">MÃ GIẢM GIÁ</span><h3 style="margin:6px 0 0">Thêm mã giảm giá mới</h3></div>
          <button type="button" onclick="closeVoucherModal()" style="width:38px;height:38px;border:none;border-radius:13px;background:var(--blue-soft);color:var(--blue-dark)"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="row g-3">
          <div class="col-12">
            <label class="sz-label">Mã code (Viết liền không dấu)</label>
            <input id="vm_code" type="text" class="sz-input" placeholder="VD: NHAPMA30" style="padding:0 14px; text-transform: uppercase;">
            <small style="color: var(--muted); font-size: 11px; display: block; margin-top: 4px;">Hệ thống sẽ tự động viết liền và loại bỏ dấu tiếng Việt khi bạn gõ.</small>
          </div>
          <div class="col-12">
            <label class="sz-label">Loại giảm giá</label>
            <select id="vm_type" class="sz-input" style="padding:0 14px" onchange="toggleVoucherInputPlaceholder()">
              <option value="percent">Giảm theo phần trăm (%)</option>
              <option value="fixed">Giảm số tiền trực tiếp (đ)</option>
            </select>
          </div>
          <div class="col-6">
            <label class="sz-label" id="vm_value_label">Giá trị giảm (%)</label>
            <input id="vm_value" type="number" class="sz-input" placeholder="VD: 10" style="padding:0 14px">
          </div>
          <div class="col-6">
            <label class="sz-label">Đơn tối thiểu (đ)</label>
            <input id="vm_minOrder" type="number" class="sz-input" placeholder="VD: 0" style="padding:0 14px" value="0">
          </div>
        </div>
        <div class="admin-modal-actions" style="margin-top:22px">
          <button type="button" class="btn-cancel" onclick="closeVoucherModal()">Huỷ</button>
          <button type="button" class="btn-logout" onclick="addVoucher()"><i class="bi bi-check-lg"></i> Tạo mã</button>
        </div>
      </div>
    </div>
    `);

    // Gắn sự kiện tự động định dạng mã (Xóa dấu, xóa khoảng trắng, viết hoa)
    $("vm_code").addEventListener("input", function () {
        this.value = this.value
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[đĐ]/g, 'd')
            .replace(/\s+/g, '')
            .replace(/[^a-zA-Z0-9]/g, '')
            .toUpperCase();
    });
}

function toggleVoucherInputPlaceholder() {
    const type = $("vm_type").value;
    $("vm_value_label").textContent = type === "percent" ? "Giá trị giảm (%)" : "Giá trị giảm (đ)";
    $("vm_value").placeholder = type === "percent" ? "VD: 10" : "VD: 50000";
}

function openVoucherModal() {
    buildVoucherModal();
    $("vm_code").value = "";
    $("vm_value").value = "";
    $("vm_minOrder").value = "0";
    $("vm_type").value = "percent";
    toggleVoucherInputPlaceholder();
    $("voucherModal").classList.add("show");
}

function closeVoucherModal() {
    $("voucherModal").classList.remove("show");
}

function addVoucher() {
    const code = $("vm_code").value.trim().toUpperCase();
    const type = $("vm_type").value;
    const value = parseInt($("vm_value").value);
    const minOrder = parseInt($("vm_minOrder").value) || 0;

    // Ràng buộc kiểm tra cuối cùng trước khi lưu: Chỉ cho phép Chữ và Số
    const perfectCodeRegex = /^[A-Z0-9]+$/;

    if (!code || !perfectCodeRegex.test(code)) {
        showAdminToast("Mã code không hợp lệ! Vui lòng nhập viết liền, không dấu.", "error");
        $("vm_code").focus();
        return;
    }

    if (isNaN(value) || value <= 0) {
        showAdminToast("Vui lòng điền đầy đủ thông tin hợp lệ!", "error");
        return;
    }

    if (type === "percent" && value > 100) {
        showAdminToast("Phần trăm giảm giá không thể lớn hơn 100%!", "error");
        return;
    }

    const vouchers = getVouchers();
    if (vouchers.some(v => v.code === code)) {
        showAdminToast("Mã giảm giá này đã tồn tại!", "error");
        return;
    }

    vouchers.push({ code, type, value, minOrder });
    saveVouchers(vouchers);
    closeVoucherModal();
    showAdminToast("Thêm mã giảm giá thành công!");
}

function deleteVoucher(code) {
    if (confirm(`Bạn có chắc chắn muốn xóa mã giảm giá ${code} không?`)) {
        let vouchers = getVouchers();
        vouchers = vouchers.filter(v => v.code !== code);
        saveVouchers(vouchers);
        showAdminToast(`Đã xóa mã giảm giá ${code}`);
    }
}

// Tìm hàm DOMContentLoaded hiện có ở cuối file admin.js của bạn, và thêm gọi hàm render:
document.addEventListener("DOMContentLoaded", () => {
    // ... các hàm render cũ của bạn ...
    renderVouchers(); // Thêm dòng này vào cuối hàm
});

/* ───────── LOW STOCK ───────── */

function renderLowStock() {
    const list = $("lowStockList");
    if (!list) return;

    list.innerHTML = [...products]
        .filter(p => p.stock <= 12)
        .sort((a, b) => a.stock - b.stock)
        .slice(0, 4)
        .map(item => `
            <div class="low-stock-item">
                <img src="${item.img}" class="product-thumb" alt="${item.name}">
                <div class="item-info">
                    <h4>${item.name}</h4>
                    <p>Còn ${item.stock} sản phẩm</p>
                </div>
                <span class="badge-status ${item.stock <= 5 ? "cancel" : "pending"}">
                    ${item.stock <= 5 ? "Sắp hết" : "Cần nhập"}
                </span>
            </div>
        `).join("");
}

/* ───────── QUICK INFO ───────── */

function renderQuickInfo() {
    const alertList = $("alertList");
    const activityList = $("activityList");

    if (alertList) {
        const low = products.filter(p => p.stock <= 12);
        alertList.innerHTML = low.length
            ? low.map(item => `
          <div class="quick-item">
            <div class="quick-icon"><i class="bi bi-exclamation-triangle-fill"></i></div>
            <div><h4>${item.name}</h4><p>Còn ${item.stock} sản phẩm, cần nhập thêm.</p></div>
          </div>
        `).join("")
            : `<p style="color:var(--muted);padding:12px 0">Không có cảnh báo nào.</p>`;
    }

    if (activityList) {
        activityList.innerHTML = activities.map(item => `
      <div class="quick-item">
        <div class="quick-icon"><i class="bi ${item.icon}"></i></div>
        <div><h4>${item.title}</h4><p>${item.text}</p></div>
      </div>
    `).join("");
    }
}

/* ───────── PRODUCTS TABLE ───────── */

function renderProducts() {
    const table = $("productTable");
    if (!table) return;

    const keyword = ($("searchInput")?.value || "").toLowerCase().trim();
    const result = products
        .filter(p => p.name.toLowerCase().includes(keyword) || p.id.toLowerCase().includes(keyword) || p.category.toLowerCase().includes(keyword))
        .filter(p => selectedCategory === "all" || p.category === selectedCategory);

    if (!result.length) {
        table.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--muted);padding:28px">Không tìm thấy sản phẩm nào.</td></tr>`;
        return;
    }

    table.innerHTML = result.map(item => {
        const status = getProductStatus(item.stock);
        return `
      <tr>
        <td><div class="product-cell"><img src="${item.img}" class="product-thumb" alt="${item.name}"><div><strong>${item.name}</strong><span>${item.id}</span></div></div></td>
        <td>
         ${
            item.category === "giay"
                ? "Giày"
                : item.category === "ao"
                    ? "Áo"
                    : item.category === "phukien"
                    ? "Phụ kiện"
                    : item.category === "gym"
                        ? "Gym"
                        : item.category === "bongda"
                            ?"Bóng đá "
                        : item.category
           }
        </td>
        <td>${formatMoney(item.price)}</td>
        <td>${item.stock}</td>
        <td><span class="badge-status ${status.className}">${status.text}</span></td>
        <td>
          <div class="action-group">
            <button class="action-btn" type="button" title="Sửa" onclick="openProductModal('${item.id}')"><i class="bi bi-pencil-fill"></i></button>
            <button class="action-btn action-btn-del" type="button" title="Xoá" onclick="confirmDeleteProduct('${item.id}')"><i class="bi bi-trash-fill"></i></button>
          </div>
        </td>
      </tr>
    `;
    }).join("");
}

/* ───────── ORDERS TABLE ───────── */

let showAllOrders = false;

function renderOrders() {
    const table = $("orderTable");
    if (!table) return;
    const list = showAllOrders ? allOrders : allOrders.slice(0, 3);
    table.innerHTML = list.map(item => `
    <tr>
      <td><b>${item.id}</b></td>
      <td>${item.customer}</td>
      <td>${item.product}</td>
      <td>${formatMoney(item.total)}</td>
      <td>${item.payment}</td>
      <td>
        <span class="badge-status ${getOrderStatusClass(item.status)}" style="cursor:pointer" onclick="cycleOrderStatus('${item.id}')" title="Click để đổi trạng thái">${item.status}</span>
      </td>
    </tr>
  `).join("");
}

function cycleOrderStatus(orderId) {
    const order = allOrders.find(o => o.id === orderId);
    if (!order) return;
    const cycle = ["Chờ xử lý", "Đang giao", "Đã giao", "Đã huỷ"];
    const idx = cycle.indexOf(order.status);
    order.status = cycle[(idx + 1) % cycle.length];
    renderOrders();
    showAdminToast(`Đã cập nhật trạng thái đơn ${orderId}`);
}

function toggleAllOrders() {
    showAllOrders = !showAllOrders;
    renderOrders();
    const btn = document.querySelector("#donhang .admin-soft-btn");
    if (btn) btn.textContent = showAllOrders ? "Thu gọn" : "Xem tất cả";
}

/* ───────── CUSTOMERS TABLE ───────── */

let showAllCustomers = false;

function renderCustomers() {
    const table = $("customerTable");
    if (!table) return;
    const list = showAllCustomers ? customers : customers.slice(0, 3);
    table.innerHTML = list.map(item => `
    <tr>
      <td>${item.id}</td>
      <td><b>${item.name}</b></td>
      <td>${item.email}</td>
      <td>${item.orders}</td>
      <td>${formatMoney(item.spend)}</td>
    </tr>
  `).join("");
}

function toggleAllCustomers() {
    showAllCustomers = !showAllCustomers;
    renderCustomers();
    const btn = document.querySelector("#khachhang .admin-soft-btn");
    if (btn) btn.textContent = showAllCustomers ? "Thu gọn" : "Xem tất cả";
}

/* ───────── SETTINGS ───────── */

function renderSettings() {
    const grid = $("settingGrid");
    if (!grid) return;
    grid.innerHTML = settings.map((item, i) => `
    <div class="col-md-4">
      <div class="setting-card" style="cursor:pointer" onclick="openSettingModal(${i})">
        <i class="bi ${item.icon}"></i>
        <h5>${item.title}</h5>
        <p>${item.text}</p>
        <span class="setting-btn">Quản lý <i class="bi bi-arrow-right-short"></i></span>
      </div>
    </div>
  `).join("");
}

/* ───────── PRODUCT MODAL (add/edit) ───────── */

function buildProductModal() {
    if ($("productModal")) return;
    document.body.insertAdjacentHTML("beforeend", `
    <div class="admin-modal" id="productModal">
      <div class="admin-modal-card" style="max-width:500px;text-align:left">
        <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:18px">
          <div><span class="eyebrow" id="productModalLabel">THÊM SẢN PHẨM</span><h3 id="productModalTitle" style="margin:6px 0 0">Sản phẩm mới</h3></div>
          <button type="button" onclick="closeProductModal()" style="width:38px;height:38px;border:none;border-radius:13px;background:var(--blue-soft);color:var(--blue-dark)"><i class="bi bi-x-lg"></i></button>
        </div>
 
        <div class="row g-3">
          <div class="col-12">
            <label class="sz-label">Tên sản phẩm</label>
            <input id="pm_name" type="text" class="sz-input" placeholder="Nhập tên sản phẩm" style="padding:0 14px">
          </div>
          <div class="col-6">
            <label class="sz-label">Danh mục</label>
            <select id="pm_category" class="sz-input" style="padding:0 14px">
              <option value="Giày">Giày</option>
              <option value="Áo">Áo</option>
              <option value="Phụ kiện">Phụ kiện</option>
            </select>
          </div>
          <div class="col-6">
            <label class="sz-label">Giá (đ)</label>
            <input id="pm_price" type="number" class="sz-input" placeholder="VD: 990000" style="padding:0 14px">
          </div>
          <div class="col-6">
            <label class="sz-label">Tồn kho</label>
            <input id="pm_stock" type="number" class="sz-input" placeholder="VD: 50" style="padding:0 14px">
          </div>
          <div class="col-6">
            <label class="sz-label">Đã bán</label>
            <input id="pm_sold" type="number" class="sz-input" placeholder="VD: 0" style="padding:0 14px">
          </div>
        </div>
 
        <div class="admin-modal-actions" style="margin-top:22px">
          <button type="button" class="btn-cancel" onclick="closeProductModal()">Huỷ</button>
          <button type="button" class="btn-logout" onclick="saveProduct()"><i class="bi bi-check-lg"></i> Lưu sản phẩm</button>
        </div>
      </div>
    </div>
  `);

    $("productModal").addEventListener("click", e => {
        if (e.target === $("productModal")) closeProductModal();
    });
}

function openProductModal(productId = null) {
    buildProductModal();
    editingProductId = productId;
    const label = $("productModalLabel");
    const title = $("productModalTitle");

    if (productId) {
        const p = products.find(x => x.id === productId);
        if (!p) return;
        label.textContent = "SỬA SẢN PHẨM";
        title.textContent = p.name;
        $("pm_name").value = p.name;
        $("pm_category").value = p.category;
        $("pm_price").value = p.price;
        $("pm_stock").value = p.stock;
        $("pm_sold").value = p.sold;
    } else {
        label.textContent = "THÊM SẢN PHẨM";
        title.textContent = "Sản phẩm mới";
        $("pm_name").value = $("pm_price").value = $("pm_stock").value = $("pm_sold").value = "";
        $("pm_category").value = "Giày";
    }

    $("productModal").classList.add("show");
}

function closeProductModal() {
    $("productModal")?.classList.remove("show");
    editingProductId = null;
}

function saveProduct() {
    const name = $("pm_name").value.trim();
    const category = $("pm_category").value;
    const price = parseInt($("pm_price").value) || 0;
    const stock = parseInt($("pm_stock").value) || 0;
    const sold = parseInt($("pm_sold").value) || 0;

    if (!name) { showAdminToast("Vui lòng nhập tên sản phẩm.", "error"); $("pm_name").focus(); return; }
    if (price <= 0) { showAdminToast("Giá phải lớn hơn 0.", "error"); $("pm_price").focus(); return; }

    if (editingProductId) {
        const p = products.find(x => x.id === editingProductId);
        if (p) { p.name = name; p.category = category; p.price = price; p.stock = stock; p.sold = sold; }
        showAdminToast("Đã cập nhật sản phẩm thành công.");
    } else {
        const newId = "SP" + String(products.length + 1).padStart(3, "0");
        products.push({ id: newId, name, category, price, stock, sold, image: "../img/shoe-1.jpg" });
        showAdminToast("Đã thêm sản phẩm mới thành công.");
    }

    closeProductModal();
    renderProducts();
    renderStats();
    renderBestProducts();
    renderLowStock();
    renderQuickInfo();
    renderSalesChart();
}

/* ───────── DELETE PRODUCT ───────── */

function buildDeleteModal() {
    if ($("deleteModal")) return;
    document.body.insertAdjacentHTML("beforeend", `
    <div class="admin-modal" id="deleteModal">
      <div class="admin-modal-card" style="max-width:380px">
        <div class="admin-modal-icon" style="background:#FFF1F1;color:#EF4444"><i class="bi bi-trash-fill"></i></div>
        <h3>Xoá sản phẩm?</h3>
        <p id="deleteModalText">Bạn có chắc muốn xoá sản phẩm này không?</p>
        <div class="admin-modal-actions">
          <button type="button" class="btn-cancel" onclick="$('deleteModal').classList.remove('show')">Huỷ</button>
          <button type="button" id="confirmDeleteBtn" class="btn-logout" style="background:linear-gradient(135deg,#EF4444,#DC2626)">Xoá <i class="bi bi-arrow-right"></i></button>
        </div>
      </div>
    </div>
  `);
    $("deleteModal").addEventListener("click", e => {
        if (e.target === $("deleteModal")) $("deleteModal").classList.remove("show");
    });
}

let pendingDeleteId = null;

function confirmDeleteProduct(productId) {
    buildDeleteModal();
    const p = products.find(x => x.id === productId);
    if (!p) return;
    pendingDeleteId = productId;
    $("deleteModalText").textContent = `Xoá "${p.name}"? Hành động này không thể hoàn tác.`;
    $("confirmDeleteBtn").onclick = doDeleteProduct;
    $("deleteModal").classList.add("show");
}

function doDeleteProduct() {
    const idx = products.findIndex(x => x.id === pendingDeleteId);
    if (idx !== -1) {
        const name = products[idx].name;
        products.splice(idx, 1);
        showAdminToast(`Đã xoá "${name}".`);
    }
    $("deleteModal").classList.remove("show");
    pendingDeleteId = null;
    renderProducts();
    renderStats();
    renderLowStock();
    renderQuickInfo();
}

/* ───────── SETTING MODAL ───────── */

const settingDetails = [
    {
        title: "Thông tin cửa hàng",
        icon: "bi-shop",
        fields: [
            { label: "Tên cửa hàng", id: "s_storename", type: "text", placeholder: "SPORTIX", value: "SPORTIX" },
            { label: "Email liên hệ", id: "s_email", type: "email", placeholder: "admin@sportix.vn", value: "admin@sportix.vn" },
            { label: "Hotline", id: "s_phone", type: "tel", placeholder: "0909 000 000", value: "0909 000 000" }
        ]
    },
    {
        title: "Tài khoản quản trị",
        icon: "bi-shield-lock-fill",
        fields: [
            { label: "Tên hiển thị", id: "s_adminname", type: "text", placeholder: "Admin", value: "Admin" },
            { label: "Mật khẩu mới", id: "s_newpass", type: "password", placeholder: "••••••••", value: "" },
            { label: "Xác nhận mật khẩu", id: "s_confpass", type: "password", placeholder: "••••••••", value: "" }
        ]
    },
    {
        title: "Thông báo",
        icon: "bi-bell-fill",
        fields: [
            { label: "Email nhận thông báo", id: "s_notimail", type: "email", placeholder: "notify@sportix.vn", value: "notify@sportix.vn" }
        ],
        toggles: [
            { label: "Thông báo đơn hàng mới", id: "t_order", checked: true },
            { label: "Thông báo tồn kho thấp", id: "t_stock", checked: true },
            { label: "Thông báo khách hàng mới", id: "t_customer", checked: false }
        ]
    }
];

function buildSettingModal() {
    if ($("settingModal")) return;
    document.body.insertAdjacentHTML("beforeend", `
    <div class="admin-modal" id="settingModal">
      <div class="admin-modal-card" style="max-width:480px;text-align:left" id="settingModalInner"></div>
    </div>
  `);
    $("settingModal").addEventListener("click", e => {
        if (e.target === $("settingModal")) $("settingModal").classList.remove("show");
    });
}

function openSettingModal(index) {
    buildSettingModal();
    const detail = settingDetails[index];
    const fieldsHtml = detail.fields.map(f => `
    <div style="margin-bottom:14px">
      <label class="sz-label">${f.label}</label>
      <input id="${f.id}" type="${f.type}" class="sz-input" placeholder="${f.placeholder}" value="${f.value}" style="padding:0 14px">
    </div>
  `).join("");

    const togglesHtml = detail.toggles ? `
    <div style="margin-top:8px;display:flex;flex-direction:column;gap:12px">
      ${detail.toggles.map(t => `
        <label style="display:flex;align-items:center;justify-content:space-between;cursor:pointer;gap:10px">
          <span style="font-weight:700;font-size:.9rem">${t.label}</span>
          <input type="checkbox" id="${t.id}" ${t.checked ? "checked" : ""} style="width:18px;height:18px;accent-color:var(--blue)">
        </label>
      `).join("")}
    </div>
  ` : "";

    $("settingModalInner").innerHTML = `
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:18px">
      <div><span class="eyebrow">CÀI ĐẶT</span><h3 style="margin:6px 0 0">${detail.title}</h3></div>
      <button type="button" onclick="$('settingModal').classList.remove('show')" style="width:38px;height:38px;border:none;border-radius:13px;background:var(--blue-soft);color:var(--blue-dark)"><i class="bi bi-x-lg"></i></button>
    </div>
    ${fieldsHtml}
    ${togglesHtml}
    <div class="admin-modal-actions" style="margin-top:18px">
      <button type="button" class="btn-cancel" onclick="$('settingModal').classList.remove('show')">Huỷ</button>
      <button type="button" class="btn-logout" onclick="saveSetting()"><i class="bi bi-check-lg"></i> Lưu thay đổi</button>
    </div>
  `;

    $("settingModal").classList.add("show");
}

function saveSetting() {
    $("settingModal").classList.remove("show");
    showAdminToast("Đã lưu cài đặt thành công.");
}

/* ───────── NOTIFICATIONS PANEL ───────── */

function buildNotifPanel() {
    if ($("notifPanel")) return;
    document.body.insertAdjacentHTML("beforeend", `
    <div class="notif-panel" id="notifPanel">
      <div class="notif-header">
        <h4>Thông báo</h4>
        <button type="button" onclick="markAllRead()" class="admin-soft-btn" style="font-size:.78rem;min-height:34px;padding:0 12px">Đánh dấu đã đọc</button>
      </div>
      <div class="notif-list" id="notifList"></div>
    </div>
  `);

    document.addEventListener("click", e => {
        const panel = $("notifPanel");
        const btn = document.querySelector(".top-icon");
        if (panel && !panel.contains(e.target) && e.target !== btn && !btn?.contains(e.target)) {
            panel.classList.remove("show");
        }
    });
}

function renderNotifs() {
    const list = $("notifList");
    if (!list) return;
    list.innerHTML = notifications.map((n, i) => `
    <div class="notif-item ${n.read ? "" : "unread"}" onclick="readNotif(${i})">
      <div class="notif-icon notif-icon-${n.color}"><i class="bi ${n.icon}"></i></div>
      <div class="notif-body"><p>${n.title}</p><small>${n.time}</small></div>
      ${!n.read ? '<span class="notif-dot"></span>' : ""}
    </div>
  `).join("");
}

function readNotif(i) {
    notifications[i].read = true;
    renderNotifs();
    updateNotifBadge();
}

function markAllRead() {
    notifications.forEach(n => n.read = true);
    renderNotifs();
    updateNotifBadge();
    showAdminToast("Đã đánh dấu tất cả là đã đọc.");
}

function updateNotifBadge() {
    const unread = notifications.filter(n => !n.read).length;
    let badge = document.querySelector(".notif-badge");
    const btn = document.querySelector(".top-icon");
    if (!btn) return;
    if (unread > 0) {
        if (!badge) {
            badge = document.createElement("span");
            badge.className = "notif-badge";
            btn.style.position = "relative";
            btn.appendChild(badge);
        }
        badge.textContent = unread;
    } else {
        badge?.remove();
    }
}

function toggleNotifPanel() {
    buildNotifPanel();
    renderNotifs();
    const panel = $("notifPanel");
    panel.classList.toggle("show");
}

/* ───────── EXPORT REPORT ───────── */

function exportReport() {
    const revenue = orders.reduce((s, o) => s + o.total, 0);
    const rows = [
        ["Mã đơn", "Khách hàng", "Sản phẩm", "Tổng tiền", "Thanh toán", "Trạng thái"],
        ...orders.map(o => [o.id, o.customer, o.product, o.total + "đ", o.payment, o.status])
    ];
    const csv = rows.map(r => r.join(",")).join("\n");
    const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "bao-cao-doanh-thu.csv";
    a.click();
    URL.revokeObjectURL(url);
    showAdminToast("Đã xuất báo cáo CSV thành công.");
}

/* ───────── HERO SLIDER ───────── */

function showSlide(index) {
    const slides = document.querySelectorAll(".hero-slide");
    const dots = document.querySelectorAll(".hero-dots button");
    if (!slides.length) return;
    if (index < 0) index = slides.length - 1;
    if (index >= slides.length) index = 0;
    slides[currentSlide]?.classList.remove("active");
    dots[currentSlide]?.classList.remove("active");
    currentSlide = index;
    slides[currentSlide].classList.add("active");
    dots[currentSlide]?.classList.add("active");
}

function startSlider() {
    clearInterval(sliderTimer);
    sliderTimer = setInterval(() => showSlide(currentSlide + 1), 5000);
}

function initHeroSlider() {
    const slides = document.querySelectorAll(".hero-slide");
    const dotsWrap = $("heroDots");
    if (!slides.length || !dotsWrap) return;
    dotsWrap.innerHTML = "";
    slides.forEach((_, i) => {
        const dot = document.createElement("button");
        dot.type = "button";
        dot.className = i === 0 ? "active" : "";
        dot.addEventListener("click", () => { showSlide(i); startSlider(); });
        dotsWrap.appendChild(dot);
    });
    $("nextSlide")?.addEventListener("click", () => { showSlide(currentSlide + 1); startSlider(); });
    $("prevSlide")?.addEventListener("click", () => { showSlide(currentSlide - 1); startSlider(); });
    showSlide(0);
    startSlider();
}

/* ───────── ADMIN USER MENU ───────── */

function openLogoutModal() { $("logoutModal")?.classList.add("show"); }
function closeLogoutModal() { $("logoutModal")?.classList.remove("show"); }

function logoutAdmin() {
    localStorage.removeItem("sportix_user");
    localStorage.removeItem("admin_user");
    sessionStorage.removeItem("redirectAfterLogin");
    sessionStorage.removeItem("nextPage");
    closeLogoutModal();
    showAdminToast("Đăng xuất thành công.");
    setTimeout(() => { window.location.href = "login.html"; }, 900);
}

function initAdminUserMenu() {
    const wrap = $("adminUserWrap");
    const btn = $("adminUserBtn");
    const logoutBtn = $("adminLogoutBtn");
    const cancelBtn = $("cancelLogoutBtn");
    const confirmBtn = $("confirmLogoutBtn");
    const modal = $("logoutModal");
    const nameEl = $("adminName");
    const avatar = $("adminAvatar");
    if (!wrap || !btn) return;

    const name = getAdminName();
    nameEl.textContent = name;
    avatar.textContent = name.trim().charAt(0).toUpperCase() || "A";

    btn.addEventListener("click", e => { e.stopPropagation(); wrap.classList.toggle("open"); });
    logoutBtn?.addEventListener("click", e => { e.stopPropagation(); wrap.classList.remove("open"); openLogoutModal(); });
    cancelBtn?.addEventListener("click", closeLogoutModal);
    confirmBtn?.addEventListener("click", logoutAdmin);
    modal?.addEventListener("click", e => { if (e.target === modal) closeLogoutModal(); });
    document.addEventListener("click", () => wrap.classList.remove("open"));
    document.addEventListener("keydown", e => {
        if (e.key === "Escape") { wrap.classList.remove("open"); closeLogoutModal(); $("notifPanel")?.classList.remove("show"); }
    });
}

/* ───────── SIDEBAR ───────── */

function initSidebar() {
    $("btnMenu")?.addEventListener("click", () => $("sidebar")?.classList.toggle("show"));
    document.querySelectorAll(".admin-link").forEach(link => {
        link.addEventListener("click", () => {
            document.querySelectorAll(".admin-link").forEach(l => l.classList.remove("active"));
            link.classList.add("active");
            $("sidebar")?.classList.remove("show");
        });
    });
}

/* ───────── CATEGORY SELECT ───────── */

function initCategorySelect() {
    const select = $("categoryFilter");
    if (!select) return;
    const btn = select.querySelector(".admin-custom-select-btn");
    const text = btn?.querySelector("span");
    const options = select.querySelectorAll(".admin-custom-option");

    btn?.addEventListener("click", e => { e.stopPropagation(); select.classList.toggle("open"); });
    options.forEach(option => {
        option.addEventListener("click", e => {
            e.stopPropagation();
            selectedCategory = option.dataset.value || "all";
            select.dataset.value = selectedCategory;
            if (text) text.textContent = option.textContent.trim();
            options.forEach(o => o.classList.remove("active"));
            option.classList.add("active");
            select.classList.remove("open");
            renderProducts();
        });
    });
    document.addEventListener("click", e => { if (!select.contains(e.target)) select.classList.remove("open"); });
}


/* ───────── FILTERS ───────── */

function initFilters() {
    const searchInput = $("searchInput");

    if (searchInput) {
        searchInput.addEventListener("input", (e) => {
            // 1. Gọi hàm lọc và hiển thị sản phẩm như cũ
            renderProducts();

            // 2. Tự động cuộn đến khu vực bảng sản phẩm (ID 'sanpham' hoặc 'productTable')
            const targetSection = document.getElementById("sanpham") || document.getElementById("productTable");

            // Chỉ cuộn nếu người dùng thực sự nhập chữ (tránh cuộn khi xoá trắng ô tìm kiếm)
            if (targetSection && e.target.value.trim() !== "") {
                targetSection.scrollIntoView({
                    behavior: "smooth", // Cuộn mượt mà
                    block: "start"      // Căn phần tử lên đầu màn hình
                });
            }
        });
    }

    initCategorySelect();
}

/* ───────── BUTTON WIRING ───────── */

function initButtonWiring() {
    /* Bell notification */
    document.querySelector(".top-icon")?.addEventListener("click", e => {
        e.stopPropagation();
        toggleNotifPanel();
    });

    /* Xuất báo cáo button inside revenue panel */
    document.querySelector(".revenue-panel .admin-soft-btn")?.addEventListener("click", exportReport);

    /* Xem tất cả orders */
    document.querySelector("#donhang .admin-soft-btn")?.addEventListener("click", toggleAllOrders);

    /* Xem tất cả customers */
    document.querySelector("#khachhang .admin-soft-btn")?.addEventListener("click", toggleAllCustomers);

    /* Thêm sản phẩm button in hero */
    document.querySelector(".admin-primary-btn[href='#sanpham']")?.addEventListener("click", e => {
        setTimeout(() => openProductModal(), 400);
    });
}

/* ───────── INIT ───────── */

document.addEventListener("DOMContentLoaded", () => {
    renderStats();
    renderSalesChart();
    renderBestProducts();
    renderLowStock();
    renderQuickInfo();
    renderProducts();
    renderOrders();
    renderCustomers();
    renderSettings();

    initHeroSlider();
    initAdminUserMenu();
    initSidebar();
    initFilters();
    initButtonWiring();
    updateNotifBadge();
});