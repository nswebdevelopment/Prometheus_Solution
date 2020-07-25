// Write your Javascript code.
function handleSideBarListItem() {
    var path = window.location.pathname.substring(1);
    var controllerName = null;
    if (path.includes("/")) {
        controllerName = path.substring(0, path.indexOf('/')).toLowerCase();

    }
    else {
        controllerName = path.toLowerCase();

    }

    switch (controllerName) {
        case "enterpriseadapter":
            jQuery("li[id=enterprise]").addClass("active");
            break;
        case "cryptoadapter":
            jQuery("li[id=crypto]").addClass("active");
            break;
        case "businessadapter":
            jQuery("li[id=business]").addClass("active");
            break;
        case "jobdefinition":
            jQuery("li[id=definition]").addClass("active");
            break;
        case "jobhistory":
            jQuery("li[id=history]").addClass("active");
            break;
        case "schedule":
            jQuery("li[id=schedule]").addClass("active");
            break;
        case "jobtimeline":
            jQuery("li[id=timeline]").addClass("active");
            break;
        case "exchanges":
            jQuery("li[id=exchanges]").addClass("active");
            break;
        case "media":
            jQuery("li[id=media]").addClass("active");
            break;
    }
}

function handlemediaCharts(labels) {
    //media bar chart with ajax
    jQuery.ajax({
        type: "GET",
        url: "/Media/GetMediaReport",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            for (var i = 0; i < result.length; i++) {
                mediaChart.data.datasets[0].data.push(result[i]);
            }
            mediaChart.update();
            jQuery("#mediabody").removeClass("loader");
        }
    });

    var mediaCanvas = document.getElementById("MediaCanvas");
    mediaCanvas.height = 200;
    var mediaChart = new Chart(mediaCanvas, {
        type: 'horizontalBar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Daily mentions",
                    //backgroundColor: "rgba(0, 123, 255, 0.5)"
                    backgroundColor: [
                        "#f7931a",
                        "#676B8A",
                        "rgba(0, 123, 255,0.7)",
                        "#b8b8b8",
                        "black",
                        "#00CC00",
                        "#1d4252",
                        "#D3D3D3"
                    ]
                }
            ]
        },
        options: {
            legend: { display: false },
            title: {
                display: true,
                text: 'Daily mentions'
            },
            scales: {
                xAxes: [{
                    ticks: {
                        suggestedMin: 0,
                        suggestedMax: 25
                    }
                }]
            }
        }
    });

    jQuery("#mediabody").addClass("loader");
    ////////////////////////////////////////////////////////////////////

    //social media pie chart with ajax
    jQuery.ajax({
        type: "GET",
        url: "/Media/GetSocialMediaReport",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            for (var i = 0; i < result.length; i++) {
                socialmediachart.data.datasets[0].data.push(result[i]);
            }
            socialmediachart.update();
            jQuery("#socialmediabody").removeClass("loader");
        }
    });

    var socialMediaCanvas = document.getElementById("SocialMediaCanvas");
    socialMediaCanvas.height = 200;
    var socialmediachart = new Chart(socialMediaCanvas, {
        type: 'doughnut',
        data: {
            datasets: [{
                backgroundColor: [
                    "#f7931a",
                    "#676B8A",
                    "rgba(0, 123, 255,0.7)",
                    "#b8b8b8",
                    "black",
                    "#00CC00",
                    "#1d4252",
                    "#D3D3D3"
                ]
                //hoverBackgroundColor: [
                //    "rgba(0, 123, 255,0.9)",
                //    "rgba(0, 123, 255,0.7)",
                //    "rgba(0, 123, 255,0.5)",
                //    "rgba(0,0,0,0.07)"
                //]

            }],
            labels: labels
        },
        options: {
            responsive: true
        }
    });

    jQuery("#socialmediabody").addClass("loader");
    /////////////////////////////////////////////////////////////
}

//Transactions & Jobs chart
function handleDashboardCharts(transactionsChartData, jobsChartData) {

    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    var lastSixMonths = [];

    var today = new Date();
    var month = today.getMonth();
    var year = today.getFullYear();

    var i = 0;

    do {
        if (month < 0) {
            month = 11;
            year--;
        }
        lastSixMonths.push(monthNames[month] + " " + year);
        month--;
        i++;
    } while (i < 6);

    lastSixMonths.reverse();

    var transactionChartCanvas = document.getElementById("transactionsChart");
    var transactionsChart = new Chart(transactionChartCanvas, {
        type: 'bar',
        data: {
            labels: lastSixMonths,
            datasets: [{
                data: transactionsChartData,
                label: '# of Transactions',
                backgroundColor: [
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(54, 162, 235, 0.2)'
                ],
                borderColor: [
                    'rgba(54, 162, 235, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(54, 162, 235, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            legend: {
                display: false
            },
            responsive: true,
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    labelColor: function (tooltipItem, chart) {
                        return {
                            borderColor: 'rgba(54, 162, 235, 1)',
                            backgroundColor: 'rgba(54, 162, 235, 0.2)'
                        }
                    }
                }
            }
        }
    });
    
    var jobChartCanvas = document.getElementById("jobsChart");
    var jobsChart = new Chart(jobChartCanvas, {
        type: 'line',
        data: {
            labels: lastSixMonths,
            datasets: [{
                data: jobsChartData,
                label: '# of Jobs',
                backgroundColor: [
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgba(255, 159, 64, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            legend: {
                display: false
            },
            responsive: true,
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    labelColor: function (tooltipItem, chart) {
                        return {
                            borderColor: 'rgba(255, 159, 64, 1)',
                            backgroundColor: 'rgba(255, 159, 64, 0.2)'
                        }
                    }
                }
            }
        }
    });
}

