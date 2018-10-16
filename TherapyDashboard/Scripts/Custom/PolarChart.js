

Highcharts.chart('polarchart', {

    chart: {
        polar: true,
        type: 'line'
    },

    title: {
        text: null
    },

    pane: {
        size: '80%'
    },

    xAxis: {
        categories: ['Tristhet', 'Indre spenning', 'Redusert nattesøvn', 'Svekket appetitt', 'Konsentrasjonsvansker',
            'Initiativløshet', 'Svekkede følelsesmessige reaksjoner', 'Depressivt tankeinnhold', 'Suicidaltanker'],
        tickmarkPlacement: 'on',
        lineWidth: 0
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
        data: [5, 5, 6, 3, 4, 3, 2, 4, 2],
        pointPlacement: 'on'
    }]

});