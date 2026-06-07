document.addEventListener("DOMContentLoaded", () => {
  document.body.classList.add("reveal-ready");
  initScrollReveal();
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