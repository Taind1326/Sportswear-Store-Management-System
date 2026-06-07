document.addEventListener("DOMContentLoaded", () => {
  const $ = (id) => document.getElementById(id);
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const phoneRegex = /^0[0-9]{9}$/;

  function showErr(input, errEl, msg) {
    if (!input || !errEl) return;

    input.classList.add("is-invalid");
    input.classList.remove("is-valid");

    const span = errEl.querySelector("span");
    if (span && msg) span.textContent = msg;

    errEl.classList.add("show");
  }

  function showOk(input, errEl) {
    if (!input || !errEl) return;

    input.classList.remove("is-invalid");
    input.classList.add("is-valid");
    errEl.classList.remove("show");
  }

  function clearState(input, errEl) {
    if (!input || !errEl) return;

    input.classList.remove("is-invalid", "is-valid");
    errEl.classList.remove("show");
  }

  // SCROLL REVEAL: lướt tới đâu hiện tới đó
 // SCROLL REVEAL: kéo tới đâu hiện tới đó
const revealItems = document.querySelectorAll(".reveal");

const revealObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    const rect = entry.target.getBoundingClientRect();
    const windowHeight = window.innerHeight;

    if (entry.isIntersecting && rect.top < windowHeight * 0.82) {
      entry.target.classList.add("show");
      revealObserver.unobserve(entry.target);
    }
  });
}, {
  threshold: 0.4,
  rootMargin: "0px 0px -160px 0px"
});

revealItems.forEach((item) => {
  revealObserver.observe(item);
});

  // TOPIC CHIPS
  document.querySelectorAll(".sz-chip").forEach((chip) => {
    chip.addEventListener("click", () => {
      document.querySelectorAll(".sz-chip").forEach((item) => item.classList.remove("selected"));
      chip.classList.add("selected");
    });
  });

  // CHAR COUNT
  const msgInput = $("cMessage");
  const charCount = $("charCount");

  msgInput?.addEventListener("input", () => {
    if (msgInput.value.length > 500) {
      msgInput.value = msgInput.value.slice(0, 500);
    }

    const length = msgInput.value.length;
    charCount.textContent = `${length} / 500 ký tự`;
    charCount.classList.toggle("warn", length > 480);

    if (!msgInput.value.trim()) {
      clearState(msgInput, $("cMessageErr"));
      return;
    }

    if (msgInput.value.trim().length >= 20) {
      showOk(msgInput, $("cMessageErr"));
    }
  });

  // STAR RATING
  let starValue = 0;

  function paintStars(value) {
    document.querySelectorAll(".sz-star").forEach((star, index) => {
      star.classList.toggle("active", index < value);
    });
  }

  document.querySelectorAll(".sz-star").forEach((star) => {
    star.addEventListener("mouseenter", () => paintStars(Number(star.dataset.v)));
    star.addEventListener("mouseleave", () => paintStars(starValue));
    star.addEventListener("click", () => {
      starValue = Number(star.dataset.v);
      paintStars(starValue);
    });
  });

  // REAL-TIME VALIDATION
  $("cName")?.addEventListener("input", () => {
    const input = $("cName");
    if (!input.value.trim()) clearState(input, $("cNameErr"));
    else showOk(input, $("cNameErr"));
  });

  $("cEmail")?.addEventListener("input", () => {
    const input = $("cEmail");
    const value = input.value.trim();

    if (!value) {
      clearState(input, $("cEmailErr"));
      return;
    }

    if (emailRegex.test(value)) showOk(input, $("cEmailErr"));
    else showErr(input, $("cEmailErr"), "Email không đúng định dạng.");
  });

  $("cPhone")?.addEventListener("input", () => {
    const input = $("cPhone");
    input.value = input.value.replace(/[^\d\s]/g, "");

    const value = input.value.replace(/\s/g, "");

    if (!value) {
      clearState(input, $("cPhoneErr"));
      return;
    }

    if (phoneRegex.test(value)) showOk(input, $("cPhoneErr"));
    else showErr(input, $("cPhoneErr"), "Số điện thoại phải bắt đầu bằng 0 và có 10 số.");
  });

  $("cSubject")?.addEventListener("input", () => {
    const input = $("cSubject");
    if (!input.value.trim()) clearState(input, $("cSubjectErr"));
    else showOk(input, $("cSubjectErr"));
  });

  // FORM SUBMIT
  $("contactForm")?.addEventListener("submit", (event) => {
    event.preventDefault();

    const name = $("cName").value.trim();
    const email = $("cEmail").value.trim();
    const phone = $("cPhone").value.replace(/\s/g, "");
    const subject = $("cSubject").value.trim();
    const message = $("cMessage").value.trim();

    let valid = true;

    if (!name) {
      showErr($("cName"), $("cNameErr"), "Vui lòng nhập họ tên.");
      valid = false;
    } else showOk($("cName"), $("cNameErr"));

    if (!email || !emailRegex.test(email)) {
      showErr($("cEmail"), $("cEmailErr"), !email ? "Vui lòng nhập email." : "Email không đúng định dạng.");
      valid = false;
    } else showOk($("cEmail"), $("cEmailErr"));

    if (!phone || !phoneRegex.test(phone)) {
      showErr($("cPhone"), $("cPhoneErr"), !phone ? "Vui lòng nhập số điện thoại." : "Số điện thoại phải bắt đầu bằng 0 và có 10 số.");
      valid = false;
    } else showOk($("cPhone"), $("cPhoneErr"));

    if (!subject) {
      showErr($("cSubject"), $("cSubjectErr"), "Vui lòng nhập tiêu đề.");
      valid = false;
    } else showOk($("cSubject"), $("cSubjectErr"));

    if (!message || message.length < 20) {
      showErr($("cMessage"), $("cMessageErr"), !message ? "Vui lòng nhập nội dung." : "Nội dung quá ngắn, ít nhất 20 ký tự.");
      valid = false;
    } else showOk($("cMessage"), $("cMessageErr"));

    if (!valid) return;

    const button = $("sendBtn");
    button.classList.add("loading");
    button.disabled = true;

    setTimeout(() => {
      button.classList.remove("loading");
      button.disabled = false;
      $("formContent").style.display = "none";
      $("sendSuccess").classList.add("show");
    }, 1100);
  });

  // SEND AGAIN
  $("sendAgainBtn")?.addEventListener("click", () => {
    $("contactForm").reset();

    document.querySelectorAll(".sz-input").forEach((input) => {
      input.classList.remove("is-valid", "is-invalid");
    });

    document.querySelectorAll(".sz-error").forEach((error) => {
      error.classList.remove("show");
    });

    document.querySelectorAll(".sz-chip").forEach((chip) => chip.classList.remove("selected"));
    document.querySelector(".sz-chip")?.classList.add("selected");

    charCount.textContent = "0 / 500 ký tự";
    charCount.classList.remove("warn");

    starValue = 0;
    paintStars(0);

    $("sendSuccess").classList.remove("show");
    $("formContent").style.display = "";
  });
});
