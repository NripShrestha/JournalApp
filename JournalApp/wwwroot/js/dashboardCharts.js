window.dashboardCharts = {
    renderMoodPie: function (data) {
        const ctx = document.getElementById("moodPieChart");
        if (!ctx) return;

        // Destroy existing chart if present
        if (window.moodChart) {
            window.moodChart.destroy();
        }

        // Check if dark mode is active
        const isDarkMode = document.body.classList.contains('dark');
        const legendColor = isDarkMode ? '#e2e8f0' : '#1f2937';
        const borderColor = isDarkMode ? '#374151' : '#ffffff';

        // Define colors for each category
        const colors = {
            'Positive': '#4ade80',  // Green
            'Neutral': '#fbbf24',   // Yellow
            'Negative': '#f87171'   // Red
        };

        const labels = Object.keys(data);
        const values = Object.values(data);
        const backgroundColors = labels.map(label => colors[label] || '#94a3b8');

        window.moodChart = new Chart(ctx, {
            type: "pie",
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: backgroundColors,
                    borderColor: borderColor,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            padding: 15,
                            color: legendColor,
                            font: {
                                size: 12,
                                family: "'Inter', sans-serif"
                            },
                            generateLabels: function (chart) {
                                const data = chart.data;
                                const total = data.datasets[0].data.reduce((a, b) => a + b, 0);

                                return data.labels.map((label, i) => {
                                    const value = data.datasets[0].data[i];
                                    const percentage = ((value / total) * 100).toFixed(1);

                                    return {
                                        text: `${label}: ${value} (${percentage}%)`,
                                        fillStyle: data.datasets[0].backgroundColor[i],
                                        hidden: false,
                                        index: i,
                                        fontColor: legendColor // This ensures the text color
                                    };
                                });
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: isDarkMode ? '#1e293b' : '#ffffff',
                        titleColor: isDarkMode ? '#e2e8f0' : '#11151a',
                        bodyColor: isDarkMode ? '#e2e8f0' : '#1f2937',
                        borderColor: isDarkMode ? '#334155' : '#e5e7eb',
                        borderWidth: 1,
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }
};