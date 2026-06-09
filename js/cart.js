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

            <img src="${item.image || item.img}" class="cart-img" alt="${item.name}">

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

function getStockMap() {
    // Lấy danh sách sản phẩm từ localStorage admin (sportix_products)
    const products = JSON.parse(localStorage.getItem("sportix_products")) || [];
    const map = {};
    products.forEach(p => {
        map[String(p.id)] = p.stock ?? 9999;
    });
    return map;
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

    // Lưu checkout trước để nếu chưa đăng nhập,
    // đăng nhập xong quay lại checkout vẫn còn sản phẩm
    localStorage.setItem(CHECKOUT_KEY, JSON.stringify(selectedItems));

    if (!isLoggedIn()) {
        sessionStorage.setItem("redirectAfterLogin", "checkout.html");

        showCartToast(
            "Cần đăng nhập",
            "Vui lòng đăng nhập trước khi thanh toán."
        );

        setTimeout(() => goWithSplash("login.html"), 900);
        return;
    }

    // Kiểm tra tồn kho sau khi đã đăng nhập
    const stockMap = getStockMap();
    const outOfStock = [];

    for (const item of selectedItems) {
        const available = stockMap[String(item.id)];

        if (available === undefined) continue;

        if (item.qty > available) {
            outOfStock.push({
                name: item.name,
                qty: item.qty,
                available: available
            });
        }
    }

    if (outOfStock.length) {
        localStorage.removeItem(CHECKOUT_KEY);
        showStockErrorModal(outOfStock);
        return;
    }

    localStorage.setItem(CHECKOUT_KEY, JSON.stringify(selectedItems));
    goWithSplash("checkout.html");
}

function showStockErrorModal(errors) {
    // Tạo modal nếu chưa có
    let modal = document.getElementById("stockErrorModal");
    if (!modal) {
        document.body.insertAdjacentHTML("beforeend", `
            <div class="modal fade" id="stockErrorModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content" style="border-radius:18px;overflow:hidden">
                        <div class="modal-header" style="background:var(--danger,#ef4444);color:#fff;border:none">
                            <h5 class="modal-title">
                                <i class="bi bi-exclamation-triangle-fill me-2"></i>
                                Không đủ hàng trong kho
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body" id="stockErrorBody" style="padding:24px"></div>
                        <div class="modal-footer" style="border:none;padding:16px 24px">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        </div>
                    </div>
                </div>
            </div>
        `);
        modal = document.getElementById("stockErrorModal");
    }

    const body = document.getElementById("stockErrorBody");
    body.innerHTML = `
        <p style="margin-bottom:14px;color:var(--muted,#666)">
            Một số sản phẩm trong giỏ hàng vượt quá số lượng tồn kho. Vui lòng điều chỉnh lại số lượng:
        </p>
        <ul style="list-style:none;padding:0;margin:0;display:flex;flex-direction:column;gap:10px">
            ${errors.map(p => `
                <li style="display:flex;justify-content:space-between;align-items:center;
                            background:#fff5f5;border:1px solid #fecaca;border-radius:10px;padding:12px 16px">
                    <span style="font-weight:600">${p.name}</span>
                    <span style="font-size:13px;color:#ef4444">
                        ${p.available === 0
            ? "Hết hàng"
            : `Bạn chọn <b>${p.qty}</b> · Còn lại <b>${p.available}</b>`
        }
                    </span>
                </li>
            `).join("")}
        </ul>
    `;

    new bootstrap.Modal(modal).show();
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