
Highcharts.chart('activitychart', {
    chart: {
        type: 'column'
    },
    title: {
        text: null
    },
    yAxis: {
        title: {
            text: 'Times logged in'
        }
    },
    xAxis: {
        title: {
            text: 'Week'
        }
    },

    plotOptions: {
        series: {
            label: {
                connectorAllowed: false
            },
            pointStart: 32
        }
    },

    series: [{
        name: 'Logins',
        data: [3, 0, 1, 2, 0, 4, 1, 1]
    }],

    legend: {
        enabled: false
    },

    credits: {
        enabled: false
    },



});