(function () {
    "use strict";

    const chartContainer = document.getElementById("thongKeBieuDo");
    const breakdown = document.getElementById("thongKeChiTiet");
    if (!chartContainer || !breakdown || typeof window.Chart !== "function") return;

    const rows = Array.from(breakdown.querySelectorAll("[data-ky]"));
    const values = rows.map(row => Number(row.dataset.thucThu));
    const currency = new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND"
    });

    chartContainer.hidden = false;
    try {
        new window.Chart(document.getElementById("thongKeCanvas"), {
            type: "bar",
            data: {
                labels: rows.map(row => row.dataset.ky),
                datasets: [{
                    label: "Thực thu",
                    data: values,
                    backgroundColor: values.map(value => value < 0 ? "#c24156" : "#2479d9"),
                    borderRadius: 6,
                    maxBarThickness: 38
                }]
            },
            options: {
                locale: "vi-VN",
                responsive: true,
                maintainAspectRatio: false,
                animation: !window.matchMedia("(prefers-reduced-motion: reduce)").matches,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: { label: item => "Thực thu: " + currency.format(item.parsed.y) }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { color: "#d9e8f5" },
                        ticks: {
                            color: "#49637e",
                            maxTicksLimit: 8,
                            maxRotation: 0,
                            callback: function (value) {
                                const label = this.getLabelForValue(value);
                                return label.length === 10 ? label.slice(0, 5) : label;
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: "#e8f0f8" },
                        border: { display: false },
                        ticks: { color: "#49637e", maxTicksLimit: 6 },
                        title: { display: true, text: "Đồng" }
                    }
                }
            }
        });
        breakdown.open = false;
    } catch (error) {
        chartContainer.hidden = true;
    }
})();
