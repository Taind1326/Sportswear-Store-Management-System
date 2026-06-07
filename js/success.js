const LAST_ORDER_KEY = "sportix_last_order";

function formatMoney(value){
  return Number(value || 0).toLocaleString("vi-VN") + "đ";
}

function getOrder(){
  const data = localStorage.getItem(LAST_ORDER_KEY);

  if(data){
    return JSON.parse(data);
  }

  return null;
}

function renderSuccessPage(){
  const order = getOrder();

  if(!order){
    renderEmptyOrder();
    return;
  }

  document.getElementById("customerName").textContent = order.customer.name;
  document.getElementById("customerPhone").textContent = order.customer.phone;
  document.getElementById("customerAddress").textContent = order.customer.address;
  document.getElementById("orderId").textContent = order.id;
  document.getElementById("orderDate").textContent = order.date;
  document.getElementById("paymentMethod").textContent = order.paymentText;

  document.getElementById("subTotal").textContent = formatMoney(order.subTotal);
  document.getElementById("shippingFee").textContent =
    order.shippingFee === 0 ? "Miễn phí" : formatMoney(order.shippingFee);
  document.getElementById("finalTotal").textContent = formatMoney(order.total);

  renderProducts(order.items);
}

function renderProducts(items){
  const productList = document.getElementById("productList");

  if(!items || !items.length){
    productList.innerHTML = `
      <div class="text-center text-muted py-3">
        Không có sản phẩm nào.
      </div>
    `;
    return;
  }

  productList.innerHTML = items.map(item => `
    <div class="product-item">
      <div class="product-left">
        <img src="${item.image}" alt="${item.name}">
        <div>
          <div class="product-name">${item.name}</div>
          <div class="product-qty">Số lượng: ${item.qty}</div>
        </div>
      </div>

      <div class="product-price">
        ${formatMoney(item.price * item.qty)}
      </div>
    </div>
  `).join("");
}

function renderEmptyOrder(){
  document.getElementById("orderId").textContent = "SPX000000";
  document.getElementById("customerName").textContent = "---";
  document.getElementById("customerPhone").textContent = "---";
  document.getElementById("customerAddress").textContent = "---";
  document.getElementById("orderDate").textContent = "---";
  document.getElementById("paymentMethod").textContent = "---";
  document.getElementById("subTotal").textContent = "0đ";
  document.getElementById("shippingFee").textContent = "0đ";
  document.getElementById("finalTotal").textContent = "0đ";

  document.getElementById("productList").innerHTML = `
    <div class="text-center text-muted py-3">
      Không tìm thấy thông tin đơn hàng.
    </div>
  `;
}

document.addEventListener("DOMContentLoaded", renderSuccessPage);