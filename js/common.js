document.addEventListener("DOMContentLoaded", () => {
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
      threshold: 0.35,
      rootMargin: "0px 0px -120px 0px"
    }
  );

  revealItems.forEach((item) => observer.observe(item));
}