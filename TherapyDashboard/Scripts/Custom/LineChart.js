
Highcharts.chart('container', {

    title: {
        text: 'Summary'
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

    series: [{
        name: 'MADRS Score',
        data: [42, 30, 32, 25, 20, 24, 20, 15]
    }],

    credits: {
        enabled: false
    },

    

});