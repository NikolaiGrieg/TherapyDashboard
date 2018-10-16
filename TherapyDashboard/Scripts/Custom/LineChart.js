
Highcharts.chart('linechart', {

    title: {
        text: null
    },
    yAxis: {
        title: {
            text: 'MADRS-S'
        }
    },
    xAxis: {
        title: {
            text: 'Measurement/Time'
        }
    },

    legend: {
        enabled: false
    },

    series: [{
        name: 'MADRS Score',
        data: [42, 30, 32, 25, 20, 24, 20, 15]
    }],

    credits: {
        enabled: false
    },

    

});