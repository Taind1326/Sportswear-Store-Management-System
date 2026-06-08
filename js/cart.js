function safeJSON(key, fallback = null) {
    try {
        return JSON.parse(localStorage.getItem(key)) || fallback;
    } catch {
        return fallback;
    }
}

function getCurrentUser() {
    return safeJSON("sportix_user") ||
           safeJSON("sportix_current_user") ||
           safeJSON("currentUser") ||
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
const CHECKOUT_KEY = "sportix_checkout";
const ORDERS_KEY = "sportix_orders";

let cart = loadCart();

function formatMoney(value) {
    return value.toLocaleString("vi-VN") + "đ";
}

function loadCart() {
    const userCart = safeJSON(CART_KEY, []);

    if (userCart.length) return userCart;

    const oldCart = safeJSON("sportix_cart", []);

    if (oldCart.length && CART_KEY !== "sportix_cart") {
        localStorage.setItem(CART_KEY, JSON.stringify(oldCart));
        localStorage.removeItem("sportix_cart");
        return oldCart;
    }

    return [];
}

const CATEGORY_LABELS = {
    giay: "Giày",
    ao: "Quần áo",
    gym: "Dụng cụ gym",
    bongda: "Bóng đá",
    phukien: "Phụ kiện"
};

function getItemCode(item) {
    return item.key || `${item.id}-${item.size || ""}-${item.color || ""}`;
}

function migrateCartCategories() {
    let changed = false;

    cart = cart.map(item => {
        let newItem = { ...item };

        if (!newItem.key) {
            newItem.key = getItemCode(newItem);
            changed = true;
        }

        if (!newItem.category) {
            const found = typeof PRODUCTS !== "undefined"
                ? PRODUCTS.find(p => String(p.id) === String(newItem.id))
                : null;

            const slug = found ? found.category : "";
            newItem.category = CATEGORY_LABELS[slug] || slug;
            changed = true;
        }

        if (newItem.selected === undefined) {
            newItem.selected = true;
            changed = true;
        }

        return newItem;
    });

    if (changed) saveCart();
}

function saveCart() {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
}

function renderCart() {
    const cartList = document.getElementById("cartList");
    const selectAll = document.getElementById("selectAll");

    if (!cartList || !selectAll) return;

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
                   onchange="toggleItem('${getItemCode(item)}')">

            <img src="${item.image}" class="cart-img" alt="${item.name}">

            <div>
                <div class="cart-name">${item.name}</div>
                <div class="cart-meta">
                    ${item.category || ""}
                    ${item.size ? ` | Size: ${item.size}` : ""}
                    ${item.color ? ` | Màu: ${item.color}` : ""}
                </div>
                <div class="cart-price">${formatMoney(item.price)}</div>
            </div>

            <div class="cart-actions">
                <div class="qty-box">
                    <button onclick="changeQty('${getItemCode(item)}', -1)">−</button>
                    <span>${item.qty}</span>
                    <button onclick="changeQty('${getItemCode(item)}', 1)">+</button>
                </div>

                <button class="btn-remove" onclick="removeItem('${getItemCode(item)}')">
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

function toggleItem(code) {
    cart = cart.map(item => {
        if (getItemCode(item) === String(code)) {
            return { ...item, selected: !item.selected };
        }

        return item;
    });

    saveCart();
    renderCart();
}

function changeQty(code, amount) {
    cart = cart.map(item => {
        if (getItemCode(item) === String(code)) {
            return { ...item, qty: Math.max(1, item.qty + amount) };
        }

        return item;
    });

    saveCart();
    renderCart();
}

function removeItem(code) {
    cart = cart.filter(item => getItemCode(item) !== String(code));
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

    const missingOption = selectedItems.find(item => !item.size || !item.color);

    if (missingOption) {
        showCartToast(
            "Thiếu thông tin sản phẩm",
            `Vui lòng chọn size và màu cho sản phẩm: ${missingOption.name}.`
        );
        return;
    }

    localStorage.setItem(CHECKOUT_KEY, JSON.stringify(selectedItems));

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

    goWithSplash("checkout.html");
}

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
    const orders = safeJSON(ORDERS_KEY, []);
    const myOrders = orders.filter(order => order.userAccount === account);

    const container = document.getElementById("orderHistoryContent");
    if (!container) return;

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

                <div><strong>${order.customer.name}</strong></div>
                <div>${order.customer.phone}</div>

                <span class="order-status">
                    <i class="bi bi-truck"></i>
                    ${order.status || "Đang giao"}
                </span>

                <div class="order-products">
                    ${order.items.map(item => `
                        <div class="order-product">
                            <span>
                                ${item.name} x${item.qty}
                                ${item.size ? ` - Size ${item.size}` : ""}
                                ${item.color ? ` - ${item.color}` : ""}
                            </span>
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

    const modal = new bootstrap.Modal(document.getElementById("orderHistoryModal"));
    modal.show();
}

document.addEventListener("DOMContentLoaded", () => {
    migrateCartCategories();
    renderCart();
});