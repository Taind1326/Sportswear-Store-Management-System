// =====================
// HELPERS
// =====================
const $ = (id) => document.getElementById(id);

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const demoUsers = [
  { email: "demo@sportix.vn", password: "Sport@123" },
  { email: "admin@sportix.vn", password: "Admin@123" },
  { email: "user1@gmail.com", password: "123456" },
  { email: "user2@gmail.com", password: "123456" },
  { email: "user3@gmail.com", password: "123456" },
  { email: "user4@gmail.com", password: "123456" },
  { email: "user5@gmail.com", password: "123456" },
  { email: "user6@gmail.com", password: "123456" },
  { email: "user7@gmail.com", password: "123456" },
  { email: "user8@gmail.com", password: "123456" }
];

// =====================
// ELEMENTS
// =====================
const loginForm = $("loginForm");
const emailInput = $("email");
const passwordInput = $("password");

const emailError = $("emailError");
const passwordError = $("passwordError");

const togglePasswordButton = $("togglePw");
const eyeIcon = $("eyeIcon");

const loginButton = $("loginBtn");
const alertBox = $("alertBox");
const alertMsg = $("alertMsg");
const successOverlay = $("successOverlay");

// =====================
// UI STATE
// =====================
function showError(inputElement, errorElement, message) {
  inputElement.classList.add("is-invalid");
  inputElement.classList.remove("is-valid");

  const messageElement = errorElement.querySelector("span");

  if (messageElement) {
    messageElement.textContent = message;
  }

  errorElement.classList.add("show");
}

function showSuccess(inputElement, errorElement) {
  inputElement.classList.remove("is-invalid");
  inputElement.classList.add("is-valid");
  errorElement.classList.remove("show");
}

function clearState(inputElement, errorElement) {
  inputElement.classList.remove("is-invalid", "is-valid");
  errorElement.classList.remove("show");
}

function setLoading(isLoading) {
  loginButton.classList.toggle("loading", isLoading);
  loginButton.disabled = isLoading;
}

function hideAlert() {
  alertBox.classList.add("d-none");
  alertBox.classList.remove("show", "d-flex");
}

function showAlert(message) {
  alertMsg.textContent = message;
  alertBox.classList.remove("d-none");
  alertBox.classList.add("show", "d-flex");
}

function showSuccessOverlay() {
  localStorage.setItem("sportix_user", JSON.stringify({
    email: emailInput.value.trim()
  }));

  successOverlay.classList.add("show");

  setTimeout(() => {
    const redirect = sessionStorage.getItem("redirectAfterLogin") || "index.html";
    sessionStorage.removeItem("redirectAfterLogin");
    window.location.href = redirect;
  }, 1500);
}

// =====================
// VALIDATION
// =====================
function validateEmail() {
  const email = emailInput.value.trim();

  if (!email) {
    showError(emailInput, emailError, "Vui lòng nhập email của bạn.");
    return false;
  }

  if (!emailRegex.test(email)) {
    showError(emailInput, emailError, "Email không đúng định dạng. Ví dụ: ten@gmail.com");
    return false;
  }

  showSuccess(emailInput, emailError);
  return true;
}

function validatePassword() {
  const password = passwordInput.value;

  if (!password) {
    showError(passwordInput, passwordError, "Vui lòng nhập mật khẩu.");
    return false;
  }

  if (password.length < 6) {
    showError(passwordInput, passwordError, "Mật khẩu phải có ít nhất 6 ký tự.");
    return false;
  }

  showSuccess(passwordInput, passwordError);
  return true;
}

function clearEmailWhenEmpty() {
  if (!emailInput.value.trim()) {
    clearState(emailInput, emailError);
  }
}

function clearPasswordWhenEmpty() {
  if (!passwordInput.value) {
    clearState(passwordInput, passwordError);
  }
}

function findUser(email, password) {
  const emailLower = email.toLowerCase();

  return demoUsers.find(
    (user) => user.email.toLowerCase() === emailLower && user.password === password
  );
}

// =====================
// EVENTS
// =====================
emailInput.addEventListener("input", () => {
  hideAlert();

  if (!emailInput.value.trim()) {
    clearEmailWhenEmpty();
    return;
  }

  validateEmail();
});

passwordInput.addEventListener("input", () => {
  hideAlert();

  if (!passwordInput.value) {
    clearPasswordWhenEmpty();
    return;
  }

  validatePassword();
});

togglePasswordButton.addEventListener("click", () => {
  const isPasswordHidden = passwordInput.type === "password";

  passwordInput.type = isPasswordHidden ? "text" : "password";
  eyeIcon.className = isPasswordHidden ? "bi bi-eye-slash" : "bi bi-eye";
});

loginForm.addEventListener("submit", (event) => {
  event.preventDefault();
  hideAlert();

  const isEmailValid = validateEmail();
  const isPasswordValid = validatePassword();

  if (!isEmailValid || !isPasswordValid) {
    return;
  }

  const email = emailInput.value.trim();
  const password = passwordInput.value;

  setLoading(true);

  setTimeout(() => {
    setLoading(false);

    const user = findUser(email, password);

    if (user) {
      showSuccessOverlay();
      return;
    }

    showAlert("Email hoặc mật khẩu không chính xác. Thử: demo@sportix.vn / Sport@123 hoặc user1@gmail.com / 123456");

    emailInput.classList.add("is-invalid");
    passwordInput.classList.add("is-invalid");
    passwordInput.classList.remove("is-valid");
    passwordInput.value = "";
  }, 1200);
});