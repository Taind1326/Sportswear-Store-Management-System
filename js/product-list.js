// ==================== TRẠNG THÁI ====================
let boLoc = { category: 'all', brand: 'all', price: 5000000 };
let tuKhoaTimKiem = '';
let sapXepTheo = 'default';
let trangHienTai = 1;
const SO_SAN_PHAM_MOI_TRANG = 9;
let gioHang = JSON.parse(localStorage.getItem('sz_cart') || '[]');

// ==================== TIỆN ÍCH ====================
function dinhDangGia(n) {
    return n.toLocaleString('vi-VN') + 'đ';
}
function nhanDanhMuc(c) {
    return { giay: 'Giày thể thao', ao: 'Áo quần', gym: 'Dụng cụ gym', bongda: 'Bóng đá', phukien: 'Phụ kiện' }[c] || c;
}
function phanTramGiam(oldPrice, price) {
    return Math.round((1 - price / oldPrice) * 100);
}

// ==================== CARD HTML ====================
function taoCardHTML(p, showSaveBadge = false) {
    const savePct = p.oldPrice ? phanTramGiam(p.oldPrice, p.price) : 0;
    return `
    <div class="product-card" onclick="window.location='product-detail.html?id=${p.id}'">
      <div class="product-img-wrap">
        <img src="${p.img}" alt="${p.name}" loading="lazy"/>
        ${p.badge === 'new' ? '<span class="badge-new">Mới</span>' : ''}
        ${p.badge === 'sale' ? '<span class="badge-sale">Sale</span>' : ''}
        ${showSaveBadge && p.oldPrice ? `<span class="badge-sale-pct">-${savePct}%</span>` : ''}
        <button class="quick-add" onclick="event.stopPropagation(); themVaoGio(${p.id})">
          <i class="bi bi-bag-plus"></i> Thêm giỏ hàng
        </button>
      </div>
      <div class="product-body">
        <div class="product-category">${nhanDanhMuc(p.category)}</div>
        <div class="product-name">${p.name}</div>
        <div class="product-price">
          <span class="price-current">${dinhDangGia(p.price)}</span>
          ${p.oldPrice ? `<span class="price-old">${dinhDangGia(p.oldPrice)}</span>` : ''}
          ${showSaveBadge && p.oldPrice ? `<span class="price-save">-${savePct}%</span>` : ''}
        </div>
        <button class="btn-add-card w-100" onclick="event.stopPropagation(); themVaoGio(${p.id})">
          <i class="bi bi-bag-plus"></i> Thêm vào giỏ
        </button>
      </div>
    </div>`;
}

// ==================== NEW ARRIVALS STRIP ====================
let stripOffset = 0;
const CARD_WIDTH = 224; // 210px card + 14px gap

function hienThiNewStrip() {
    const newProducts = PRODUCTS.filter(p => p.badge === 'new');
    const strip = document.getElementById('newStrip');
    strip.innerHTML = newProducts.map(p => `
      <div>${taoCardHTML(p, false)}</div>
    `).join('');
    stripOffset = 0;
    strip.style.transform = 'translateX(0)';
}

function capNhatStripNav() {
    const newProducts = PRODUCTS.filter(p => p.badge === 'new');
    const wrapper = document.querySelector('.new-strip-wrapper');
    const visible = Math.floor(wrapper.offsetWidth / CARD_WIDTH);
    const maxOffset = Math.max(0, newProducts.length - visible);

    document.getElementById('stripPrev').disabled = stripOffset <= 0;
    document.getElementById('stripNext').disabled = stripOffset >= maxOffset;
}

document.getElementById('stripNext').addEventListener('click', () => {
    const newProducts = PRODUCTS.filter(p => p.badge === 'new');
    const wrapper = document.querySelector('.new-strip-wrapper');
    const visible = Math.floor(wrapper.offsetWidth / CARD_WIDTH);
    const maxOffset = newProducts.length - visible;
    if (stripOffset < maxOffset) {
        stripOffset++;
        document.getElementById('newStrip').style.transform = `translateX(-${stripOffset * CARD_WIDTH}px)`;
        capNhatStripNav();
    }
});

document.getElementById('stripPrev').addEventListener('click', () => {
    if (stripOffset > 0) {
        stripOffset--;
        document.getElementById('newStrip').style.transform = `translateX(-${stripOffset * CARD_WIDTH}px)`;
        capNhatStripNav();
    }
});

// ==================== LỌC & SẮP XẾP ====================
function layDanhSachDaLoc() {
    let ketQua = [...PRODUCTS];
    if (boLoc.category !== 'all') ketQua = ketQua.filter(p => p.category === boLoc.category);
    if (boLoc.brand !== 'all') ketQua = ketQua.filter(p => p.brand === boLoc.brand);
    ketQua = ketQua.filter(p => p.price <= boLoc.price);
    if (tuKhoaTimKiem) {
        const q = tuKhoaTimKiem.toLowerCase();
        ketQua = ketQua.filter(p => p.name.toLowerCase().includes(q) || p.brand.toLowerCase().includes(q));
    }
    if (sapXepTheo === 'price-asc') ketQua.sort((a, b) => a.price - b.price);
    if (sapXepTheo === 'price-desc') ketQua.sort((a, b) => b.price - a.price);
    if (sapXepTheo === 'name-asc') ketQua.sort((a, b) => a.name.localeCompare(b.name));
    return ketQua;
}

function applyFilters() {
    sapXepTheo = document.getElementById('sortSelect').value;
    trangHienTai = 1;
    hienThiSanPham();
}

function resetFilters() {
    boLoc = { category: 'all', brand: 'all', price: 5000000 };
    tuKhoaTimKiem = '';
    document.getElementById('searchInput').value = '';
    document.getElementById('priceRange').value = 5000000;
    document.getElementById('priceLabel').textContent = '5.000.000đ';
    document.getElementById('sortSelect').value = 'default';
    sapXepTheo = 'default';
    // Reset chips
    document.querySelectorAll('.chip').forEach(b => b.classList.toggle('active', b.dataset.value === 'all'));
    document.querySelectorAll('.brand-btn').forEach(b => b.classList.toggle('active', b.dataset.value === 'all'));
    trangHienTai = 1;
    hienThiSanPham();
}

// ==================== HIỂN THỊ CHÍNH ====================
function hienThiSanPham() {
    const tatCa = layDanhSachDaLoc();

    // Tách sale và non-sale
    const danhSachSale = tatCa.filter(p => p.badge === 'sale');

    // Tổng hiển thị
    document.getElementById('totalCount').textContent = tatCa.length;

    // ── Sale section ──
    const saleWrap = document.getElementById('saleSectionWrap');
    const saleGrid = document.getElementById('saleGrid');
    if (danhSachSale.length > 0) {
        saleWrap.style.display = 'block';
        saleGrid.innerHTML = danhSachSale.map(p => `
          <div class="col-sm-6 col-xl-4">${taoCardHTML(p, true)}</div>
        `).join('');
    } else {
        saleWrap.style.display = 'none';
    }

    // ── All section label ──
    const allLabel = document.getElementById('allSectionLabel');
    allLabel.style.display = tatCa.length > 0 ? 'flex' : 'none';

    // ── Pagination trên toàn bộ danh sách ──
    const tongSo = tatCa.length;
    const soTrang = Math.ceil(tongSo / SO_SAN_PHAM_MOI_TRANG);
    const sanPhamTrang = tatCa.slice((trangHienTai - 1) * SO_SAN_PHAM_MOI_TRANG, trangHienTai * SO_SAN_PHAM_MOI_TRANG);

    document.getElementById('showingCount').textContent = sanPhamTrang.length;

    const grid = document.getElementById('productGrid');
    const emptyState = document.getElementById('emptyState');

    if (tatCa.length === 0) {
        grid.innerHTML = '';
        emptyState.style.display = 'block';
    } else {
        emptyState.style.display = 'none';
        grid.innerHTML = sanPhamTrang.map(p => `
          <div class="col-sm-6 col-xl-4">${taoCardHTML(p, true)}</div>
        `).join('');
    }

    // Pagination
    const pag = document.getElementById('pagination');
    if (soTrang <= 1) { pag.innerHTML = ''; return; }
    let html = `<li class="page-item ${trangHienTai === 1 ? 'disabled' : ''}">
      <a class="page-link" href="#" onclick="chuyenTrang(${trangHienTai - 1})">‹</a></li>`;
    for (let i = 1; i <= soTrang; i++) {
        html += `<li class="page-item ${i === trangHienTai ? 'active' : ''}">
        <a class="page-link" href="#" onclick="chuyenTrang(${i})">${i}</a></li>`;
    }
    html += `<li class="page-item ${trangHienTai === soTrang ? 'disabled' : ''}">
      <a class="page-link" href="#" onclick="chuyenTrang(${trangHienTai + 1})">›</a></li>`;
    pag.innerHTML = html;
}

function chuyenTrang(n) {
    event.preventDefault();
    const soTrang = Math.ceil(layDanhSachDaLoc().length / SO_SAN_PHAM_MOI_TRANG);
    if (n < 1 || n > soTrang) return;
    trangHienTai = n;
    hienThiSanPham();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ==================== GIỎ HÀNG ====================
function themVaoGio(id) {
    const sanPham = PRODUCTS.find(x => x.id === id);
    const daCoTrongGio = gioHang.find(x => x.id === id);
    if (daCoTrongGio) daCoTrongGio.qty++;
    else gioHang.push({ id, name: sanPham.name, price: sanPham.price, qty: 1 });
    localStorage.setItem('sz_cart', JSON.stringify(gioHang));
    document.getElementById('cartCount').textContent = gioHang.reduce((s, x) => s + x.qty, 0);
    hienThongBao(`<i class="bi bi-check-circle-fill"></i> Đã thêm <strong>${sanPham.name}</strong>`);
}

function hienThongBao(msg) {
    const wrap = document.getElementById('toastWrap');
    const el = document.createElement('div');
    el.className = 'my-toast';
    el.innerHTML = msg;
    wrap.appendChild(el);
    setTimeout(() => el.remove(), 3000);
}

// ==================== SỰ KIỆN ====================
// Chips danh mục
document.querySelectorAll('.chip[data-filter]').forEach(btn => {
    btn.addEventListener('click', function () {
        document.querySelectorAll(`.chip[data-filter="${this.dataset.filter}"]`).forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        boLoc[this.dataset.filter] = this.dataset.value;
        trangHienTai = 1;
        hienThiSanPham();
    });
});

// Brand buttons
document.querySelectorAll('.brand-btn[data-filter]').forEach(btn => {
    btn.addEventListener('click', function () {
        document.querySelectorAll('.brand-btn[data-filter]').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        boLoc[this.dataset.filter] = this.dataset.value;
        trangHienTai = 1;
        hienThiSanPham();
    });
});

document.getElementById('searchInput').addEventListener('input', function () {
    tuKhoaTimKiem = this.value.trim();
    trangHienTai = 1;
    hienThiSanPham();
});

document.getElementById('priceRange').addEventListener('input', function () {
    boLoc.price = parseInt(this.value);
    document.getElementById('priceLabel').textContent = dinhDangGia(boLoc.price);
    trangHienTai = 1;
    hienThiSanPham();
});

// ==================== KHỞI CHẠY ====================
document.getElementById('cartCount').textContent = gioHang.reduce((s, x) => s + x.qty, 0);
hienThiNewStrip();
capNhatStripNav();
hienThiSanPham();