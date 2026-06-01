const $ = (id) => document.getElementById(id);

let products = [
    { id: 1, name: "Nike Air Zoom Pegasus 40", category: "Giày", price: 3200000, stock: 20 },
    { id: 2, name: "Adidas Ultraboost 23", category: "Giày", price: 4500000, stock: 12 },
    { id: 3, name: "Áo Nike Pro Compression", category: "Áo quần", price: 850000, stock: 35 },
    { id: 4, name: "Bóng đá Nike Premier League", category: "Bóng đá", price: 950000, stock: 18 }
];

let orders = [
    { id: "DH001", customer: "Nguyễn Văn A", date: "05/05/2026", total: 4050000, status: "Hoàn thành" },
    { id: "DH002", customer: "Trần Thị B", date: "04/05/2026", total: 950000, status: "Đang giao" },
    { id: "DH003", customer: "Lê Minh C", date: "03/05/2026", total: 1800000, status: "Chờ xử lý" }
];

let customers = [
    { id: "KH001", name: "Nguyễn Văn A", email: "vana@gmail.com", phone: "0987654321", orders: 3 },
    { id: "KH002", name: "Trần Thị B", email: "thib@gmail.com", phone: "0912345678", orders: 1 },
    { id: "KH003", name: "Lê Minh C", email: "minhc@gmail.com", phone: "0909123456", orders: 2 }
];

let categories = [
    { id: "DM01", name: "Giày", count: 2 },
    { id: "DM02", name: "Áo quần", count: 1 },
    { id: "DM03", name: "Bóng đá", count: 1 }
];

function formatMoney(number) {
    return number.toLocaleString("vi-VN") + "đ";
}

/* CHUYỂN TAB SIDEBAR */
document.querySelectorAll(".menu-item").forEach(button => {
    button.addEventListener("click", () => {
        document.querySelectorAll(".menu-item").forEach(btn => btn.classList.remove("active"));
        button.classList.add("active");

        const page = button.dataset.page;

        document.querySelectorAll(".content-page").forEach(section => {
            section.classList.remove("active");
        });

        $(page).classList.add("active");
        $("pageTitle").textContent = button.textContent.trim();
    });
});

/* RENDER DASHBOARD */
function renderDashboard() {
    $("totalProducts").textContent = products.length;
    $("totalOrders").textContent = orders.length;
    $("totalCustomers").textContent = customers.length;

    const revenue = orders.reduce((sum, item) => sum + item.total, 0);
    $("totalRevenue").textContent = formatMoney(revenue);
}

/* RENDER PRODUCTS */
function renderProducts() {
    $("productTable").innerHTML = products.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.name}</td>
      <td>${item.category}</td>
      <td>${formatMoney(item.price)}</td>
      <td>${item.stock}</td>
      <td>
        <button class="btn btn-sm btn-outline-warning">Sửa</button>
        <button class="btn btn-sm btn-outline-danger">Xóa</button>
      </td>
    </tr>
  `).join("");
}

/* RENDER ORDERS */
function renderOrders() {
    $("orderTable").innerHTML = orders.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.customer}</td>
      <td>${item.date}</td>
      <td>${formatMoney(item.total)}</td>
      <td><span class="badge bg-warning text-dark">${item.status}</span></td>
    </tr>
  `).join("");
}

/* RENDER CUSTOMERS */
function renderCustomers() {
    $("customerTable").innerHTML = customers.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.name}</td>
      <td>${item.email}</td>
      <td>${item.phone}</td>
      <td>${item.orders}</td>
    </tr>
  `).join("");
}

/* RENDER CATEGORIES */
function renderCategories() {
    $("categoryTable").innerHTML = categories.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.name}</td>
      <td>${item.count}</td>
    </tr>
  `).join("");
}

/* KHỞI CHẠY */
renderDashboard();
renderProducts();
renderOrders();
renderCustomers();
renderCategories();