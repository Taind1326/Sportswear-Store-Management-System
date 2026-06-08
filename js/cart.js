const CART_KEY = "sportix_cart";

const demoCart = [
  {
    id: "SP001",
    name: "Giày chạy bộ Nike Air Zoom",
    category: "Giày",
    price: 2490000,
    qty: 1,
    image: "../img/shoe-1.jpg",
    selected: true
  },
  {
    id: "SP002",
    name: "Áo thun Adidas Training",
    category: "Áo",
    price: 690000,
    qty: 2,
    image: "../img/shirt-1.jpg",
    selected: true
  }
];

let cart = loadCart();

function formatMoney(value) {
  return value.toLocaleString("vi-VN") + "đ";
}

function loadCart() {
  const data = localStorage.getItem(CART_KEY);

  if (!data) {
    localStorage.setItem(CART_KEY, JSON.stringify(demoCart));
    return demoCart;
  }

  return JSON.parse(data);
}

function saveCart() {
  localStorage.setItem(CART_KEY, JSON.stringify(cart));
}

function renderCart() {
  const cartList = document.getElementById("cartList");
  const selectAll = document.getElementById("selectAll");

  if (!cart.length) {
    cartList.innerHTML = `
      <div class="cart-empty">
        <i class="bi bi-cart-x"></i>
        <h5>Giỏ hàng đang trống</h5>
        <p>Hãy chọn thêm sản phẩm yêu thích của bạn.</p>
      </div>
    `;
    selectAll.checked = false;
    updateTotal();
    return;
  }

  cartList.innerHTML = cart.map(item => `
    <div class="cart-item">
      <input type="checkbox"
             class="cart-item-check"
             ${item.selected ? "checked" : ""}
             onchange="toggleItem('${item.id}')">

      <img src="${item.image}" class="cart-img" alt="${item.name}">

      <div>
        <div class="cart-name">${item.name}</div>
        <div class="cart-meta">${item.category}</div>
        <div class="cart-price">${formatMoney(item.price)}</div>
      </div>

      <div class="cart-actions">
        <div class="qty-box">
          <button onclick="changeQty('${item.id}', -1)">−</button>
          <span>${item.qty}</span>
          <button onclick="changeQty('${item.id}', 1)">+</button>
        </div>

        <button class="btn-remove" onclick="removeItem('${item.id}')">
          <i class="bi bi-trash"></i> Xóa
        </button>
      </div>
    </div>
  `).join("");

  selectAll.checked = cart.every(item => item.selected);
  updateTotal();
}

function updateTotal() {
  const total = cart
    .filter(item => item.selected)
    .reduce((sum, item) => sum + item.price * item.qty, 0);

  document.getElementById("totalPrice").textContent = formatMoney(total);
}

function toggleAll() {
  const checked = document.getElementById("selectAll").checked;

  cart = cart.map(item => ({
    ...item,
    selected: checked
  }));

  saveCart();
  renderCart();
}

function toggleItem(id) {
  cart = cart.map(item => {
    if (item.id === id) {
      return {
        ...item,
        selected: !item.selected
      };
    }

    return item;
  });

  saveCart();
  renderCart();
}

function changeQty(id, amount) {
  cart = cart.map(item => {
    if (item.id === id) {
      return {
        ...item,
        qty: Math.max(1, item.qty + amount)
      };
    }

    return item;
  });

  saveCart();
  renderCart();
}

function removeItem(id) {
  cart = cart.filter(item => item.id !== id);
  saveCart();
  renderCart();
}

function isLoggedIn(){
  const user = localStorage.getItem("sportix_user");

  if(!user) return false;

  try{
    const data = JSON.parse(user);
    return !!(data && data.email);
  }catch{
    return false;
  }
}

function goCheckout(){
  const selectedItems = cart.filter(item => item.selected);

  if(!selectedItems.length){
    showCartToast(
      "Chưa chọn sản phẩm",
      "Bạn cần chọn ít nhất 1 sản phẩm để thanh toán."
    );
    return;
  }

  localStorage.setItem("sportix_checkout", JSON.stringify(selectedItems));

  if(!isLoggedIn()){
    sessionStorage.setItem("redirectAfterLogin", "checkout.html");
    showCartToast(
      "Cần đăng nhập",
      "Vui lòng đăng nhập trước khi thanh toán."
    );

    setTimeout(() => {
      goWithSplash("login.html");
    }, 900);

    return;
  }

  goWithSplash("checkout.html");
}

document.addEventListener("DOMContentLoaded", renderCart);


function showCartToast(title, message) {
  const toast = document.getElementById("cartToast");

  if (!toast) return;

  toast.querySelector("strong").textContent = title;
  toast.querySelector("p").textContent = message;

  toast.classList.add("show");

  clearTimeout(showCartToast.timer);

  showCartToast.timer = setTimeout(() => {
    toast.classList.remove("show");
  }, 2600);
}