const $ = (id) => document.getElementById(id);

let selectedCategory = "all";
let currentSlide = 0;
let sliderTimer = null;

const products = [
  { id:"SP001", name:"Giày chạy bộ Nike Air Zoom", category:"Giày", price:2490000, stock:12, sold:128, image:"../img/shoe-1.jpg" },
  { id:"SP002", name:"Áo thun Adidas Training", category:"Áo", price:690000, stock:54, sold:96, image:"../img/shirt-1.jpg" },
  { id:"SP003", name:"Balo thể thao SPORTIX", category:"Phụ kiện", price:450000, stock:8, sold:74, image:"../img/bag-1.jpg" },
  { id:"SP004", name:"Bình nước tập gym Active", category:"Phụ kiện", price:150000, stock:5, sold:61, image:"../img/bottle-1.jpg" },
  { id:"SP005", name:"Áo khoác thể thao Runner", category:"Áo", price:890000, stock:18, sold:52, image:"../img/jacket-1.jpg" },
  { id:"SP006", name:"Giày bóng rổ Pro Court", category:"Giày", price:1890000, stock:22, sold:88, image:"../img/shoe-2.jpg" }
];

const orders = [
  { id:"#SX1024", customer:"Nguyễn Hải Yến", product:"Nike Air Zoom", total:2490000, payment:"MoMo", status:"Đang giao" },
  { id:"#SX1023", customer:"Trần Minh Anh", product:"Adidas Training", total:690000, payment:"COD", status:"Chờ xử lý" },
  { id:"#SX1022", customer:"Lê Hoàng Nam", product:"Balo SPORTIX", total:450000, payment:"Banking", status:"Đã giao" }
];

const customers = [
  { id:"KH001", name:"Nguyễn Văn A", email:"vana@gmail.com", orders:12, spend:8500000 },
  { id:"KH002", name:"Trần Thị B", email:"thib@gmail.com", orders:7, spend:4200000 },
  { id:"KH003", name:"Lê Văn C", email:"vanc@gmail.com", orders:15, spend:11300000 }
];

const sales = [
  { day:"T2", value:12 },
  { day:"T3", value:18 },
  { day:"T4", value:10 },
  { day:"T5", value:24 },
  { day:"T6", value:20 },
  { day:"T7", value:16 },
  { day:"CN", value:14 }
];

const activities = [
  { icon:"bi-bag-check-fill", title:"Đơn hàng mới", text:"#SX1024 vừa được tạo thành công." },
  { icon:"bi-box-seam-fill", title:"Cập nhật sản phẩm", text:"Áo khoác Runner đã được cập nhật tồn kho." },
  { icon:"bi-person-check-fill", title:"Khách hàng mới", text:"Nguyễn Hải Yến vừa đăng ký tài khoản." }
];

const settings = [
  { icon:"bi-shop", title:"Thông tin cửa hàng", text:"Quản lý tên cửa hàng, logo và thông tin liên hệ." },
  { icon:"bi-shield-lock-fill", title:"Tài khoản quản trị", text:"Đổi mật khẩu và bảo mật tài khoản admin." },
  { icon:"bi-bell-fill", title:"Thông báo", text:"Quản lý email và thông báo hệ thống." }
];

function formatMoney(value){
  return value.toLocaleString("vi-VN") + "đ";
}

function getProductStatus(stock){
  if(stock <= 5) return { text:"Sắp hết", className:"cancel" };
  if(stock <= 12) return { text:"Cần nhập", className:"pending" };
  return { text:"Đang bán", className:"done" };
}

function getOrderStatusClass(status){
  if(status === "Đã giao") return "done";
  if(status === "Đang giao") return "shipping";
  if(status === "Chờ xử lý") return "pending";
  return "cancel";
}

function renderStats(){
  const statsBox = document.querySelector(".stats-grid");
  if(!statsBox) return;

  const revenue = orders.reduce((sum, item) => sum + item.total, 0);
  const lowStock = products.filter(item => item.stock <= 12).length;

  statsBox.innerHTML = `
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-cash-stack"></i></div><div><p>Doanh thu</p><h3>${(revenue / 1000000).toFixed(1)}M</h3><span>+12.4%</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-bag-check-fill"></i></div><div><p>Đơn hàng</p><h3>${orders.length}</h3><span>Hôm nay</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-box-seam-fill"></i></div><div><p>Sản phẩm</p><h3>${products.length}</h3><span>Đang bán</span></div></div>
    <div class="stat-card"><div class="stat-icon"><i class="bi bi-exclamation-triangle-fill"></i></div><div><p>Cảnh báo</p><h3>${lowStock}</h3><span class="danger">Tồn kho thấp</span></div></div>
  `;
}

function renderSalesChart(){
  const chart = $("salesChart");
  if(!chart) return;

  const max = Math.max(...sales.map(item => item.value));

  chart.innerHTML = sales.map((item, index) => `
    <div class="chart-bar ${index === 4 ? "today" : ""}" style="height:${(item.value / max) * 100}%">
      <strong>${item.value}M</strong>
      <span>${item.day}</span>
    </div>
  `).join("");
}

function renderBestProducts(){
  const bestList = $("bestList");
  if(!bestList) return;

  bestList.innerHTML = [...products]
    .sort((a, b) => b.sold - a.sold)
    .slice(0, 4)
    .map(item => `
      <div class="best-item">
        <img src="${item.image}" class="product-thumb" alt="${item.name}">
        <div class="item-info"><h4>${item.name}</h4><p>Đã bán ${item.sold}</p></div>
      </div>
    `).join("");
}

function renderLowStock(){
  const list = $("lowStockList");
  if(!list) return;

  list.innerHTML = products
    .filter(item => item.stock <= 12)
    .map(item => `
      <div class="low-stock-item">
        <img src="${item.image}" class="product-thumb" alt="${item.name}">
        <div class="item-info"><h4>${item.name}</h4><p>Còn ${item.stock} sản phẩm</p></div>
      </div>
    `).join("");
}

function renderQuickInfo(){
  const alertList = $("alertList");
  const activityList = $("activityList");

  if(alertList){
    alertList.innerHTML = products
      .filter(item => item.stock <= 12)
      .map(item => `
        <div class="quick-item">
          <div class="quick-icon"><i class="bi bi-exclamation-triangle-fill"></i></div>
          <div><h4>${item.name}</h4><p>Còn ${item.stock} sản phẩm, cần nhập thêm.</p></div>
        </div>
      `).join("");
  }

  if(activityList){
    activityList.innerHTML = activities.map(item => `
      <div class="quick-item">
        <div class="quick-icon"><i class="bi ${item.icon}"></i></div>
        <div><h4>${item.title}</h4><p>${item.text}</p></div>
      </div>
    `).join("");
  }
}

function renderProducts(){
  const table = $("productTable");
  if(!table) return;

  const keyword = ($("searchInput")?.value || "").toLowerCase().trim();

  const result = products
    .filter(item =>
      item.name.toLowerCase().includes(keyword) ||
      item.id.toLowerCase().includes(keyword) ||
      item.category.toLowerCase().includes(keyword)
    )
    .filter(item => selectedCategory === "all" || item.category === selectedCategory);

  table.innerHTML = result.map(item => {
    const status = getProductStatus(item.stock);

    return `
      <tr>
        <td><div class="product-cell"><img src="${item.image}" class="product-thumb" alt="${item.name}"><div><strong>${item.name}</strong><span>${item.id}</span></div></div></td>
        <td>${item.category}</td>
        <td>${formatMoney(item.price)}</td>
        <td>${item.stock}</td>
        <td><span class="badge-status ${status.className}">${status.text}</span></td>
        <td><button class="action-btn" type="button"><i class="bi bi-three-dots"></i></button></td>
      </tr>
    `;
  }).join("");
}

function renderOrders(){
  const table = $("orderTable");
  if(!table) return;

  table.innerHTML = orders.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.customer}</td>
      <td>${item.product}</td>
      <td>${formatMoney(item.total)}</td>
      <td>${item.payment}</td>
      <td><span class="badge-status ${getOrderStatusClass(item.status)}">${item.status}</span></td>
    </tr>
  `).join("");
}

function renderCustomers(){
  const table = $("customerTable");
  if(!table) return;

  table.innerHTML = customers.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.name}</td>
      <td>${item.email}</td>
      <td>${item.orders}</td>
      <td>${formatMoney(item.spend)}</td>
    </tr>
  `).join("");
}

function renderSettings(){
  const grid = $("settingGrid");
  if(!grid) return;

  grid.innerHTML = settings.map(item => `
    <div class="col-md-4">
      <div class="setting-card">
        <i class="bi ${item.icon}"></i>
        <h5>${item.title}</h5>
        <p>${item.text}</p>
      </div>
    </div>
  `).join("");
}

function showSlide(index){
  const slides = document.querySelectorAll(".hero-slide");
  const dots = document.querySelectorAll(".hero-dots button");
  if(!slides.length) return;

  if(index < 0) index = slides.length - 1;
  if(index >= slides.length) index = 0;

  slides[currentSlide]?.classList.remove("active");
  dots[currentSlide]?.classList.remove("active");

  currentSlide = index;

  slides[currentSlide].classList.add("active");
  dots[currentSlide]?.classList.add("active");
}

function startSlider(){
  clearInterval(sliderTimer);
  sliderTimer = setInterval(() => showSlide(currentSlide + 1), 5000);
}

function initHeroSlider(){
  const slides = document.querySelectorAll(".hero-slide");
  const dotsWrap = $("heroDots");
  if(!slides.length || !dotsWrap) return;

  dotsWrap.innerHTML = "";

  slides.forEach((_, index) => {
    const dot = document.createElement("button");
    dot.type = "button";
    dot.className = index === 0 ? "active" : "";
    dot.addEventListener("click", () => {
      showSlide(index);
      startSlider();
    });
    dotsWrap.appendChild(dot);
  });

  $("nextSlide")?.addEventListener("click", () => {
    showSlide(currentSlide + 1);
    startSlider();
  });

  $("prevSlide")?.addEventListener("click", () => {
    showSlide(currentSlide - 1);
    startSlider();
  });

  showSlide(0);
  startSlider();
}

function getAdminName(){
  const user = localStorage.getItem("sportix_user") || localStorage.getItem("admin_user");
  if(!user) return "Admin";

  try{
    const data = JSON.parse(user);
    return data.name || data.email || "Admin";
  }catch{
    return "Admin";
  }
}

function showAdminToast(message){
  const toast = $("adminToast");
  if(!toast) return;

  toast.querySelector("span").textContent = message;
  toast.classList.add("show");

  clearTimeout(showAdminToast.timer);
  showAdminToast.timer = setTimeout(() => toast.classList.remove("show"), 1800);
}

function openLogoutModal(){
  $("logoutModal")?.classList.add("show");
}

function closeLogoutModal(){
  $("logoutModal")?.classList.remove("show");
}

function logoutAdmin(){
  localStorage.removeItem("sportix_user");
  localStorage.removeItem("admin_user");
  sessionStorage.removeItem("redirectAfterLogin");
  sessionStorage.removeItem("nextPage");

  closeLogoutModal();
  showAdminToast("Đăng xuất thành công.");

  setTimeout(() => {
    window.location.href = "login.html";
  }, 900);
}

function initAdminUserMenu(){
  const wrap = $("adminUserWrap");
  const btn = $("adminUserBtn");
  const logoutBtn = $("adminLogoutBtn");
  const cancelBtn = $("cancelLogoutBtn");
  const confirmBtn = $("confirmLogoutBtn");
  const modal = $("logoutModal");
  const nameEl = $("adminName");
  const avatar = $("adminAvatar");
  if(!wrap || !btn) return;

  const name = getAdminName();
  nameEl.textContent = name;
  avatar.textContent = name.trim().charAt(0).toUpperCase() || "A";

  btn.addEventListener("click", event => {
    event.stopPropagation();
    wrap.classList.toggle("open");
  });

  logoutBtn?.addEventListener("click", event => {
    event.stopPropagation();
    wrap.classList.remove("open");
    openLogoutModal();
  });

  cancelBtn?.addEventListener("click", closeLogoutModal);
  confirmBtn?.addEventListener("click", logoutAdmin);

  modal?.addEventListener("click", event => {
    if(event.target === modal) closeLogoutModal();
  });

  document.addEventListener("click", () => wrap.classList.remove("open"));

  document.addEventListener("keydown", event => {
    if(event.key === "Escape"){
      wrap.classList.remove("open");
      closeLogoutModal();
    }
  });
}

function initSidebar(){
  $("btnMenu")?.addEventListener("click", () => {
    $("sidebar")?.classList.toggle("show");
  });

  document.querySelectorAll(".admin-link").forEach(link => {
    link.addEventListener("click", () => {
      document.querySelectorAll(".admin-link").forEach(item => item.classList.remove("active"));
      link.classList.add("active");
      $("sidebar")?.classList.remove("show");
    });
  });
}

function initCategorySelect(){
  const select = $("categoryFilter");
  if(!select) return;

  const btn = select.querySelector(".admin-custom-select-btn");
  const text = btn?.querySelector("span");
  const options = select.querySelectorAll(".admin-custom-option");

  btn?.addEventListener("click", event => {
    event.stopPropagation();
    select.classList.toggle("open");
  });

  options.forEach(option => {
    option.addEventListener("click", event => {
      event.stopPropagation();
      selectedCategory = option.dataset.value || "all";
      select.dataset.value = selectedCategory;

      if(text) text.textContent = option.textContent.trim();

      options.forEach(item => item.classList.remove("active"));
      option.classList.add("active");
      select.classList.remove("open");
      renderProducts();
    });
  });

  document.addEventListener("click", event => {
    if(!select.contains(event.target)) select.classList.remove("open");
  });
}

function initFilters(){
  $("searchInput")?.addEventListener("input", renderProducts);
  initCategorySelect();
}

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
});
