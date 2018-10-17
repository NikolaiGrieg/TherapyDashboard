function spiderChart(data) {

    var prev = [4, 6, 4, 4, 3, 3, 4, 2, 2];

    Highcharts.chart('spiderchart', {

        chart: {
            polar: true,
            type: 'line'
        },

        title: {
            text: null
        },


        xAxis: {
            categories: ['Tristhet', 'Indre spenning', 'Redusert nattesøvn', 'Svekket appetitt', 'Konsentrasjonsvansker',
                'Initiativløshet', 'Svekkede følelsesmessige reaksjoner', 'Depressivt tankeinnhold', 'Suicidaltanker'],
            tickmarkPlacement: 'on',
            lineWidth: 0
        },

        yAxis: {
            gridLineInterpolation: 'polygon',
            lineWidth: 0,
            max: 6,
            minRange: 1,
            tickInterval: 1
        },

        tooltip: {
            enabled: false
        },

        legend: {
            enabled: false
        },

        credits: {
            enabled: false
        },


        series: [{
            type: 'area',
            name: 'Patient',
            data: data,
            pointPlacement: 'on'
        }, {
            type: 'area',
            name: 'Previous',
            data: prev,
            pointPlacement: 'on'
        }]

    });
}