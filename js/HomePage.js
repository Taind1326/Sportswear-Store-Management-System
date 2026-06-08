const featuredProducts = document.getElementById("featuredProducts");
const btnToggleProducts = document.getElementById("btnToggleProducts");
const backToTop = document.getElementById("backToTop");
const cartCount = document.getElementById("cartCount");

let currentSlide = 0;
let slideTimer = null;
let expanded = false;

const slides = document.querySelectorAll(".hero-slide");
const dotsWrap = document.getElementById("heroDots");

const products = [...PRODUCTS]
  .sort(() => Math.random() - 0.5)
  .slice(0, 8);

function formatPrice(value){
  return value.toLocaleString("vi-VN") + "đ";
}

function getCategoryName(category){
  const names = {
    giay:"Giày thể thao",
    ao:"Quần áo",
    gym:"Dụng cụ gym",
    bongda:"Bóng đá",
    phukien:"Phụ kiện"
  };

  return names[category] || category;
}

function initSlider(){
  dotsWrap.innerHTML = "";

  slides.forEach((_, index) => {
    const dot = document.createElement("button");
    dot.className = index === 0 ? "hero-dot active" : "hero-dot";
    dot.addEventListener("click", () => showSlide(index));
    dotsWrap.appendChild(dot);
  });

  document.getElementById("prevSlide").addEventListener("click", () => {
    showSlide(currentSlide - 1);
  });

  document.getElementById("nextSlide").addEventListener("click", () => {
    showSlide(currentSlide + 1);
  });

  startAutoSlide();
}

function showSlide(index){
  const dots = document.querySelectorAll(".hero-dot");

  if(index < 0) index = slides.length - 1;
  if(index >= slides.length) index = 0;

  slides[currentSlide].classList.remove("active");
  dots[currentSlide].classList.remove("active");

  currentSlide = index;

  slides[currentSlide].classList.add("active");
  dots[currentSlide].classList.add("active");

  startAutoSlide();
}

function startAutoSlide(){
  clearInterval(slideTimer);

  slideTimer = setInterval(() => {
    showSlide(currentSlide + 1);
  }, 4300);
}

function renderProducts(showAll = false){
  featuredProducts.innerHTML = "";

  const visibleProducts = showAll ? products : products.slice(0, 4);

  visibleProducts.forEach(product => {
    const col = document.createElement("div");
    col.className = "col-12 col-sm-6 col-lg-3";

    const oldPrice = product.oldPrice
      ? `<span class="home-product-old">${formatPrice(product.oldPrice)}</span>`
      : "";

    const badge = product.badge
      ? `<span class="product-badge">${product.badge === "sale" ? "SALE" : "MỚI"}</span>`
      : "";

    col.innerHTML = `
      <div class="home-product-card">
        <div class="home-product-img">
          ${badge}
          <img src="${product.img}" alt="${product.name}">
        </div>

        <div class="home-product-body">
          <h5>${product.name}</h5>
          <div class="home-product-category">${getCategoryName(product.category)}</div>

          <div>
            <span class="home-product-price">${formatPrice(product.price)}</span>
            ${oldPrice}
          </div>

          <div class="home-product-actions">
            <button class="btn-view" onclick="goWithSplash('product-detail.html?id=${product.id}')">
              Xem
            </button>
            <button class="btn-add" onclick="addToCart('${product.id}')">
              <i class="bi bi-bag-plus"></i>
            </button>
          </div>
        </div>
      </div>
    `;

    featuredProducts.appendChild(col);
  });
}


function getCart(){
  return JSON.parse(localStorage.getItem("sportix_cart") || "[]");
}

function saveCart(cart){
  localStorage.setItem("sportix_cart", JSON.stringify(cart));
}

function updateCartCount(){
  const cart = getCart();
  const total = cart.reduce((sum, item) => sum + item.qty, 0);
  cartCount.textContent = total;
}

function addToCart(id){
  const product = PRODUCTS.find(item => item.id === id);
  if(!product) return;

  const cart = getCart();
  const existed = cart.find(item => item.id === id);

  if(existed){
    existed.qty++;
  }else{
    cart.push({
      id:product.id,
      name:product.name,
      price:product.price,
      image:product.img,
      qty:1,
      selected:true
    });
  }

  saveCart(cart);
  updateCartCount();
  showToast(`Đã thêm ${product.name} vào giỏ hàng.`);
}

function showToast(message){
  const toast = document.getElementById("homeToast");
  toast.querySelector("span").textContent = message;
  toast.classList.add("show");

  clearTimeout(showToast.timer);

  showToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2200);
}

function initBackToTop(){
  window.addEventListener("scroll", () => {
    if(window.scrollY > 500){
      backToTop.classList.add("show");
    }else{
      backToTop.classList.remove("show");
    }
  });

  backToTop.addEventListener("click", () => {
    window.scrollTo({ top:0, behavior:"smooth" });
  });
}

function scrollToProducts(){
  document.getElementById("products").scrollIntoView({
    behavior:"smooth",
    block:"start"
  });
}

function isLoggedIn(){
  return localStorage.getItem("sportix_user") !== null;
}

function requireLogin(targetPage){
  if(!isLoggedIn()){
    sessionStorage.setItem("redirectAfterLogin", targetPage);
    goWithSplash("login.html");
    return false;
  }

  goWithSplash(targetPage);
  return true;
}


function buyNow(){
  requireLogin("product-list.html");
}

document.addEventListener("DOMContentLoaded", () => {
  initSlider();
  renderProducts();
  updateCartCount();
  initBackToTop();

btnToggleProducts.addEventListener("click", () => {
  goWithSplash("product-list.html");
});
});