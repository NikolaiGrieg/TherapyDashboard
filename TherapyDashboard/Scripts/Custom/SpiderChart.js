function spiderChart(data) {


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
            max: 6
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
            name: 'Area',
            data: data,
            pointPlacement: 'on'
        }]

    });
}