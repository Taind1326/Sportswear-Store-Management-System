const CHECKOUT_KEY = "sportix_checkout";

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
    const cartData = localStorage.getItem("sportix_cart");

    if (checkoutData) {
        orderItems = JSON.parse(checkoutData);
        return;
    }

    if (cartData) {
        const cartItems = JSON.parse(cartData);
        orderItems = cartItems.filter(item => item.selected);
        localStorage.setItem(CHECKOUT_KEY, JSON.stringify(orderItems));
        return;
    }

    orderItems = [];
}

function renderOrderList() {
    const orderList = document.getElementById("orderList");

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
          <div class="order-qty">Số lượng: ${item.qty}</div>
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

    // 1. Đọc và kiểm tra mã giảm giá được lưu từ trang Admin
    const vouchersData = localStorage.getItem("sportix_vouchers");
    if (vouchersData) {
        const vouchers = JSON.parse(vouchersData);
        // Tìm mã tương ứng trong danh sách
        const voucher = vouchers.find(v => v.code === code);

        if (voucher) {
            // Kiểm tra xem tổng giá trị đơn hàng có đạt mức tối thiểu (minOrder) không
            if (subTotal >= (voucher.minOrder || 0)) {
                if (voucher.type === "percent") {
                    // Giảm giá theo phần trăm (%)
                    return Math.round(subTotal * (voucher.value / 100));
                } else if (voucher.type === "fixed") {
                    // Giảm giá theo số tiền cố định (đ)
                    return voucher.value;
                }
            } else {
                // Đã tìm thấy mã nhưng chưa đủ điều kiện giá trị đơn hàng tối thiểu
                return 0;
            }
        }
    }

    // 2. Giữ lại logic dự phòng cũ (nếu cần)
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
        discountEl.textContent = discountAmount > 0 ? "-" + formatMoney(discountAmount) : "0đ";
    }

    document.getElementById("totalPrice").textContent = formatMoney(total);

    const payment = document.querySelector("input[name='payment']:checked");
    if (payment && payment.value === "bank") {
        showQrBox();
    }
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

    provinceMenu.innerHTML = provinces.map(item =>
        `<div class="custom-select-option" data-value="${item.name}">
      ${item.name}
   </div>`
    ).join("");

    provinceBox.querySelector(".custom-select-btn").onclick = () => {
        provinceBox.classList.toggle("open");
        districtBox.classList.remove("open");
    };

    districtBox.querySelector(".custom-select-btn").onclick = () => {
        districtBox.classList.toggle("open");
        provinceBox.classList.remove("open");
    };

    provinceMenu.querySelectorAll(".custom-select-option").forEach(option => {
        option.onclick = () => {

            const value = option.dataset.value;
            const selectedProvince = provinces.find(item => item.name === value);

            provinceInput.value = value;
            provinceBtn.textContent = value;
            provinceBox.classList.remove("open");

            districtInput.value = "";
            districtBtn.textContent = "Chọn phường/xã";

            districtMenu.innerHTML = selectedProvince.districts.map(item =>
                `<div class="custom-select-option" data-value="${item.name}">
          ${item.name}
       </div>`
            ).join("");

            districtMenu.querySelectorAll(".custom-select-option").forEach(dis => {
                dis.onclick = () => {
                    districtInput.value = dis.dataset.value;
                    districtBtn.textContent = dis.dataset.value;
                    districtBox.classList.remove("open");
                    showSuccess(districtInput, "districtError");
                };
            });

            showSuccess(provinceInput, "provinceError");
            clearState(districtInput, "districtError");
            updateTotal();
        };
    });

    document.addEventListener("click", e => {
        if (!provinceBox.contains(e.target)) {
            provinceBox.classList.remove("open");
        }

        if (!districtBox.contains(e.target)) {
            districtBox.classList.remove("open");
        }
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
        showError(districtInput, "districtError", "Vui lòng chọn quận/huyện.");
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
        total: total
    };

    localStorage.setItem("sportix_last_order", JSON.stringify(order));

    localStorage.removeItem(CHECKOUT_KEY);
    localStorage.removeItem("sportix_cart");

    goWithSplash("success.html");
}

document.addEventListener("DOMContentLoaded", () => {
    loadCheckoutItems();
    renderOrderList();
    initLocationSelect();
    initPayment();
    initCheckoutValidation();
    updateTotal();
});

function goCheckoutCategory(category) {
    sessionStorage.setItem("sportix_home_category", category);
    goWithSplash("product-list.html");
}