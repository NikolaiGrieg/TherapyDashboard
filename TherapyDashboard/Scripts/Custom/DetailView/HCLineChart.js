
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

    series: [{
        name: 'Patient',
        data: [42, 30, 32, 25, 20, 24, 20, 15]
    },  {
        name: 'Average',
        data: [44, 33, 31, 25, 21, 20, 18, 14]
        }],


    plotOptions: {
        series: {
            cursor: 'pointer',
            point: {
                events: {
                    click: function (e) {
                        
                        document.getElementById('spiderchart').hidden = false
                        console.log(this.y)
                        var data = [5, 5, 6, 3, 4, 3, 2, 4, 2];

                        let scale = this.y / 52; 
                        for (let i = 0; i < data.length; i++) {
                            data[i] = Math.round((data[i] - Math.random() * scale) % 6) 
                        }
                        spiderChart(data)
                        
                        
                        
                        /*
                        hs.htmlExpand(null, {
                            pageOrigin: {
                                x: e.pageX || e.clientX,
                                y: e.pageY || e.clientY
                            },
                            headingText: this.series.name,
                            maincontentText: this.y,
                            width: 200
                        });
                        */
                    }
                }
            },
            marker: {
                lineWidth: 1
            }
        }
    },
   

    legend: {
        layout: 'vertical',
        align: 'right',
        verticalAlign: 'middle'
    },

    credits: {
        enabled: false
    },

    

});