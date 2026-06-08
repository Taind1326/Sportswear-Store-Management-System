let currentProduct = null;
let selectedSize = null;
let selectedColor = "Đen";
let isLiked = false;

const CART_KEY = "sportix_cart";
let cart = JSON.parse(localStorage.getItem(CART_KEY) || "[]");

function getProductIdFromUrl(){
  const params = new URLSearchParams(window.location.search);
  const id = params.get("id");

  if(id) return String(id);

  const match = window.location.href.match(/[?&]id=([^&]+)/);
  return match ? String(match[1]) : String(PRODUCTS[0]?.id || "");
}

function formatMoney(value){
  return value.toLocaleString("vi-VN") + "đ";
}

function updateCartCount(){
  const total = cart.reduce((sum, item) => sum + item.qty, 0);
  document.getElementById("cartCount").textContent = total;
}

function initDetail(){
  const id = getProductIdFromUrl();
  currentProduct = PRODUCTS.find(item => String(item.id) === id) || PRODUCTS[0];

  renderProductDetail();
  renderRelatedProducts();
  updateCartCount();
  initColorEvents();
}

function renderProductDetail(){
  const product = currentProduct;

  document.title = `${product.name} - SPORTIX`;
  document.getElementById("breadProduct").textContent = product.name;
  document.getElementById("infoBrand").textContent = product.brand;
  document.getElementById("infoName").textContent = product.name;
  document.getElementById("infoPrice").textContent = formatMoney(product.price);

  const oldPrice = document.getElementById("infoOldPrice");
  const discount = document.getElementById("infoDiscount");

  if(product.oldPrice){
    const percent = Math.round((1 - product.price / product.oldPrice) * 100);
    oldPrice.textContent = formatMoney(product.oldPrice);
    discount.style.display = "inline-flex";
    discount.textContent = `-${percent}%`;
  }else{
    oldPrice.textContent = "";
    discount.style.display = "none";
  }

  const badge = document.getElementById("mainBadge");

  if(product.badge === "new"){
    badge.textContent = "Mới";
    badge.className = "badge-detail badge-new";
  }else if(product.badge === "sale"){
    badge.textContent = "Sale";
    badge.className = "badge-detail badge-sale";
  }else{
    badge.textContent = "";
    badge.className = "badge-detail";
  }

  const images = product.imgs && product.imgs.length ? product.imgs : [product.img];
  document.getElementById("mainImg").src = images[0];
  document.getElementById("mainImg").alt = product.name;

  document.getElementById("thumbList").innerHTML = images.map((image, index) => `
    <button class="thumb ${index === 0 ? "active" : ""}" onclick="changeMainImage('${image}', this)">
      <img src="${image}" alt="${product.name}">
    </button>
  `).join("");

  document.getElementById("sizeButtons").innerHTML = product.sizes.map(size => {
    const unavailable = product.unavailable && product.unavailable.includes(size);

    return `
      <button class="size-btn ${unavailable ? "unavailable" : ""}"
        ${unavailable ? "disabled" : `onclick="selectSize(this, '${size}')"`}>
        ${size}
      </button>
    `;
  }).join("");

  document.getElementById("tabDescription").textContent = product.desc;
  document.getElementById("specTable").innerHTML = product.specs.map(([key, value]) => `
    <tr>
      <td>${key}</td>
      <td>${value}</td>
    </tr>
  `).join("");

  renderReviews();
}

function renderReviews(){
  const reviewList = document.getElementById("reviewList");

  if(typeof DANH_GIA === "undefined" || !DANH_GIA.length){
    reviewList.innerHTML = `<div class="review-empty">Chưa có đánh giá nào cho sản phẩm này.</div>`;
    return;
  }

  reviewList.innerHTML = DANH_GIA.map(review => `
    <div class="review-card">
      <div class="review-head">
        <strong>${review.user}</strong>
        <span>${review.date}</span>
      </div>
      <div class="review-stars">${"★".repeat(review.rating)}${"☆".repeat(5 - review.rating)}</div>
      <p>${review.text}</p>
    </div>
  `).join("");
}

function renderRelatedProducts(){
  const related = PRODUCTS
    .filter(item => item.id !== currentProduct.id && item.category === currentProduct.category)
    .slice(0, 4);

  const fallback = PRODUCTS
    .filter(item => item.id !== currentProduct.id)
    .slice(0, 4);

  const products = related.length >= 2 ? related : fallback;

  document.getElementById("relatedGrid").innerHTML = products.map(item => `
    <div class="col-6 col-md-3">
      <a href="#" onclick="goWithSplash('product-detail.html?id=${item.id}'); return false;" class="related-card">
        <img src="${item.img}" alt="${item.name}">
        <div class="related-card-body">
          <div class="related-card-name">${item.name}</div>
          <div class="related-card-price">${formatMoney(item.price)}</div>
        </div>
      </a>
    </div>
  `).join("");
}

function changeMainImage(src, button){
  document.getElementById("mainImg").src = src;

  document.querySelectorAll(".thumb").forEach(item => {
    item.classList.remove("active");
  });

  button.classList.add("active");
}

function selectSize(button, size){
  document.querySelectorAll(".size-btn").forEach(item => {
    item.classList.remove("active");
  });

  button.classList.add("active");
  selectedSize = size;
  document.getElementById("sizeLabel").textContent = size;
}

function initColorEvents(){
  document.querySelectorAll(".color-swatch").forEach(button => {
    button.addEventListener("click", () => {
      document.querySelectorAll(".color-swatch").forEach(item => {
        item.classList.remove("active");
      });

      button.classList.add("active");
      selectedColor = button.dataset.color;
      document.getElementById("colorLabel").textContent = selectedColor;
    });
  });
}

function changeQuantity(step){
  const input = document.getElementById("qtyInput");
  let quantity = Number(input.value) || 1;

  quantity += step;

  if(quantity < 1) quantity = 1;
  if(quantity > 99) quantity = 99;

  input.value = quantity;
}

function getQuantity(){
  const quantity = Number(document.getElementById("qtyInput").value) || 1;
  return Math.min(Math.max(quantity, 1), 99);
}

function addCurrentToCart(){
  if(!selectedSize){
    showToast("Vui lòng chọn kích thước trước khi thêm vào giỏ.", false);
    return false;
  }

  const quantity = getQuantity();
  const key = `${currentProduct.id}-${selectedSize}-${selectedColor}`;
  const existed = cart.find(item => item.key === key);

  if(existed){
    existed.qty += quantity;
  }else{
    cart.push({
      key:key,
      id:currentProduct.id,
      name:currentProduct.name,
      price:currentProduct.price,
      image:currentProduct.img,
      qty:quantity,
      size:selectedSize,
      color:selectedColor,
      selected:true
    });
  }

  localStorage.setItem(CART_KEY, JSON.stringify(cart));
  updateCartCount();
  showToast(`Đã thêm ${currentProduct.name} vào giỏ hàng.`, true);
  return true;
}

function isLoggedIn(){
  return localStorage.getItem("sportix_user") !== null;
}

function buyCurrentNow(){
  if(!addCurrentToCart()) return;

  if(!isLoggedIn()){
    sessionStorage.setItem("redirectAfterLogin", "cart.html");
    goWithSplash("login.html");
    return;
  }

  goWithSplash("cart.html");
}

function toggleWishlist(){
  isLiked = !isLiked;

  const button = document.getElementById("wishBtn");
  button.classList.toggle("liked", isLiked);
  button.innerHTML = isLiked ? `<i class="bi bi-heart-fill"></i>` : `<i class="bi bi-heart"></i>`;

  showToast(isLiked ? "Đã thêm vào yêu thích." : "Đã xóa khỏi yêu thích.", true);
}

function switchTab(id, button){
  document.querySelectorAll(".tab-content").forEach(item => {
    item.classList.remove("active");
  });

  document.querySelectorAll(".tab-btn").forEach(item => {
    item.classList.remove("active");
  });

  document.getElementById(`tab-${id}`).classList.add("active");
  button.classList.add("active");
}

function showToast(message, success = true){
  const toast = document.getElementById("detailToast");

  toast.querySelector("span").textContent = message;
  toast.querySelector("i").className = success ? "bi bi-check-circle-fill" : "bi bi-exclamation-circle-fill";
  toast.classList.add("show");

  clearTimeout(showToast.timer);
  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2400);
}

document.addEventListener("DOMContentLoaded", initDetail);
