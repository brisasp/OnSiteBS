window._charts = {};

window.renderBarChart = (canvasId, labels, data, label) => {
    if (window._charts[canvasId]) {
        window._charts[canvasId].destroy();
    }
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    const todayIdx = data.length - 1;
    const bgColors = data.map((_, i) =>
        i === todayIdx ? 'rgba(169,28,50,0.85)' : 'rgba(169,28,50,0.18)'
    );
    const borderColors = data.map((_, i) =>
        i === todayIdx ? 'rgba(169,28,50,1)' : 'rgba(169,28,50,0.6)'
    );
    const tickColors = labels.map((_, i) =>
        i === todayIdx ? '#a91c32' : '#94a3b8'
    );
    const tickWeights = labels.map((_, i) =>
        i === todayIdx ? '700' : '400'
    );

    window._charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                backgroundColor: bgColors,
                borderColor: borderColors,
                borderWidth: 2,
                borderRadius: 6,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1, color: '#94a3b8', font: { size: 11 } },
                    grid: { color: '#f1f5f9' }
                },
                x: {
                    ticks: {
                        color: (ctx) => tickColors[ctx.index] ?? '#94a3b8',
                        font: (ctx) => ({ size: 11, weight: tickWeights[ctx.index] ?? '400' })
                    },
                    grid: { display: false }
                }
            }
        }
    });
};

window.renderDoughnutChart = (canvasId, labels, data, colors) => {
    if (window._charts[canvasId]) {
        window._charts[canvasId].destroy();
    }
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    window._charts[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '68%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { color: '#475569', font: { size: 11 }, padding: 12 }
                }
            }
        }
    });
};
