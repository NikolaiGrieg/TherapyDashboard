
Highcharts.chart('barchart', {
    chart: {
        type: 'column'
    },
    title: {
        text: null
    },
    xAxis: {
        categories: [...Array(8).keys()],
        crosshair: true,
        title: {
            text: 'Module'
        }
    },
    yAxis: {
        min: 0,
        title: {
            text: 'Num Patients'
        }
    },
    plotOptions: {
        column: {
            pointPadding: 0.2,
            borderWidth: 0
        }
    },
    credits: {
        enabled: false
    },
    legend: {
        enabled: false
    },
    series: [{
        name: 'Patients',
        data: [20, 17, 14, 17, 13, 10, 9, 8]

    }]
});