function getCurrentUser() {
    return JSON.parse(localStorage.getItem("sportix_user")) ||
           JSON.parse(localStorage.getItem("sportix_current_user")) ||
           JSON.parse(localStorage.getItem("currentUser")) ||
           null;
}

function getUserAccount() {
    const user = getCurrentUser();
    return user?.email || user?.phone || user?.username || user?.id || null;
}

function getUserKey(key) {
    const account = getUserAccount();
    return account ? `${key}_${account}` : key;
}

const CART_KEY = getUserKey("sportix_cart");
const CHECKOUT_KEY = getUserKey("sportix_checkout");
const ORDERS_KEY = "sportix_orders";

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

const CATEGORY_LABELS = {
    giay: "Giày",
    ao: "Quần áo",
    gym: "Dụng cụ gym",
    bongda: "Bóng đá",
    phukien: "Phụ kiện"
};

function migrateCartCategories() {
    let changed = false;

    cart = cart.map(item => {
        if (item.category) return item;

        const found = typeof PRODUCTS !== "undefined"
            ? PRODUCTS.find(p => String(p.id) === String(item.id))
            : null;

        changed = true;
        const slug = found ? found.category : "";
        return { ...item, category: CATEGORY_LABELS[slug] || slug };
    });

    if (changed) saveCart();
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
                <div class="cart-meta">${item.category || ""}</div>
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
        if (String(item.id) === String(id)) {
            return { ...item, selected: !item.selected };
        }
        return item;
    });

    saveCart();
    renderCart();
}

function changeQty(id, amount) {
    cart = cart.map(item => {
        if (String(item.id) === String(id)) {
            return { ...item, qty: Math.max(1, item.qty + amount) };
        }
        return item;
    });

    saveCart();
    renderCart();
}

function removeItem(id) {
    cart = cart.filter(item => String(item.id) !== String(id));
    saveCart();
    renderCart();
}

function isLoggedIn() {
    return !!getUserAccount();
}

function goCheckout() {
    const selectedItems = cart.filter(item => item.selected);

    if (!selectedItems.length) {
        showCartToast(
            "Chưa chọn sản phẩm",
            "Bạn cần chọn ít nhất 1 sản phẩm để thanh toán."
        );
        return;
    }

    if (!isLoggedIn()) {
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

    localStorage.setItem(CHECKOUT_KEY, JSON.stringify(selectedItems));
    goWithSplash("checkout.html");
}

document.addEventListener("DOMContentLoaded", () => {
    migrateCartCategories();
    renderCart();
});

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

function goCartCategory(category) {
    sessionStorage.setItem("sportix_home_category", category);
    goWithSplash("product-list.html");
}

function showOrderHistory() {
    const account = getUserAccount();

    const orders = JSON.parse(localStorage.getItem(ORDERS_KEY)) || [];

    const myOrders = account
        ? orders.filter(order =>
            order.userAccount === account ||
            order.customer?.email === account ||
            order.customer?.phone === account
        )
        : [];

    const container = document.getElementById("orderHistoryContent");

    if (!myOrders.length) {
        container.innerHTML = `
            <div class="text-center py-5 text-muted">
                <i class="bi bi-bag-x fs-1 d-block mb-3"></i>
                Bạn chưa có đơn hàng nào.
            </div>
        `;
    } else {
        container.innerHTML = [...myOrders].reverse().map(order => `
            <div class="order-card">

                <div class="order-top">
                    <div class="order-id">Mã đơn: ${order.id}</div>
                    <div class="order-date">${order.date}</div>
                </div>

                <div>
                    <strong>${order.customer.name}</strong>
                </div>

                <div>${order.customer.phone}</div>

                <span class="order-status">
                    <i class="bi bi-truck"></i>
                    Đang giao
                </span>

                <div class="order-delivery-note">
                    <i class="bi bi-shield-check-fill"></i>
                    Dịch vụ Đảm bảo giao hàng đúng hạn cam đoan sẽ có lần đến giao chậm nhất là sau 5 ngày kể từ ngày đặt hàng.
                    Nhận voucher nếu đơn đến muộn.
                </div>

                <div class="order-products">
                    ${order.items.map(item => `
                        <div class="order-product">
                            <span>${item.name} x${item.qty}</span>
                            <strong>${formatMoney(item.price * item.qty)}</strong>
                        </div>
                    `).join("")}
                </div>

                <div class="order-total">
                    ${formatMoney(order.total)}
                </div>

            </div>
        `).join("");
    }

    const modal = new bootstrap.Modal(
        document.getElementById("orderHistoryModal")
    );

    modal.show();
}