BarChart = function(_parentElement, controller, name, elements,
     dataPath, height = 200, width = 700){
  this.parentElement = _parentElement;
  this.controller = controller;
  this.name = name;
  this.elements = elements;
  this.dataPath = dataPath;
  this.initHeight = height;
  this.initWidt = width;

  this.initVis();

};

//TODO create and read from csv
BarChart.prototype.initVis = function(){
  var vis = this

  Highcharts.chart(vis.parentElement, {
    chart: {
        type: 'column',
        height: (5 / 16 * 100) + '%' // 16:5 ratio
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
}

