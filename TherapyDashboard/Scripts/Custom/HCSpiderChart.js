function spiderChart(data) {

    let prev = [5, 6, 4, 4, 3, 3, 4, 2, 2];
    let curr = [4, 5, 4, 3, 3, 2, 4, 2, 1]

    document.getElementById("spiderchart").style.display = "block";

    Highcharts.chart('spiderchart', {

        chart: {
            polar: true,
            type: 'line'
        },

        title: {
            text: 'MADRS form for 25th Oct'
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

        

        credits: {
            enabled: false
        },


        series: [{
            type: 'area',
            name: 'Selected',
            data: curr,
            pointPlacement: 'on'
        }, {
            name: 'Previous',
            data: prev,
            pointPlacement: 'on'
        }]

    });
}