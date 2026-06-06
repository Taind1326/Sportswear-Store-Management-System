const $ = (id) => document.getElementById(id);

const products = [
  {
    id: 'SP001',
    name: 'Giày chạy bộ Nike Air Zoom',
    category: 'Giày',
    price: 2490000,
    stock: 12,
    sold: 128,
    image: '../img/shoe-1.jpg'
  },
  {
    id: 'SP002',
    name: 'Áo thun Adidas Training',
    category: 'Áo',
    price: 690000,
    stock: 54,
    sold: 96,
    image: '../img/shirt-1.jpg'
  },
  {
    id: 'SP003',
    name: 'Balo thể thao SportZone',
    category: 'Phụ kiện',
    price: 450000,
    stock: 8,
    sold: 74,
    image: '../img/bag-1.jpg'
  },
  {
    id: 'SP004',
    name: 'Bình nước tập gym Active',
    category: 'Phụ kiện',
    price: 150000,
    stock: 5,
    sold: 61,
    image: '../img/bottle-1.jpg'
  },
  {
    id: 'SP005',
    name: 'Áo khoác thể thao Runner',
    category: 'Áo',
    price: 890000,
    stock: 18,
    sold: 52,
    image: '../img/jacket-1.jpg'
  }
];

const orders = [
  {
    id: '#SZ1024',
    customer: 'Nguyễn Hải Yến',
    product: 'Giày chạy bộ Nike Air Zoom',
    total: 2490000,
    payment: 'Momo',
    status: 'Đang giao'
  },
  {
    id: '#SZ1023',
    customer: 'Trần Minh Anh',
    product: 'Áo thun Adidas Training',
    total: 690000,
    payment: 'COD',
    status: 'Chờ xử lý'
  },
  {
    id: '#SZ1022',
    customer: 'Lê Hoàng Nam',
    product: 'Balo thể thao SportZone',
    total: 450000,
    payment: 'Banking',
    status: 'Đã giao'
  },
  {
    id: '#SZ1021',
    customer: 'Phạm Gia Hân',
    product: 'Bình nước tập gym Active',
    total: 150000,
    payment: 'COD',
    status: 'Đã hủy'
  }
];

const sales = [
  { day: 'T2', value: 12.5 },
  { day: 'T3', value: 18.2 },
  { day: 'T4', value: 10.8 },
  { day: 'T5', value: 24.6 },
  { day: 'T6', value: 20.1 },
  { day: 'T7', value: 0 },
  { day: 'CN', value: 0 }
];

function formatMoney(value) {
  return value.toLocaleString('vi-VN') + 'đ';
}

function getProductStatus(stock) {
  if (stock <= 5) return { text: 'Sắp hết', className: 'cancel' };
  if (stock <= 12) return { text: 'Cần nhập', className: 'pending' };
  return { text: 'Đang bán', className: 'done' };
}

function getOrderStatusClass(status) {
  if (status === 'Đã giao') return 'done';
  if (status === 'Đang giao') return 'shipping';
  if (status === 'Chờ xử lý') return 'pending';
  return 'cancel';
}

function renderStats() {
  const totalProducts = products.length;
  const todayOrders = orders.length;
  const weekRevenue = orders.reduce((sum, item) => sum + item.total, 0);
  const lowStock = products.filter(item => item.stock <= 12).length;

  $('statsGrid').innerHTML = `
    <div class="stat-card">
      <div class="stat-icon blue"><i class="bi bi-cash-stack"></i></div>
      <div>
        <p>Doanh thu tuần</p>
        <h3>${(weekRevenue / 1000000).toFixed(1)}M</h3>
        <span>+12.4% tuần này</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon yellow"><i class="bi bi-bag-check-fill"></i></div>
      <div>
        <p>Đơn hôm nay</p>
        <h3>${todayOrders}</h3>
        <span>Đang cập nhật</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon green"><i class="bi bi-box-seam-fill"></i></div>
      <div>
        <p>Sản phẩm</p>
        <h3>${totalProducts}</h3>
        <span>Đang kinh doanh</span>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon red"><i class="bi bi-exclamation-triangle-fill"></i></div>
      <div>
        <p>Sắp hết hàng</p>
        <h3>${lowStock}</h3>
        <span class="danger">Cần nhập thêm</span>
      </div>
    </div>
  `;
}

function renderSalesChart() {
  const maxValue = Math.max(...sales.map(item => item.value));

  $('salesChart').innerHTML = sales.map(item => {
    const height = item.value === 0 ? 8 : Math.max((item.value / maxValue) * 100, 18);
    const todayClass = item.day === 'T5' ? 'today' : '';

    return `
      <div class="chart-bar ${todayClass}" style="height:${height}%">
        <strong>${item.value === 0 ? '-' : item.value + 'M'}</strong>
        <span>${item.day}</span>
      </div>
    `;
  }).join('');
}

function renderBestProducts() {
  const bestProducts = [...products]
    .sort((a, b) => b.sold - a.sold)
    .slice(0, 4);

  $('bestList').innerHTML = bestProducts.map(item => `
    <div class="best-item">
      <img src="${item.image}" class="product-thumb" alt="${item.name}">
      <div class="item-info">
        <h4>${item.name}</h4>
        <p>Đã bán ${item.sold} sản phẩm</p>
      </div>
    </div>
  `).join('');
}

function renderLowStock() {
  const lowStockProducts = products.filter(item => item.stock <= 12);

  $('lowStockList').innerHTML = lowStockProducts.map(item => `
    <div class="low-stock-item">
      <img src="${item.image}" class="product-thumb" alt="${item.name}">
      <div class="item-info">
        <h4>${item.name}</h4>
        <p>Còn ${item.stock} sản phẩm trong kho</p>
      </div>
    </div>
  `).join('');
}

function renderProducts() {
  const keyword = $('searchInput').value.trim().toLowerCase();
  const category = $('categoryFilter').value;

  const filteredProducts = products.filter(item => {
    const matchKeyword =
      item.name.toLowerCase().includes(keyword) ||
      item.id.toLowerCase().includes(keyword);

    const matchCategory = category === 'all' || item.category === category;

    return matchKeyword && matchCategory;
  });

  $('productTable').innerHTML = filteredProducts.map(item => {
    const status = getProductStatus(item.stock);

    return `
      <tr>
        <td>
          <div class="product-cell">
            <img src="${item.image}" class="product-thumb" alt="${item.name}">
            <div>
              <strong>${item.name}</strong>
              <span>${item.id}</span>
            </div>
          </div>
        </td>
        <td>${item.category}</td>
        <td>${formatMoney(item.price)}</td>
        <td>${item.stock}</td>
        <td><span class="badge-status ${status.className}">${status.text}</span></td>
        <td><button class="action-btn"><i class="bi bi-three-dots"></i></button></td>
      </tr>
    `;
  }).join('');
}

function renderOrders() {
  $('orderTable').innerHTML = orders.map(item => `
    <tr>
      <td>${item.id}</td>
      <td>${item.customer}</td>
      <td>${item.product}</td>
      <td>${formatMoney(item.total)}</td>
      <td>${item.payment}</td>
      <td><span class="badge-status ${getOrderStatusClass(item.status)}">${item.status}</span></td>
    </tr>
  `).join('');
}

function initEvents() {
  $('searchInput').addEventListener('input', renderProducts);
  $('categoryFilter').addEventListener('change', renderProducts);

  $('btnMenu').addEventListener('click', () => {
    $('sidebar').classList.toggle('show');
  });
}

function initAdminPage() {
  renderStats();
  renderSalesChart();
  renderBestProducts();
  renderLowStock();
  renderProducts();
  renderOrders();
  initEvents();
}

initAdminPage();