var pieColors = (function () {
    //0: steady, 1: improving, 2: declining
    //blue, green, red
    var colors = ['#058DC7', '#50B432', '#ED561B'];

    return colors;
}());

function plotSummariesPieChart(data){
    let categories = ['steady', 'improving', 'declining'];
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
            pointFormat: '{series.name}: <b>{point.y}</b> - <b>{point.percentage:.1f}%</b> '
        },

        credits: {
            enabled: false
        },

        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                colors: pieColors,
                dataLabels: {
                    enabled: false
                },
                showInLegend: true,
                animation: false,
                slicedOffset: 0
            }
        },
        series: [{
            name: 'Patients',
            colorByPoint: true,
            data: data,
            point:{
              events:{
                  click: function (event) {
                      sortTableByCategory(categories[this.x]);
                  }
              }
          } 
        }]

    });
}
