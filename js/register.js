document.addEventListener("DOMContentLoaded", () => {
  const $ = (id) => document.getElementById(id);

  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const phoneRegex = /^0[0-9]{9}$/;

  let currentStep = 1;

  function showError(input, errorEl, msg) {
    if (!input || !errorEl) return;

    input.classList.add("is-invalid");
    input.classList.remove("is-valid");

    const span = errorEl.querySelector("span");
    if (span && msg) span.textContent = msg;

    errorEl.classList.add("show");
  }

  function showSuccess(input, errorEl) {
    if (!input || !errorEl) return;

    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    errorEl.classList.remove("show");
  }

  function clearState(input, errorEl) {
    if (!input || !errorEl) return;

    input.classList.remove("is-invalid", "is-valid");
    errorEl.classList.remove("show");
  }

  function goToStep(stepNumber) {
    for (let i = 1; i <= 3; i++) {
      const step = $("step" + i);
      const dot = $("dot" + i);

      if (step) step.style.display = i === stepNumber ? "block" : "none";

      if (dot) {
        dot.classList.remove("active", "done");

        if (i < stepNumber) {
          dot.classList.add("done");
          dot.innerHTML = `<i class="bi bi-check-lg" style="font-size:0.75rem;"></i>`;
        } else if (i === stepNumber) {
          dot.classList.add("active");
          dot.textContent = i;
        } else {
          dot.textContent = i;
        }
      }
    }

    $("line1")?.classList.toggle("done", stepNumber > 1);
    $("line2")?.classList.toggle("done", stepNumber > 2);

    currentStep = stepNumber;
  }

  function validateName(input, errorEl, message) {
    const value = input.value.trim();

    if (!value) {
      showError(input, errorEl, message);
      return false;
    }

    showSuccess(input, errorEl);
    return true;
  }

  function validateEmail() {
    const emailInput = $("email");
    const email = emailInput.value.trim();

    if (!email) {
      showError(emailInput, $("emailError"), "Vui lòng nhập email.");
      return false;
    }

    if (!emailRegex.test(email)) {
      showError(emailInput, $("emailError"), "Email không đúng định dạng. Ví dụ: ten@gmail.com");
      return false;
    }

    showSuccess(emailInput, $("emailError"));
    return true;
  }

  function validatePhone() {
    const phoneInput = $("phone");
    const phone = phoneInput.value.replace(/\s/g, "");

    if (!phone) {
      showError(phoneInput, $("phoneError"), "Vui lòng nhập số điện thoại.");
      return false;
    }

    if (!phoneRegex.test(phone)) {
      showError(phoneInput, $("phoneError"), "Số điện thoại phải bắt đầu bằng 0 và có 10 số.");
      return false;
    }

    showSuccess(phoneInput, $("phoneError"));
    return true;
  }

  function checkStrength(password) {
    let score = 0;

    if (password.length >= 8) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;
    if (password.length >= 12) score++;

    return Math.min(score, 5);
  }

  function updateHint(id, isOk) {
    const hint = $(id);
    if (!hint) return;

    const icon = hint.querySelector("i");

    hint.style.color = isOk ? "#2F9E44" : "#64748B";
    if (icon) {
      icon.className = isOk ? "bi bi-check-circle-fill" : "bi bi-circle";
    }
  }

  function updatePasswordStrength() {
    const password = $("password").value;
    const strengthFill = $("strengthFill");
    const strengthText = $("strengthText");

    const levels = [
      { label: "", color: "transparent" },
      { label: "Rất yếu", color: "#E03131" },
      { label: "Yếu", color: "#F97316" },
      { label: "Trung bình", color: "#EAB308" },
      { label: "Mạnh", color: "#2F9E44" },
      { label: "Rất mạnh 💪", color: "#16A34A" }
    ];

    const score = checkStrength(password);

    if (strengthFill) {
      strengthFill.style.width = score * 20 + "%";
      strengthFill.style.background = levels[score].color;
    }

    if (strengthText) {
      strengthText.textContent = password ? levels[score].label : "";
      strengthText.style.color = levels[score].color;
    }

    updateHint("hint-length", password.length >= 8);
    updateHint("hint-upper", /[A-Z]/.test(password));
    updateHint("hint-number", /[0-9]/.test(password));
    updateHint("hint-special", /[^A-Za-z0-9]/.test(password));
  }

  function validatePassword() {
    const passwordInput = $("password");
    const password = passwordInput.value;

    if (!password) {
      showError(passwordInput, $("passwordError"), "Vui lòng nhập mật khẩu.");
      return false;
    }

    if (password.length < 8) {
      showError(passwordInput, $("passwordError"), "Mật khẩu phải có ít nhất 8 ký tự.");
      return false;
    }

    showSuccess(passwordInput, $("passwordError"));
    return true;
  }

  function validateConfirmPassword() {
    const password = $("password").value;
    const confirmPasswordInput = $("confirmPassword");
    const confirmPassword = confirmPasswordInput.value;

    if (!confirmPassword) {
      showError(confirmPasswordInput, $("confirmPasswordError"), "Vui lòng xác nhận mật khẩu.");
      return false;
    }

    if (password !== confirmPassword) {
      showError(confirmPasswordInput, $("confirmPasswordError"), "Mật khẩu xác nhận không khớp.");
      return false;
    }

    showSuccess(confirmPasswordInput, $("confirmPasswordError"));
    return true;
  }

  $("nextBtn")?.addEventListener("click", () => {
    const okFirstName = validateName($("firstName"), $("firstNameError"), "Vui lòng nhập họ.");
    const okLastName = validateName($("lastName"), $("lastNameError"), "Vui lòng nhập tên.");
    const okEmail = validateEmail();
    const okPhone = validatePhone();

    if (okFirstName && okLastName && okEmail && okPhone) {
      goToStep(2);
    }
  });

  $("nextBtn2")?.addEventListener("click", () => {
    const okPassword = validatePassword();
    const okConfirm = validateConfirmPassword();

    if (okPassword && okConfirm) {
      $("summaryName").textContent = `${$("firstName").value.trim()} ${$("lastName").value.trim()}`;
      $("summaryEmail").textContent = $("email").value.trim();
      $("summaryPhone").textContent = $("phone").value.trim();

      goToStep(3);
    }
  });

  $("backBtn")?.addEventListener("click", () => goToStep(1));
  $("backBtn2")?.addEventListener("click", () => goToStep(2));

  $("password")?.addEventListener("input", () => {
    const passwordInput = $("password");

    if (!passwordInput.value) {
      clearState(passwordInput, $("passwordError"));
    } else {
      validatePassword();
    }

    updatePasswordStrength();

    if ($("confirmPassword").value) {
      validateConfirmPassword();
    }
  });

  $("confirmPassword")?.addEventListener("input", () => {
    if (!$("confirmPassword").value) {
      clearState($("confirmPassword"), $("confirmPasswordError"));
      return;
    }

    validateConfirmPassword();
  });

  $("email")?.addEventListener("input", () => {
    if (!$("email").value.trim()) {
      clearState($("email"), $("emailError"));
      return;
    }

    validateEmail();
  });

  $("phone")?.addEventListener("input", () => {
    const phoneInput = $("phone");

    phoneInput.value = phoneInput.value.replace(/[^\d\s]/g, "");

    if (!phoneInput.value.trim()) {
      clearState(phoneInput, $("phoneError"));
      return;
    }

    validatePhone();
  });

  $("firstName")?.addEventListener("input", () => {
    if (!$("firstName").value.trim()) clearState($("firstName"), $("firstNameError"));
    else validateName($("firstName"), $("firstNameError"), "Vui lòng nhập họ.");
  });

  $("lastName")?.addEventListener("input", () => {
    if (!$("lastName").value.trim()) clearState($("lastName"), $("lastNameError"));
    else validateName($("lastName"), $("lastNameError"), "Vui lòng nhập tên.");
  });

  $("togglePw")?.addEventListener("click", () => {
    const passwordInput = $("password");
    const isHidden = passwordInput.type === "password";

    passwordInput.type = isHidden ? "text" : "password";
    $("eyeIcon1").className = isHidden ? "bi bi-eye-slash" : "bi bi-eye";
  });

  $("togglePw2")?.addEventListener("click", () => {
    const confirmPasswordInput = $("confirmPassword");
    const isHidden = confirmPasswordInput.type === "password";

    confirmPasswordInput.type = isHidden ? "text" : "password";
    $("eyeIcon2").className = isHidden ? "bi bi-eye-slash" : "bi bi-eye";
  });

  $("terms")?.addEventListener("change", () => {
    if ($("terms").checked) {
      $("termsError")?.classList.remove("show");
    }
  });

  $("registerForm")?.addEventListener("submit", (event) => {
    event.preventDefault();

    if (!$("terms").checked) {
      $("termsError")?.classList.add("show");
      return;
    }

    $("termsError")?.classList.remove("show");

    const submitBtn = $("submitBtn");
    submitBtn.classList.add("loading");
    submitBtn.disabled = true;

    setTimeout(() => {
      $("successOverlay")?.classList.add("show");

      setTimeout(() => {
        window.location.href = "login.html";
      }, 1800);
    }, 1000);
  });

  goToStep(1);
});