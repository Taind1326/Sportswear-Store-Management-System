document.addEventListener("DOMContentLoaded", () => {
  const $ = (id) => document.getElementById(id);
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const demoOtp = "123456";

  let countdownTimer = null;
  let secondsLeft = 60;

  function showErr(input, error, message) {
    if (!input || !error) return;
    input.classList.add("is-invalid");
    input.classList.remove("is-valid");
    const span = error.querySelector("span");
    if (span && message) span.textContent = message;
    error.classList.add("show");
  }

  function showOk(input, error) {
    if (!input || !error) return;
    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    error.classList.remove("show");
  }

  function clearState(input, error) {
    if (!input || !error) return;
    input.classList.remove("is-invalid", "is-valid");
    error.classList.remove("show");
  }

  function setLoading(button, isLoading) {
    if (!button) return;
    button.classList.toggle("loading", isLoading);
    button.disabled = isLoading;
  }

  function setView(viewNumber) {
    ["view1", "view2", "view3"].forEach((id, index) => {
      $(id).style.display = index + 1 === viewNumber ? "block" : "none";
    });

    ["seg1", "seg2", "seg3"].forEach((id, index) => {
      $(id).classList.toggle("active", index + 1 <= viewNumber);
    });
  }

  function startCountdown() {
    clearInterval(countdownTimer);
    secondsLeft = 60;
    $("resendBtn").disabled = true;
    $("countdownText").textContent = secondsLeft;

    countdownTimer = setInterval(() => {
      secondsLeft--;
      $("countdownText").textContent = secondsLeft;

      if (secondsLeft <= 0) {
        clearInterval(countdownTimer);
        $("resendBtn").disabled = false;
        $("resendBtn").innerHTML = "Gửi lại";
      }
    }, 1000);
  }

  function getOtpValue() {
    return Array.from(document.querySelectorAll(".otp-input"))
      .map((input) => input.value)
      .join("");
  }

  function clearOtp() {
    document.querySelectorAll(".otp-input").forEach((input) => {
      input.value = "";
      input.classList.remove("filled");
    });
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

  function updateStrength() {
    const password = $("newPw").value;
    const score = checkStrength(password);
    const levels = [
      { text: "", color: "transparent" },
      { text: "Rất yếu", color: "#E03131" },
      { text: "Yếu", color: "#F97316" },
      { text: "Trung bình", color: "#EAB308" },
      { text: "Mạnh", color: "#2F9E44" },
      { text: "Rất mạnh", color: "#16A34A" }
    ];

    $("newFill").style.width = score * 20 + "%";
    $("newFill").style.background = levels[score].color;
    $("newStrTxt").textContent = password ? levels[score].text : "";
    $("newStrTxt").style.color = levels[score].color;
  }

  function togglePassword(inputId, iconId) {
    const input = $(inputId);
    const icon = $(iconId);
    const isHidden = input.type === "password";
    input.type = isHidden ? "text" : "password";
    icon.className = isHidden ? "bi bi-eye-slash" : "bi bi-eye";
  }

  $("emailInput")?.addEventListener("input", () => {
    const email = $("emailInput").value.trim();
    if (!email) return clearState($("emailInput"), $("emailError"));
    if (emailRegex.test(email)) showOk($("emailInput"), $("emailError"));
    else showErr($("emailInput"), $("emailError"), "Email không đúng định dạng.");
  });

  $("sendOtpBtn")?.addEventListener("click", () => {
    const email = $("emailInput").value.trim();

    if (!email || !emailRegex.test(email)) {
      showErr($("emailInput"), $("emailError"), !email ? "Vui lòng nhập email." : "Email không đúng định dạng.");
      return;
    }

    showOk($("emailInput"), $("emailError"));
    setLoading($("sendOtpBtn"), true);

    setTimeout(() => {
      setLoading($("sendOtpBtn"), false);
      $("emailDisplay").textContent = email;
      setView(2);
      startCountdown();
      document.querySelector(".otp-input")?.focus();
    }, 900);
  });

  document.querySelectorAll(".otp-input").forEach((input, index, list) => {
    input.addEventListener("input", () => {
      input.value = input.value.replace(/\D/g, "").slice(0, 1);
      input.classList.toggle("filled", !!input.value);
      $("otpError").classList.remove("show");

      if (input.value && list[index + 1]) {
        list[index + 1].focus();
      }
    });

    input.addEventListener("keydown", (event) => {
      if (event.key === "Backspace" && !input.value && list[index - 1]) {
        list[index - 1].focus();
      }
    });

    input.addEventListener("paste", (event) => {
      event.preventDefault();
      const pasted = event.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
      list.forEach((box, boxIndex) => {
        box.value = pasted[boxIndex] || "";
        box.classList.toggle("filled", !!box.value);
      });
    });
  });

  $("verifyOtpBtn")?.addEventListener("click", () => {
    const otp = getOtpValue();

    if (otp.length !== 6 || otp !== demoOtp) {
      $("otpError").classList.add("show");
      clearOtp();
      document.querySelector(".otp-input")?.focus();
      return;
    }

    setLoading($("verifyOtpBtn"), true);

    setTimeout(() => {
      setLoading($("verifyOtpBtn"), false);
      setView(3);
    }, 700);
  });

  $("resendBtn")?.addEventListener("click", () => {
    clearOtp();
    $("otpError").classList.remove("show");
    $("resendBtn").innerHTML = 'Gửi lại (<span id="countdownText">60</span>s)';
    startCountdown();
  });

  $("newPw")?.addEventListener("input", () => {
    updateStrength();
    const password = $("newPw").value;
    if (!password) clearState($("newPw"), $("newPwError"));
    else if (password.length >= 8) showOk($("newPw"), $("newPwError"));
    else showErr($("newPw"), $("newPwError"), "Mật khẩu phải có ít nhất 8 ký tự.");
  });

  $("confirmPw")?.addEventListener("input", () => {
    const password = $("newPw").value;
    const confirm = $("confirmPw").value;
    if (!confirm) clearState($("confirmPw"), $("confirmPwError"));
    else if (confirm === password) showOk($("confirmPw"), $("confirmPwError"));
    else showErr($("confirmPw"), $("confirmPwError"), "Mật khẩu xác nhận không khớp.");
  });

  $("toggleNew")?.addEventListener("click", () => togglePassword("newPw", "eyeNew"));
  $("toggleConfirm")?.addEventListener("click", () => togglePassword("confirmPw", "eyeConfirm"));

  $("resetBtn")?.addEventListener("click", () => {
    const password = $("newPw").value;
    const confirm = $("confirmPw").value;
    let valid = true;

    if (!password || password.length < 8) {
      showErr($("newPw"), $("newPwError"), !password ? "Vui lòng nhập mật khẩu mới." : "Mật khẩu phải có ít nhất 8 ký tự.");
      valid = false;
    } else showOk($("newPw"), $("newPwError"));

    if (!confirm || confirm !== password) {
      showErr($("confirmPw"), $("confirmPwError"), !confirm ? "Vui lòng xác nhận mật khẩu." : "Mật khẩu xác nhận không khớp.");
      valid = false;
    } else showOk($("confirmPw"), $("confirmPwError"));

    if (!valid) return;

    setLoading($("resetBtn"), true);

    setTimeout(() => {
      setLoading($("resetBtn"), false);
      ["view1", "view2", "view3"].forEach((id) => $(id).style.display = "none");
      $("successState").classList.add("show");
    }, 900);
  });
});
