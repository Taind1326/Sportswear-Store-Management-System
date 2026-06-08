document.addEventListener("DOMContentLoaded", () => {
  document.body.classList.add("reveal-ready");
  initScrollReveal();

  const isAdminPage = document.querySelector(".admin-layout");

  if (!isAdminPage) {
    updateAuthNavbar();
    showSaleModal();
  }
});

function initScrollReveal() {
  const revealItems = document.querySelectorAll(
    ".reveal, .reveal-left, .reveal-right, .reveal-scale"
  );

  if (!revealItems.length) return;

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (!entry.isIntersecting) return;

        entry.target.classList.add("show");
        observer.unobserve(entry.target);
      });
    },
    {
      threshold: 0.12,
      rootMargin: "0px 0px -40px 0px"
    }
  );

  revealItems.forEach((item) => observer.observe(item));
}

function goWithSplash(url) {
  sessionStorage.setItem("nextPage", url);
  window.location.href = "splash.html";
}

/* =====================
   USER NAVBAR
===================== */
function updateAuthNavbar() {
  const authBtn = document.querySelector(".sz-nav-btn");
  const user = localStorage.getItem("sportix_user");

  if (!authBtn) return;

  const navItem = authBtn.closest(".nav-item");
  const currentPage = window.location.pathname.split("/").pop();

  navItem.classList.remove("user-nav", "open");

  const oldDropdown = navItem.querySelector(".user-dropdown");
  if (oldDropdown) oldDropdown.remove();

  if (!user) {
    if (currentPage === "login.html") {
      authBtn.innerHTML = "Đăng ký";
      authBtn.onclick = function () {
        goWithSplash("register.html");
        return false;
      };
    } else {
      authBtn.innerHTML = "Đăng nhập";
      authBtn.onclick = function () {
        goWithSplash("login.html");
        return false;
      };
    }

    return;
  }

  let data = {};

  try {
    data = JSON.parse(user);
  } catch {
    data = {};
  }

  const name = data.name || data.email || "Khách hàng";

  navItem.classList.add("user-nav");

  authBtn.innerHTML = `
    <i class="bi bi-person-check-fill"></i>
    Xin chào, ${name}
    <i class="bi bi-chevron-down ms-1"></i>
  `;

  navItem.insertAdjacentHTML("beforeend", `
    <div class="user-dropdown">
      <button type="button" onclick="showLogoutModal()">
        <i class="bi bi-box-arrow-right"></i>
        Đăng xuất
      </button>
    </div>
  `);

  authBtn.onclick = function (e) {
    e.preventDefault();
    navItem.classList.toggle("open");
    return false;
  };

  document.addEventListener("click", function (e) {
    if (!navItem.contains(e.target)) {
      navItem.classList.remove("open");
    }
  });
}

/* =====================
   LOGOUT MODAL
===================== */
function showLogoutModal() {
  document.querySelector(".user-nav")?.classList.remove("open");

  if (!document.getElementById("logoutModal")) {
    document.body.insertAdjacentHTML("beforeend", `
      <div class="logout-modal" id="logoutModal">
        <div class="logout-box">
          <i class="bi bi-box-arrow-right"></i>
          <h3>Đăng xuất?</h3>
          <p>Bạn có chắc chắn muốn đăng xuất khỏi SPORTIX không?</p>

          <div class="logout-actions">
            <button class="btn-cancel-logout" onclick="hideLogoutModal()">Hủy</button>
            <button class="btn-confirm-logout" onclick="logoutUser()">Đăng xuất</button>
          </div>
        </div>
      </div>
    `);
  }

  document.getElementById("logoutModal").classList.add("show");
}

function hideLogoutModal() {
  document.getElementById("logoutModal")?.classList.remove("show");
}

function logoutUser() {
  localStorage.removeItem("sportix_user");
  hideLogoutModal();
  location.reload();
}

/* =====================
   SALE MODAL
===================== */
function showSaleModal() {
  const modal = document.getElementById("saleModal");

  if (!modal) return;

  const skipSale = sessionStorage.getItem("skipSaleOnProduct");

  if (skipSale === "true") {
    sessionStorage.removeItem("skipSaleOnProduct");
    return;
  }

  setTimeout(() => {
    modal.classList.add("show");
  }, 1200);
}

function closeSaleModal() {
  document.getElementById("saleModal")?.classList.remove("show");
}

function goSaleProduct() {
  sessionStorage.setItem("skipSaleOnProduct", "true");
  goWithSplash("product-list.html");
}