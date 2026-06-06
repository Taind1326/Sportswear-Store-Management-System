// ======================
// HELPERS
// ======================
const $ = (id) => document.getElementById(id);

function formatCurrency(value) {
  return value.toLocaleString('vi-VN') + 'đ';
}

function formatMillion(value) {
  return (value / 1000000).toFixed(1) + 'M';
}

function getTodayIndex() {
  const day = new Date().getDay();

  if (day === 0) return 6;

  return day - 1;
}

function getTodayDate() {
  return new Date().toISOString().slice(0, 10);
}

function getVisibleSalesData() {
  return salesData.slice(0, getTodayIndex() + 1);
}

// ======================
// DATA
// ======================
const products = [
  {
    code: 'SP001',
    name: 'Giày chạy bộ Nike Air Zoom',
    category: 'Giày',
    price: 2450000,
    stock: 32,
    sold: 128,
    image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=200'
  },
  {
    code: 'SP002',
    name: 'Áo thun Adidas Training',
    category: 'Áo',
    price: 690000,
    stock: 54,
    sold: 96,
    image: 'https://images.unsplash.com/photo-1523381294911-8d3cead13475?w=200'
  },
  {
    code: 'SP003',
    name: 'Balo thể thao SportZone',
    category: 'Phụ kiện',
    price: 850000,
    stock: 8,
    sold: 74,
    image: 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=200'
  },
  {
    code: 'SP004',
    name: 'Bình nước tập gym',
    category: 'Phụ kiện',
    price: 220000,
    stock: 6,
    sold: 63,
    image: 'https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=200'
  },
  {
    code: 'SP005',
    name: 'Quần short thể thao nam',
    category: 'Áo',
    price: 420000,
    stock: 26,
    sold: 58,
    image: 'https://images.unsplash.com/photo-1506629905607-d9d297d3f5f5?w=200'
  }
];

const orders = [
  {
    id: '#SZ1024',
    customer: 'Nguyễn Hải Yến',
    product: 'Giày chạy bộ Nike Air Zoom',
    total: 2450000,
    status: 'done',
    text: 'Hoàn thành',
    date: getTodayDate()
  },
  {
    id: '#SZ1023',
    customer: 'Lê Minh Chiến',
    product: 'Áo thun Adidas Training',
    total: 690000,
    status: 'pending',
    text: 'Đang xử lý',
    date: getTodayDate()
  },
  {
    id: '#SZ1022',
    customer: 'Trần Gia Bảo',
    product: 'Balo thể thao SportZone',
    total: 850000,
    status: 'done',
    text: 'Hoàn thành',
    date: '2026-06-04'
  },
  {
    id: '#SZ1021',
    customer: 'Phạm Ngọc Anh',
    product: 'Bình nước tập gym',
    total: 220000,
    status: 'cancel',
    text: 'Đã hủy',
    date: '2026-06-03'
  }
];

const salesData = [
  { day: 'T2', value: 12500000 },
  { day: 'T3', value: 18200000 },
  { day: 'T4', value: 10800000 },
  { day: 'T5', value: 24600000 },
  { day: 'T6', value: 20100000 },
  { day: 'T7', value: 28400000 },
  { day: 'CN', value: 16700000 }
];

// ======================
// STATS
// ======================
function renderStats() {
  const visibleSales = getVisibleSalesData();

  const totalRevenue = visibleSales.reduce((sum, item) => {
    return sum + item.value;
  }, 0);

  const todayOrders = orders.filter(order => {
    return order.date === getTodayDate();
  }).length;

  const lowStock = products.filter(product => {
    return product.stock <= 10;
  }).length;

  const customerCount = 1284;

  const stats = [
    {
      title: 'Doanh thu tuần',
      value: formatMillion(totalRevenue),
      note: '+12.4% so với tuần trước',
      noteClass: 'up',
      icon: 'bi-cash-stack',
      color: 'blue'
    },
    {
      title: 'Đơn hôm nay',
      value: todayOrders,
      note: '+18 đơn đang xử lý',
      noteClass: 'up',
      icon: 'bi-receipt',
      color: 'green'
    },
    {
      title: 'Khách hàng',
      value: customerCount,
      note: '+35 khách mới',
      noteClass: 'up',
      icon: 'bi-people',
      color: 'orange'
    },
    {
      title: 'Sắp hết hàng',
      value: lowStock,
      note: 'Cần nhập thêm',
      noteClass: 'down',
      icon: 'bi-exclamation-triangle',
      color: 'red'
    }
  ];

  $('statsGrid').innerHTML = stats.map(item => `
    <div class="stat-card">
      <div class="stat-icon ${item.color}">
        <i class="bi ${item.icon}"></i>
      </div>

      <div>
        <p>${item.title}</p>
        <h3>${item.value}</h3>
        <span class="${item.noteClass}">${item.note}</span>
      </div>
    </div>
  `).join('');
}

// ======================
// PRODUCTS
// ======================
function getProductStatus(product) {
  if (product.stock <= 10) {
    return {
      className: 'low',
      text: 'Sắp hết'
    };
  }

  return {
    className: 'active',
    text: 'Đang bán'
  };
}

function renderProducts(list = products) {
  $('productTable').innerHTML = list.map(product => {
    const status = getProductStatus(product);

    return `
      <tr>
        <td>
          <div class="product-info">
            <img class="product-img" src="${product.image}" alt="${product.name}">
            <div>
              <span class="product-name">${product.name}</span>
              <span class="product-code">${product.code}</span>
            </div>
          </div>
        </td>

        <td>${product.category}</td>

        <td>${formatCurrency(product.price)}</td>

        <td>${product.stock}</td>

        <td>
          <span class="badge-status ${status.className}">
            ${status.text}
          </span>
        </td>

        <td>
          <button class="btn btn-sm btn-light">
            <i class="bi bi-three-dots"></i>
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

// ======================
// BEST SELLER
// ======================
function renderBestSeller() {
  const bestProducts = [...products]
    .sort((a, b) => b.sold - a.sold)
    .slice(0, 3);

  $('bestList').innerHTML = bestProducts.map((product, index) => `
    <div class="best-item">
      <div class="best-rank">${index + 1}</div>

      <img src="${product.image}" alt="${product.name}">

      <div>
        <h4>${product.name}</h4>
        <p>Đã bán ${product.sold} sản phẩm</p>
      </div>
    </div>
  `).join('');
}

// ======================
// ORDERS
// ======================
function renderOrders() {
  $('orderTable').innerHTML = orders.map(order => `
    <tr>
      <td>${order.id}</td>
      <td>${order.customer}</td>
      <td>${order.product}</td>
      <td>${formatCurrency(order.total)}</td>
      <td>${order.date}</td>
      <td>
        <span class="badge-status ${order.status}">
          ${order.text}
        </span>
      </td>
    </tr>
  `).join('');
}

// ======================
// CHART
// ======================
function renderSalesChart() {
  const data = getVisibleSalesData();
  const maxValue = Math.max(...data.map(item => item.value));
  const todayIndex = data.length - 1;

  $('salesChart').innerHTML = data.map((item, index) => {
    const height = maxValue === 0 ? 10 : (item.value / maxValue) * 100;
    const todayClass = index === todayIndex ? 'today' : '';

    return `
      <div
        class="chart-bar ${todayClass}"
        style="height:${height}%"
        data-value="${formatMillion(item.value)}"
      >
        <span>${item.day}</span>
      </div>
    `;
  }).join('');
}

// ======================
// FILTER
// ======================
function initFilter() {
  const dropdown = $('categoryDropdown');
  const categoryText = $('categoryText');
  const searchInput = $('searchInput');

  let currentCategory = 'all';

  function applyFilter() {
    const keyword = searchInput.value.trim().toLowerCase();

    const filteredProducts = products.filter(product => {
      const matchCategory =
        currentCategory === 'all' ||
        product.category === currentCategory;

      const matchKeyword =
        product.name.toLowerCase().includes(keyword) ||
        product.code.toLowerCase().includes(keyword);

      return matchCategory && matchKeyword;
    });

    renderProducts(filteredProducts);
  }

  dropdown.querySelector('.custom-select-btn').addEventListener('click', () => {
    dropdown.classList.toggle('open');
  });

  dropdown.querySelectorAll('.custom-select-menu button').forEach(button => {
    button.addEventListener('click', () => {
      currentCategory = button.dataset.value;
      categoryText.textContent = button.textContent.trim();

      dropdown.querySelectorAll('.custom-select-menu button').forEach(item => {
        item.classList.remove('active');
      });

      button.classList.add('active');
      dropdown.classList.remove('open');

      applyFilter();
    });
  });

  document.addEventListener('click', (event) => {
    if (!dropdown.contains(event.target)) {
      dropdown.classList.remove('open');
    }
  });

  searchInput.addEventListener('input', applyFilter);
}

// ======================
// SIDEBAR
// ======================
function initSidebar() {
  $('btnMenu').addEventListener('click', () => {
    $('sidebar').classList.toggle('show');
  });
}

// ======================
// INIT
// ======================
function init() {
  renderStats();
  renderProducts();
  renderBestSeller();
  renderOrders();
  renderSalesChart();
  initFilter();
  initSidebar();
}

document.addEventListener('DOMContentLoaded', init);