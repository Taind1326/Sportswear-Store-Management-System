let filters = {
  category: "all",
  brand: "all",
  price: 5000000
};

let searchText = "";
let sortType = "default";
let currentPage = 1;

const PRODUCTS_PER_PAGE = 9;
const STRIP_CARD_WIDTH = 258;
const CART_KEY = "sportix_cart";

let cart = JSON.parse(localStorage.getItem(CART_KEY) || "[]");

const productGrid = document.getElementById("productGrid");
const saleGrid = document.getElementById("saleGrid");
const saleWrap = document.getElementById("saleSectionWrap");
const emptyState = document.getElementById("emptyState");
const pagination = document.getElementById("pagination");
const newStrip = document.getElementById("newStrip");
const cartCount = document.getElementById("cartCount");

function formatMoney(value) {
  return value.toLocaleString("vi-VN") + "đ";
}

function getCategoryName(category) {
  const names = {
    giay: "Giày thể thao",
    ao: "Áo quần",
    gym: "Dụng cụ gym",
    bongda: "Bóng đá",
    phukien: "Phụ kiện"
  };

  return names[category] || category;
}

function getSalePercent(oldPrice, price) {
  if (!oldPrice) return 0;
  return Math.round((1 - price / oldPrice) * 100);
}

function getCartCount() {
  return cart.reduce((sum, item) => sum + item.qty, 0);
}

function updateCartCount() {
  if (!cartCount) return;
  cartCount.textContent = getCartCount();
}

function showToast(message) {
  const toast = document.getElementById("productToast");
  if (!toast) return;

  toast.querySelector("span").textContent = message;
  toast.classList.add("show");

  clearTimeout(showToast.timer);

  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2200);
}

function addToCart(id) {
  sessionStorage.setItem("sportix_need_option", "true");
  goWithSplash(`product-detail.html?id=${id}`);
}

function goDetail(id) {
  goWithSplash(`product-detail.html?id=${id}`);
}

function createProductCard(product, showSalePercent = true) {
  const salePercent = getSalePercent(product.oldPrice, product.price);

  return `
    <div class="product-card">
      <div class="product-img-wrap" onclick="goDetail('${product.id}')">
        <img src="${product.img}" alt="${product.name}" loading="lazy">

        ${product.badge === "new" ? `<span class="badge-new">Mới</span>` : ""}
        ${product.badge === "sale" ? `<span class="badge-sale">Sale</span>` : ""}
        ${showSalePercent && product.oldPrice ? `<span class="badge-sale-pct">-${salePercent}%</span>` : ""}

        <button class="quick-add" type="button" onclick="event.stopPropagation(); addToCart('${product.id}')">
          <i class="bi bi-bag-plus"></i>
          Chọn nhanh
        </button>
      </div>

      <div class="product-body">
        <div class="product-category">${getCategoryName(product.category)}</div>

        <div class="product-rating">
          <i class="bi bi-star-fill"></i>
          <i class="bi bi-star-fill"></i>
          <i class="bi bi-star-fill"></i>
          <i class="bi bi-star-fill"></i>
          <i class="bi bi-star-half"></i>
          <span>4.8</span>
        </div>

        <div class="product-name">${product.name}</div>

        <div class="product-price">
          <span class="price-current">${formatMoney(product.price)}</span>
          ${product.oldPrice ? `<span class="price-old">${formatMoney(product.oldPrice)}</span>` : ""}
          ${showSalePercent && product.oldPrice ? `<span class="price-save">-${salePercent}%</span>` : ""}
        </div>

        <button class="btn-add-card" type="button" onclick="addToCart('${product.id}')">
          <i class="bi bi-bag-plus"></i>
          Chọn size / màu
        </button>
      </div>
    </div>
  `;
}

function renderNewStrip() {
  const newProducts = PRODUCTS.filter(item => item.badge === "new");

  newStrip.innerHTML = newProducts.map(item => `
    <div class="new-strip-item">
      ${createProductCard(item, false)}
    </div>
  `).join("");

  updateStripButtons();
}

function updateStripButtons() {
  const wrapper = document.querySelector(".new-strip-wrap");
  const prevBtn = document.getElementById("stripPrev");
  const nextBtn = document.getElementById("stripNext");

  if (!wrapper || !prevBtn || !nextBtn) return;

  prevBtn.disabled = wrapper.scrollLeft <= 2;
  nextBtn.disabled = wrapper.scrollLeft + wrapper.clientWidth >= wrapper.scrollWidth - 2;
}

function moveStrip(direction) {
  const wrapper = document.querySelector(".new-strip-wrap");
  if (!wrapper) return;

  wrapper.scrollBy({
    left: direction * STRIP_CARD_WIDTH,
    behavior: "smooth"
  });

  setTimeout(updateStripButtons, 350);
}

function getFilteredProducts() {
  let result = [...PRODUCTS];

  if (filters.category !== "all") {
    result = result.filter(item => item.category === filters.category);
  }

  if (filters.brand !== "all") {
    result = result.filter(item => item.brand === filters.brand);
  }

  result = result.filter(item => item.price <= filters.price);

  if (searchText) {
    const keyword = searchText.toLowerCase();

    result = result.filter(item =>
      item.name.toLowerCase().includes(keyword) ||
      item.brand.toLowerCase().includes(keyword)
    );
  }

  if (sortType === "price-asc") {
    result.sort((a, b) => a.price - b.price);
  }

  if (sortType === "price-desc") {
    result.sort((a, b) => b.price - a.price);
  }

  if (sortType === "name-asc") {
    result.sort((a, b) => a.name.localeCompare(b.name));
  }

  return result;
}

function renderProducts() {
  const products = getFilteredProducts();
  const saleProducts = products.filter(item => item.badge === "sale");

  document.getElementById("totalCount").textContent = products.length;

  renderSaleProducts(saleProducts);
  renderProductPage(products);
  renderPagination(products.length);
}

function renderSaleProducts(saleProducts) {
  if (!saleProducts.length) {
    saleWrap.style.display = "none";
    saleGrid.innerHTML = "";
    return;
  }

  saleWrap.style.display = "block";

  saleGrid.innerHTML = saleProducts.slice(0, 3).map(item => `
    <div class="col-sm-6 col-xl-4">
      ${createProductCard(item, true)}
    </div>
  `).join("");
}

function renderProductPage(products) {
  const start = (currentPage - 1) * PRODUCTS_PER_PAGE;
  const pageProducts = products.slice(start, start + PRODUCTS_PER_PAGE);

  document.getElementById("showingCount").textContent = pageProducts.length;

  if (!products.length) {
    productGrid.innerHTML = "";
    emptyState.style.display = "block";
    return;
  }

  emptyState.style.display = "none";

  productGrid.innerHTML = pageProducts.map(item => `
    <div class="col-sm-6 col-xl-4">
      ${createProductCard(item, true)}
    </div>
  `).join("");
}

function renderPagination(totalProducts) {
  const totalPages = Math.ceil(totalProducts / PRODUCTS_PER_PAGE);

  if (totalPages <= 1) {
    pagination.innerHTML = "";
    return;
  }

  let html = `
    <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
      <a class="page-link" href="#" onclick="changePage(${currentPage - 1}); return false;">‹</a>
    </li>
  `;

  for (let i = 1; i <= totalPages; i++) {
    html += `
      <li class="page-item ${i === currentPage ? "active" : ""}">
        <a class="page-link" href="#" onclick="changePage(${i}); return false;">${i}</a>
      </li>
    `;
  }

  html += `
    <li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
      <a class="page-link" href="#" onclick="changePage(${currentPage + 1}); return false;">›</a>
    </li>
  `;

  pagination.innerHTML = html;
}

function changePage(page) {
  const totalPages = Math.ceil(getFilteredProducts().length / PRODUCTS_PER_PAGE);

  if (page < 1 || page > totalPages) return;

  currentPage = page;
  renderProducts();

  document.querySelector(".product-toolbar").scrollIntoView({
    behavior: "smooth",
    block: "start"
  });
}

function resetFilters() {
  filters = {
    category: "all",
    brand: "all",
    price: 5000000
  };

  searchText = "";
  sortType = "default";
  currentPage = 1;

  document.getElementById("searchInput").value = "";
  document.getElementById("priceRange").value = 5000000;
  document.getElementById("priceLabel").textContent = "5.000.000đ";

  setCustomSelectValue("categorySelect", "all");
  setCustomSelectValue("brandSelect", "all");
  setCustomSelectValue("sortSelect", "default");

  renderProducts();
}

function setCustomSelectValue(id, value) {
  const select = document.getElementById(id);
  if (!select) return;

  const option = select.querySelector(`.sportix-option[data-value="${value}"]`);
  if (!option) return;

  select.dataset.value = value;
  select.querySelector(".sportix-select-btn span").textContent = option.textContent;

  select.querySelectorAll(".sportix-option").forEach(item => {
    item.classList.toggle("active", item.dataset.value === value);
  });
}

function initCustomSelect(id, onChange) {
  const select = document.getElementById(id);
  if (!select) return;

  const button = select.querySelector(".sportix-select-btn");
  const options = select.querySelectorAll(".sportix-option");

  button.addEventListener("click", event => {
    event.stopPropagation();

    document.querySelectorAll(".sportix-select.open").forEach(item => {
      if (item !== select) item.classList.remove("open");
    });

    select.classList.toggle("open");
  });

  options.forEach(option => {
    option.addEventListener("click", event => {
      event.stopPropagation();

      setCustomSelectValue(id, option.dataset.value);
      select.classList.remove("open");
      onChange(option.dataset.value);
    });
  });
}

function initFilterEvents() {
  initCustomSelect("categorySelect", value => {
    filters.category = value;
    currentPage = 1;
    renderProducts();
  });

  initCustomSelect("brandSelect", value => {
    filters.brand = value;
    currentPage = 1;
    renderProducts();
  });

  initCustomSelect("sortSelect", value => {
    sortType = value;
    currentPage = 1;
    renderProducts();
  });

  document.getElementById("searchInput").addEventListener("input", event => {
    searchText = event.target.value.trim();
    currentPage = 1;
    renderProducts();
  });

  document.getElementById("priceRange").addEventListener("input", event => {
    filters.price = Number(event.target.value);
    document.getElementById("priceLabel").textContent = formatMoney(filters.price);
    currentPage = 1;
    renderProducts();
  });

  document.addEventListener("click", () => {
    document.querySelectorAll(".sportix-select.open").forEach(item => {
      item.classList.remove("open");
    });
  });

  document.getElementById("stripPrev").addEventListener("click", () => moveStrip(-1));
  document.getElementById("stripNext").addEventListener("click", () => moveStrip(1));

  window.addEventListener("resize", updateStripButtons);
  document.querySelector(".new-strip-wrap")?.addEventListener("scroll", updateStripButtons);
}

function applyHomeCategory() {
  const homeCategory = sessionStorage.getItem("sportix_home_category");

  if (!homeCategory) return false;

  if (homeCategory === "new") {
    const newProducts = PRODUCTS.filter(item => item.badge === "new");

    searchText = "";
    sortType = "default";
    filters.category = "all";

    document.getElementById("totalCount").textContent = newProducts.length;
    renderSaleProducts([]);
    renderProductPage(newProducts);
    renderPagination(newProducts.length);

    sessionStorage.removeItem("sportix_home_category");
    return true;
  }

  filters.category = homeCategory;
  setCustomSelectValue("categorySelect", homeCategory);
  sessionStorage.removeItem("sportix_home_category");

  return false;
}

function goHomeCategory(category) {
  sessionStorage.setItem("sportix_home_category", category);
  goWithSplash("product-list.html");
}

document.addEventListener("DOMContentLoaded", () => {
  updateCartCount();
  renderNewStrip();
  initFilterEvents();

  const alreadyRendered = applyHomeCategory();

  if (!alreadyRendered) {
    renderProducts();
  }
});