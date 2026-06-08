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

let orderItems = [];
let shippingFee = 30000;
let discountAmount = 0;
let qrTimer = null;
let qrSeconds = 300;

function formatMoney(value) {
    return value.toLocaleString("vi-VN") + "đ";
}

function showCheckoutToast(title, message) {
    const toast = document.getElementById("checkoutToast");
    if (!toast) return;

    toast.querySelector("strong").textContent = title;
    toast.querySelector("p").textContent = message;
    toast.classList.add("show");

    clearTimeout(showCheckoutToast.timer);
    showCheckoutToast.timer = setTimeout(() => {
        toast.classList.remove("show");
    }, 2600);
}

function showError(input, errorId, message) {
    const errorEl = document.getElementById(errorId);
    if (!input || !errorEl) return;

    input.classList.add("is-invalid");
    input.classList.remove("is-valid");

    const span = errorEl.querySelector("span");
    if (span && message) span.textContent = message;

    errorEl.classList.add("show");
}

function showSuccess(input, errorId) {
    const errorEl = document.getElementById(errorId);
    if (!input || !errorEl) return;

    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    errorEl.classList.remove("show");
}

function clearState(input, errorId) {
    const errorEl = document.getElementById(errorId);
    if (!input || !errorEl) return;

    input.classList.remove("is-invalid", "is-valid");
    errorEl.classList.remove("show");
}

function loadCheckoutItems() {
    const checkoutData = localStorage.getItem(CHECKOUT_KEY);
    const cartData = localStorage.getItem(CART_KEY);

    if (checkoutData) {
        orderItems = safeJSON(CHECKOUT_KEY, []);
        return;
    }

    if (cartData) {
        const cartItems = safeJSON(CART_KEY, []);
        orderItems = cartItems.filter(item => item.selected);
        localStorage.setItem(CHECKOUT_KEY, JSON.stringify(orderItems));
        return;
    }

    orderItems = [];
}

function renderOrderList() {
    const orderList = document.getElementById("orderList");
    if (!orderList) return;

    if (!orderItems.length) {
        orderList.innerHTML = `
            <div class="text-center text-muted py-3">
                Chưa có sản phẩm nào được chọn.
            </div>
        `;
        updateTotal();
        return;
    }

    orderList.innerHTML = orderItems.map(item => `
        <div class="order-item">
            <div class="order-info">
                <img src="${item.image}" alt="${item.name}">
                <div>
                    <div class="order-name">${item.name}</div>
                    <div class="order-qty">
                        Số lượng: ${item.qty}
                        ${item.size ? ` | Size: ${item.size}` : ""}
                        ${item.color ? ` | Màu: ${item.color}` : ""}
                    </div>
                </div>
            </div>
            <div class="order-price">${formatMoney(item.price * item.qty)}</div>
        </div>
    `).join("");

    updateTotal();
}

function getSubTotal() {
    return orderItems.reduce((sum, item) => sum + item.price * item.qty, 0);
}

function getBaseShippingFee() {
    const subTotal = getSubTotal();
    const province = document.getElementById("province")?.value;

    if (subTotal >= 2000000) return 0;
    if (province === "TP. Hồ Chí Minh") return 30000;
    if (province) return 50000;

    return 30000;
}

function getDiscountAmount(subTotal) {
    const codeInput = document.getElementById("discountCode");
    const code = codeInput ? codeInput.value.trim().toUpperCase() : "";

    if (!code) return 0;

    const vouchers = safeJSON("sportix_vouchers", []);
    const voucher = vouchers.find(v => v.code === code);

    if (voucher) {
        if (subTotal < (voucher.minOrder || 0)) return 0;

        if (voucher.type === "percent") {
            return Math.round(subTotal * (voucher.value / 100));
        }

        if (voucher.type === "fixed") {
            return voucher.value;
        }
    }

    if (code === "SPORTIX10") {
        return Math.round(subTotal * 0.1);
    }

    if (/^0[0-9]{9}$/.test(code)) {
        return 30000;
    }

    return 0;
}

function updateTotal() {
    const subTotal = getSubTotal();

    shippingFee = getBaseShippingFee();
    discountAmount = getDiscountAmount(subTotal);

    let total = subTotal - discountAmount + shippingFee;
    if (total < 0) total = 0;

    document.getElementById("subTotal").textContent = formatMoney(subTotal);

    document.getElementById("shippingFee").textContent =
        shippingFee === 0 ? "Miễn phí" : formatMoney(shippingFee);

    const discountEl = document.getElementById("discountPrice");
    if (discountEl) {
        discountEl.textContent =
            discountAmount > 0 ? "-" + formatMoney(discountAmount) : "0đ";
    }

    document.getElementById("totalPrice").textContent = formatMoney(total);

    const payment = document.querySelector("input[name='payment']:checked");
    if (payment && payment.value === "bank") {
        showQrBox();
    }
}

function removeVietnameseTones(str) {
    return str
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/đ/g, "d")
        .replace(/Đ/g, "D")
        .toLowerCase();
}

function initLocationSelect() {
    const provinceInput = document.getElementById("province");
    const districtInput = document.getElementById("district");

    const provinceBox = document.getElementById("provinceSelect");
    const districtBox = document.getElementById("districtSelect");

    const provinceBtn = provinceBox.querySelector(".custom-select-btn span");
    const districtBtn = districtBox.querySelector(".custom-select-btn span");

    const provinceMenu = provinceBox.querySelector(".custom-select-menu");
    const districtMenu = districtBox.querySelector(".custom-select-menu");

    const provinces = window.vietnamData || [];

    function renderProvinceOptions() {
        provinceMenu.innerHTML = `
            <div class="select-search-box">
                <input type="text" id="provinceSearch" placeholder="Tìm tỉnh/thành phố...">
            </div>
            ${provinces.map(item => `
                <div class="custom-select-option" data-value="${item.name}">
                    ${item.name}
                </div>
            `).join("")}
        `;

        const provinceSearch = document.getElementById("provinceSearch");

        provinceSearch?.addEventListener("click", e => e.stopPropagation());
        provinceSearch?.addEventListener("keydown", e => e.stopPropagation());

        provinceSearch?.addEventListener("input", function () {
            const keyword = removeVietnameseTones(this.value.trim());

            provinceMenu.querySelectorAll(".custom-select-option").forEach(option => {
                const text = removeVietnameseTones(option.textContent.trim());
                option.style.display = text.includes(keyword) ? "flex" : "none";
            });
        });

        provinceMenu.querySelectorAll(".custom-select-option").forEach(option => {
            option.onclick = () => {
                const value = option.dataset.value;
                const selectedProvince = provinces.find(item => item.name === value);

                provinceInput.value = value;
                provinceBtn.textContent = value;
                provinceBox.classList.remove("open");

                districtInput.value = "";
                districtBtn.textContent = "Chọn phường/xã";

                renderDistrictOptions(selectedProvince?.districts || []);

                showSuccess(provinceInput, "provinceError");
                clearState(districtInput, "districtError");
                updateTotal();
            };
        });
    }

    function renderDistrictOptions(districts) {
        districtMenu.innerHTML = `
            <div class="select-search-box">
                <input type="text" id="districtSearch" placeholder="Tìm phường/xã...">
            </div>
            ${districts.map(item => {
                const name = typeof item === "string" ? item : item.name;

                return `
                    <div class="custom-select-option" data-value="${name}">
                        ${name}
                    </div>
                `;
            }).join("")}
        `;

        const districtSearch = document.getElementById("districtSearch");

        districtSearch?.addEventListener("click", e => e.stopPropagation());
        districtSearch?.addEventListener("keydown", e => e.stopPropagation());

        districtSearch?.addEventListener("input", function () {
            const keyword = removeVietnameseTones(this.value.trim());

            districtMenu.querySelectorAll(".custom-select-option").forEach(option => {
                const text = removeVietnameseTones(option.textContent.trim());
                option.style.display = text.includes(keyword) ? "flex" : "none";
            });
        });

        districtMenu.querySelectorAll(".custom-select-option").forEach(dis => {
            dis.onclick = () => {
                districtInput.value = dis.dataset.value;
                districtBtn.textContent = dis.dataset.value;
                districtBox.classList.remove("open");
                showSuccess(districtInput, "districtError");
            };
        });
    }

    renderProvinceOptions();

    provinceBox.querySelector(".custom-select-btn").onclick = () => {
        provinceBox.classList.toggle("open");
        districtBox.classList.remove("open");
    };

    districtBox.querySelector(".custom-select-btn").onclick = () => {
        if (!provinceInput.value) {
            showCheckoutToast("Chưa chọn tỉnh/thành phố", "Vui lòng chọn tỉnh/thành phố trước.");
            showError(provinceInput, "provinceError", "Vui lòng chọn tỉnh/thành phố trước.");
            return;
        }

        districtBox.classList.toggle("open");
        provinceBox.classList.remove("open");
    };

    document.addEventListener("click", e => {
        if (!provinceBox.contains(e.target)) provinceBox.classList.remove("open");
        if (!districtBox.contains(e.target)) districtBox.classList.remove("open");
    });
}

function initPayment() {
    document.querySelectorAll("input[name='payment']").forEach(input => {
        input.addEventListener("change", () => {
            const qrBox = document.getElementById("qrBox");

            if (input.value === "bank" && input.checked) {
                showQrBox();
            }

            if (input.value === "cod" && input.checked) {
                qrBox.style.display = "none";
                clearInterval(qrTimer);
            }
        });
    });
}

function getFinalTotal() {
    const subTotal = getSubTotal();
    let total = subTotal - discountAmount + shippingFee;
    return total < 0 ? 0 : total;
}

function showQrBox() {
    const total = getFinalTotal();
    const note = "SPORTIX" + Date.now().toString().slice(-6);

    document.getElementById("qrBox").style.display = "block";
    document.getElementById("transferNote").textContent = note;

    document.getElementById("qrImg").src =
        `https://img.vietqr.io/image/970422-0123456789-compact2.png?amount=${total}&addInfo=${note}&accountName=SPORTIX`;

    startTimer();
}

function startTimer() {
    clearInterval(qrTimer);
    qrSeconds = 300;
    updateTimerText();

    qrTimer = setInterval(() => {
        qrSeconds--;
        updateTimerText();

        if (qrSeconds <= 0) {
            clearInterval(qrTimer);
            document.getElementById("timer").textContent = "Hết hạn";
        }
    }, 1000);
}

function updateTimerText() {
    const minutes = Math.floor(qrSeconds / 60).toString().padStart(2, "0");
    const seconds = (qrSeconds % 60).toString().padStart(2, "0");
    document.getElementById("timer").textContent = `${minutes}:${seconds}`;
}

function initCheckoutValidation() {
    const nameInput = document.getElementById("name");
    const phoneInput = document.getElementById("phone");
    const addressInput = document.getElementById("address");
    const discountInput = document.getElementById("discountCode");

    nameInput.addEventListener("input", () => {
        if (!nameInput.value.trim()) return clearState(nameInput, "nameError");
        showSuccess(nameInput, "nameError");
    });

    phoneInput.addEventListener("input", () => {
        phoneInput.value = phoneInput.value.replace(/[^\d]/g, "");

        if (!phoneInput.value.trim()) return clearState(phoneInput, "phoneError");

        if (/^0[0-9]{9}$/.test(phoneInput.value.trim())) {
            showSuccess(phoneInput, "phoneError");
        } else {
            showError(phoneInput, "phoneError", "Số điện thoại phải có 10 số và bắt đầu bằng 0.");
        }
    });

    addressInput.addEventListener("input", () => {
        if (!addressInput.value.trim()) return clearState(addressInput, "addressError");
        showSuccess(addressInput, "addressError");
    });

    discountInput?.addEventListener("input", updateTotal);
}

function validateForm() {
    const nameInput = document.getElementById("name");
    const phoneInput = document.getElementById("phone");
    const provinceInput = document.getElementById("province");
    const districtInput = document.getElementById("district");
    const addressInput = document.getElementById("address");

    let isValid = true;

    if (!nameInput.value.trim()) {
        showError(nameInput, "nameError", "Vui lòng nhập họ tên.");
        isValid = false;
    } else showSuccess(nameInput, "nameError");

    if (!/^0[0-9]{9}$/.test(phoneInput.value.trim())) {
        showError(phoneInput, "phoneError", "Số điện thoại phải có 10 số và bắt đầu bằng 0.");
        isValid = false;
    } else showSuccess(phoneInput, "phoneError");

    if (!provinceInput.value) {
        showError(provinceInput, "provinceError", "Vui lòng chọn tỉnh/thành phố.");
        isValid = false;
    } else showSuccess(provinceInput, "provinceError");

    if (!districtInput.value) {
        showError(districtInput, "districtError", "Vui lòng chọn phường/xã.");
        isValid = false;
    } else showSuccess(districtInput, "districtError");

    if (!addressInput.value.trim()) {
        showError(addressInput, "addressError", "Vui lòng nhập địa chỉ giao hàng.");
        isValid = false;
    } else showSuccess(addressInput, "addressError");

    if (!isValid) {
        showCheckoutToast("Thông tin chưa hợp lệ", "Vui lòng kiểm tra lại các ô đang báo lỗi.");
        document.querySelector(".is-invalid")?.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    return isValid;
}

function goCart() {
    goWithSplash("cart.html");
}

function placeOrder() {
    if (!orderItems.length) {
        showCheckoutToast("Chưa có sản phẩm", "Không có sản phẩm nào để đặt hàng.");
        return;
    }

    if (!validateForm()) return;

    const account = getUserAccount();

    if (!account) {
        showCheckoutToast("Cần đăng nhập", "Vui lòng đăng nhập trước khi đặt hàng.");
        return;
    }

    const name = document.getElementById("name").value.trim();
    const phone = document.getElementById("phone").value.trim();
    const province = document.getElementById("province").value;
    const district = document.getElementById("district").value;
    const address = document.getElementById("address").value.trim();
    const payment = document.querySelector("input[name='payment']:checked").value;

    const subTotal = getSubTotal();
    const total = getFinalTotal();

    const order = {
        id: "SPX" + Date.now().toString().slice(-6),
        userAccount: account,
        date: new Date().toLocaleDateString("vi-VN"),
        customer: {
            name: name,
            phone: phone,
            address: `${address}, ${district}, ${province}`
        },
        payment: payment,
        paymentText: payment === "cod" ? "Thanh toán khi nhận hàng" : "Thanh toán QR",
        items: orderItems,
        subTotal: subTotal,
        discount: discountAmount,
        shippingFee: shippingFee,
        total: total,
        status: "Đang giao"
    };

    const orders = safeJSON(ORDERS_KEY, []);
    orders.push(order);

    localStorage.setItem(ORDERS_KEY, JSON.stringify(orders));
    localStorage.setItem("sportix_last_order", JSON.stringify(order));

    localStorage.removeItem(CHECKOUT_KEY);
    localStorage.removeItem(CART_KEY);

    goWithSplash("success.html");
}

function goCheckoutCategory(category) {
    sessionStorage.setItem("sportix_home_category", category);
    goWithSplash("product-list.html");
}

document.addEventListener("DOMContentLoaded", () => {
    loadCheckoutItems();
    renderOrderList();
    initLocationSelect();
    initPayment();
    initCheckoutValidation();
    updateTotal();
});