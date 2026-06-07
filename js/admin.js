// =====================
// HELPERS
// =====================
const $ = (id) => document.getElementById(id);

function formatMoney(value) {
  return value.toLocaleString("vi-VN") + "đ";
}

// =====================
// DATA
// =====================
const products = [
  { id: "SP001", name: "Giày chạy bộ Nike Air Zoom", category: "Giày", price: 2490000, stock: 12, sold: 128, image: "../img/shoe-1.jpg" },
  { id: "SP002", name: "Áo thun Adidas Training", category: "Áo", price: 690000, stock: 54, sold: 96, image: "../img/shirt-1.jpg" },
  { id: "SP003", name: "Balo thể thao SPORTIX", category: "Phụ kiện", price: 450000, stock: 8, sold: 74, image: "../img/bag-1.jpg" },
  { id: "SP004", name: "Bình nước tập gym Active", category: "Phụ kiện", price: 150000, stock: 5, sold: 61, image: "../img/bottle-1.jpg" },
  { id: "SP005", name: "Áo khoác thể thao Runner", category: "Áo", price: 890000, stock: 18, sold: 52, image: "../img/jacket-1.jpg" },
  { id: "SP006", name: "Giày bóng rổ Pro Court", category: "Giày", price: 1890000, stock: 22, sold: 88, image: "../img/shoe-2.jpg" }
];

const orders = [
  { id: "#SX1024", customer: "Nguyễn Hải Yến", product: "Nike Air Zoom", total: 2490000, payment: "Momo", status: "Đang giao" },
  { id: "#SX1023", customer: "Trần Minh Anh", product: "Adidas Training", total: 690000, payment: "COD", status: "Chờ xử lý" },
  { id: "#SX1022", customer: "Lê Hoàng Nam", product: "Balo SPORTIX", total: 450000, payment: "Banking", status: "Đã giao" }
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

// =====================
// STATUS
// =====================
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

// =====================
// STATS
// =====================
function renderStats() {
  const statsBox = document.querySelector(".stats-grid");
  if (!statsBox) return;

  const revenue = orders.reduce((s, o) => s + o.total, 0);

  statsBox.innerHTML = `
    <div class="stat-card">
      <div class="stat-icon"><i class="bi bi-cash-stack"></i></div>
      <div>
        <p>Doanh thu</p>
        <h3>${(revenue / 1000000).toFixed(1)}M</h3>
        <span>+12.4%</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon"><i class="bi bi-bag-check-fill"></i></div>
      <div>
        <p>Đơn hàng</p>
        <h3>${orders.length}</h3>
        <span>Hôm nay</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon"><i class="bi bi-box-seam-fill"></i></div>
      <div>
        <p>Sản phẩm</p>
        <h3>${products.length}</h3>
        <span>Đang bán</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon"><i class="bi bi-exclamation-triangle-fill"></i></div>
      <div>
        <p>Cảnh báo</p>
        <h3>${products.filter(p => p.stock <= 12).length}</h3>
        <span class="danger">Tồn kho thấp</span>
      </div>
    </div>
  `;
}

// =====================
// CHART
// =====================
function renderSalesChart() {
  const chart = $("salesChart");
  if (!chart) return;

  const max = Math.max(...sales.map(i => i.value));

  chart.innerHTML = sales.map(item => `
    <div class="chart-bar"
         style="height:${(item.value / max) * 100}%">
      <strong>${item.value}M</strong>
      <span>${item.day}</span>
    </div>
  `).join("");
}

// =====================
// BEST SELLER
// =====================
function renderBestProducts() {
  const bestList = $("bestList");
  if (!bestList) return;

  const best = [...products]
    .sort((a, b) => b.sold - a.sold)
    .slice(0, 4);

  bestList.innerHTML = best.map(item => `
    <div class="best-item">
      <img src="${item.image}" class="product-thumb">
      <div class="item-info">
        <h4>${item.name}</h4>
        <p>Đã bán ${item.sold}</p>
      </div>
    </div>
  `).join("");
}

// =====================
// LOW STOCK
// =====================
function renderLowStock() {
  const lowStock = $("lowStockList");
  if (!lowStock) return;

  lowStock.innerHTML = products
    .filter(item => item.stock <= 12)
    .map(item => `
      <div class="low-stock-item">
        <img src="${item.image}" class="product-thumb">
        <div class="item-info">
          <h4>${item.name}</h4>
          <p>Còn ${item.stock} sản phẩm</p>
        </div>
      </div>
    `).join("");
}

// =====================
// PRODUCTS
// =====================
function renderProducts() {
  const table = $("productTable");
  if (!table) return;

  const keyword =
    $("searchInput")?.value.toLowerCase() || "";

  table.innerHTML = products
    .filter(item =>
      item.name.toLowerCase().includes(keyword)
    )
    .map(item => {
      const status = getProductStatus(item.stock);

      return `
      <tr>
        <td>
          <div class="product-cell">
            <img src="${item.image}" class="product-thumb">
            <div>
              <strong>${item.name}</strong>
              <span>${item.id}</span>
            </div>
          </div>
        </td>
        <td>${item.category}</td>
        <td>${formatMoney(item.price)}</td>
        <td>${item.stock}</td>
        <td>
          <span class="badge-status ${status.className}">
            ${status.text}
          </span>
        </td>
        <td>
          <button class="action-btn">
            <i class="bi bi-three-dots"></i>
          </button>
        </td>
      </tr>
      `;
    }).join("");
}

// =====================
// ORDERS
// =====================
function renderOrders() {
  const table = $("orderTable");
  if (!table) return;

  table.innerHTML = orders.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.customer}</td>
      <td>${item.product}</td>
      <td>${formatMoney(item.total)}</td>
      <td>${item.payment}</td>
      <td>
        <span class="badge-status ${getOrderStatusClass(item.status)}">
          ${item.status}
        </span>
      </td>
    </tr>
  `).join("");
}

// =====================
// HERO SLIDER
// =====================
// =====================
// HERO SLIDER
// =====================
let currentSlide = 0;
let sliderTimer;

function showSlide(index) {
  const slides = document.querySelectorAll(".hero-slide");
  const dots = document.querySelectorAll(".hero-dots button");

  if (!slides.length) return;

  if (index < 0) index = slides.length - 1;
  if (index >= slides.length) index = 0;

  slides.forEach(slide => slide.classList.remove("active"));
  dots.forEach(dot => dot.classList.remove("active"));

  slides[index].classList.add("active");
  dots[index].classList.add("active");

  currentSlide = index;
}

function nextSlide() {
  showSlide(currentSlide + 1);
}

function prevSlide() {
  showSlide(currentSlide - 1);
}

function startSlider() {
  sliderTimer = setInterval(nextSlide, 5000);
}

function resetSlider() {
  clearInterval(sliderTimer);
  startSlider();
}

function initHeroSlider() {
  const dots = document.querySelectorAll(".hero-dots button");
  const nextButton = $("nextSlide");
  const prevButton = $("prevSlide");

  dots.forEach(dot => {
    dot.addEventListener("click", () => {
      showSlide(Number(dot.dataset.index));
      resetSlider();
    });
  });

  nextButton?.addEventListener("click", () => {
    nextSlide();
    resetSlider();
  });

  prevButton?.addEventListener("click", () => {
    prevSlide();
    resetSlider();
  });

  showSlide(0);
  startSlider();
}

// =====================
// SCROLL REVEAL
// =====================
function reveal() {
  document.querySelectorAll(".reveal").forEach(item => {
    if (item.getBoundingClientRect().top < window.innerHeight - 100) {
      item.classList.add("show");
    }
  });
}

window.addEventListener("scroll", reveal);

// =====================
// INIT
// =====================
document.addEventListener("DOMContentLoaded", () => {
  renderStats();
  renderSalesChart();
  renderBestProducts();
  renderLowStock();
  renderProducts();
  renderOrders();


  $("searchInput")?.addEventListener("input", renderProducts);
  initHeroSlider();
  reveal();
});