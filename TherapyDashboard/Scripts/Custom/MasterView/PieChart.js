
Highcharts.chart('piechart', {
    chart: {
        plotBackgroundColor: null,
        plotBorderWidth: null,
        plotShadow: false,
        type: 'pie'
    },
    title: {
        text: null
    },
    tooltip: {
        pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
    },

    credits: {
        enabled: false
    },

    plotOptions: {
        pie: {
            allowPointSelect: true,
            cursor: 'pointer',
            dataLabels: {
                enabled: false
            },
            showInLegend: true
        }
    },
    series: [{
        name: 'Patients',
        colorByPoint: true,
        data: [{
            name: 'Steady',
            y: 61,
            sliced: true,
            selected: true
        }, {
            name: 'Improving',
            y: 30
        }, {
            name: 'Declining',
            y: 10
        }]
    }]

});