// ==================== TRẠNG THÁI ====================
let sanPhamHienTai = null;
let sizeDaChon = null;
let mauDaChon = 'Đen';
let gioHang = JSON.parse(localStorage.getItem('sz_cart') || '[]');
let daThich = false;

// ==================== KHỞI TẠO ====================
function layIdTuURL() {
    try {
        const params = new URLSearchParams(window.location.search);
        const id = parseInt(params.get('id'));
        if (id) return id;
    } catch (e) { }

    // Fallback: tách thủ công (hoạt động khi mở file:// trên máy)
    const match = window.location.href.match(/[?&]id=(\d+)/);
    if (match) return parseInt(match[1]);

    return 1; // mặc định sản phẩm đầu tiên
}

function khoiTao() {
    const id = layIdTuURL();
    sanPhamHienTai = PRODUCTS.find(p => p.id === id) || PRODUCTS[0];
    hienThiChiTiet();
    hienThiSanPhamLienQuan();
    capNhatSoBadgeGio();
}

function dinhDangGia(n) {
    return n.toLocaleString('vi-VN') + 'đ';
}

// ==================== HIỂN THỊ CHI TIẾT ====================
function hienThiChiTiet() {
    const p = sanPhamHienTai;
    document.title = `${p.name} | Hawk Shop`;
    document.getElementById('breadProduct').textContent = p.name;
    document.getElementById('infoBrand').textContent = p.brand;
    document.getElementById('infoName').textContent = p.name;
    document.getElementById('infoPrice').textContent = dinhDangGia(p.price);

    // Giá gốc & phần trăm giảm
    if (p.oldPrice) {
        document.getElementById('infoOldPrice').textContent = dinhDangGia(p.oldPrice);
        const phanTramGiam = Math.round((1 - p.price / p.oldPrice) * 100);
        const tagGiam = document.getElementById('infoDiscount');
        tagGiam.style.display = 'inline';
        tagGiam.textContent = `-${phanTramGiam}%`;
    }

    // Badge mới / sale
    const badge = document.getElementById('mainBadge');
    if (p.badge === 'new') { badge.textContent = 'Mới'; badge.className = 'badge-detail badge-new'; }
    else if (p.badge === 'sale') { badge.textContent = 'Sale'; badge.className = 'badge-detail badge-sale'; }
    else badge.style.display = 'none';

    // Ảnh chính & thumbnails
    const anhChinh = document.getElementById('mainImg');
    anhChinh.src = p.imgs[0];
    anhChinh.alt = p.name;
    document.getElementById('thumbList').innerHTML = p.imgs.map((img, i) => `
    <div class="thumb ${i === 0 ? 'active' : ''}" onclick="doiAnhChinh('${img}', this)">
      <img src="${img}" alt=""/>
    </div>
  `).join('');

    // Nút chọn size
    document.getElementById('sizeButtons').innerHTML = p.sizes.map(s => `
    <button class="size-btn ${p.unavailable.includes(s) ? 'unavailable' : ''}"
      onclick="${p.unavailable.includes(s) ? '' : `chonSize(this,'${s}')`}">${s}</button>
  `).join('');

    // Mô tả
    document.getElementById('tabDescription').textContent = p.desc;

    // Thông số kỹ thuật
    document.getElementById('specTable').innerHTML = p.specs.map(([k, v]) =>
        `<tr><td>${k}</td><td>${v}</td></tr>`
    ).join('');

    // Đánh giá (dùng DANH_GIA từ product-data.js)
    document.getElementById('reviewList').innerHTML = DANH_GIA.map(r => `
    <div style="background:var(--dark3);border-radius:8px;padding:1rem;">
      <div style="display:flex;justify-content:space-between;margin-bottom:0.4rem">
        <strong style="color:var(--text)">${r.user}</strong>
        <span style="color:var(--muted);font-size:0.8rem">${r.date}</span>
      </div>
      <div style="color:var(--primary);margin-bottom:0.4rem">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</div>
      <p style="color:var(--muted);font-size:0.9rem;margin:0">${r.text}</p>
    </div>
  `).join('');
}

// ==================== SẢN PHẨM LIÊN QUAN ====================
function hienThiSanPhamLienQuan() {
    const p = sanPhamHienTai;
    const lienQuan = PRODUCTS.filter(x => x.id !== p.id && x.category === p.category).slice(0, 4);
    const duPhong = PRODUCTS.filter(x => x.id !== p.id).slice(0, 4);
    const danhSach = lienQuan.length >= 2 ? lienQuan : duPhong;
    document.getElementById('relatedGrid').innerHTML = danhSach.map(r => `
    <div class="col-6 col-md-3">
      <a class="related-card" href="product-detail.html?id=${r.id}">
        <img src="${r.imgs[0]}" alt="${r.name}"/>
        <div class="related-card-body">
          <div class="related-card-name">${r.name}</div>
          <div class="related-card-price">${dinhDangGia(r.price)}</div>
        </div>
      </a>
    </div>
  `).join('');
}

// ==================== THAO TÁC GALLERY ====================
function doiAnhChinh(src, el) {
    document.getElementById('mainImg').src = src;
    document.querySelectorAll('.thumb').forEach(t => t.classList.remove('active'));
    el.classList.add('active');
}

// ==================== CHỌN SIZE ====================
function chonSize(btn, size) {
    document.querySelectorAll('.size-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    sizeDaChon = size;
    document.getElementById('sizeLabel').textContent = size;
}

// ==================== CHỌN MÀU ====================
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.color-swatch').forEach(sw => {
        sw.addEventListener('click', function () {
            document.querySelectorAll('.color-swatch').forEach(s => s.classList.remove('active'));
            this.classList.add('active');
            mauDaChon = this.dataset.color;
            document.getElementById('colorLabel').textContent = mauDaChon;
        });
    });
});

// ==================== SỐ LƯỢNG ====================
function thayDoiSoLuong(buoc) {
    const input = document.getElementById('qtyInput');
    let soLuong = parseInt(input.value) + buoc;
    if (soLuong < 1) soLuong = 1;
    if (soLuong > 99) soLuong = 99;
    input.value = soLuong;
}

// ==================== GIỎ HÀNG ====================
function themVaoGio() {
    if (!sizeDaChon) { hienThongBao('Vui lòng chọn kích thước!'); return; }
    const p = sanPhamHienTai;
    const soLuong = parseInt(document.getElementById('qtyInput').value);
    const khoa = `${p.id}-${sizeDaChon}-${mauDaChon}`;
    const daCoTrongGio = gioHang.find(x => x.key === khoa);
    if (daCoTrongGio) daCoTrongGio.qty += soLuong;
    else gioHang.push({ key: khoa, id: p.id, name: p.name, price: p.price, qty: soLuong, size: sizeDaChon, color: mauDaChon });
    localStorage.setItem('sz_cart', JSON.stringify(gioHang));
    capNhatSoBadgeGio();
    hienThongBao(`Đã thêm <strong>${p.name}</strong> (${sizeDaChon}) vào giỏ!`);
}

// ==================== YÊU THÍCH ====================
function batTatYeuThich() {
    daThich = !daThich;
    const btn = document.getElementById('wishBtn');
    btn.classList.toggle('liked', daThich);
    btn.innerHTML = daThich ? '<i class="bi bi-heart-fill"></i>' : '<i class="bi bi-heart"></i>';
    hienThongBao(daThich ? 'Đã thêm vào yêu thích!' : 'Đã xóa khỏi yêu thích.');
}

// ==================== TAB ====================
function chuyenTab(id, btn) {
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.getElementById('tab-' + id).classList.add('active');
    btn.classList.add('active');
}

// ==================== THÔNG BÁO ====================
function hienThongBao(msg) {
    const wrap = document.getElementById('toastWrap');
    const el = document.createElement('div');
    el.className = 'my-toast';
    el.innerHTML = msg;
    wrap.appendChild(el);
    setTimeout(() => el.remove(), 3000);
}

function capNhatSoBadgeGio() {
    document.getElementById('cartCount').textContent = gioHang.reduce((s, x) => s + x.qty, 0);
}

// ==================== KHỞI CHẠY ====================
khoiTao();