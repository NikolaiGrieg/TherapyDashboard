function calculateDurationBins(data){
    //get highest month
    let calcs = {};
    let maxMonth = 0;
    let minMonth = Infinity;
    let monthDict = {};
    Object.values(data).forEach(date => {
        let diffDays = getDiffDays(date);
        let diffMonths = Math.floor(diffDays / 30); //approximate month
        if (diffMonths > maxMonth) {
            maxMonth = diffMonths;
        }
        if (diffMonths < minMonth) {
            minMonth = diffMonths;
        }

        if (monthDict[diffMonths]) {
            monthDict[diffMonths] += 1;
        }
        else {
            monthDict[diffMonths] = 1;
        }
    });

    let months = [];
    for (let i = minMonth; i < maxMonth+1; i++){ //months are 0 indexed
        if (monthDict[i]){
            months.push(monthDict[i]);
        }
        else{
            months.push(0);
        }
    }
    calcs['months'] = months;
    calcs['min'] = minMonth;
    calcs['max'] = maxMonth;

    return calcs;
}

function getDiffDays(date){
    var oneDay = 24*60*60*1000; // hours*minutes*seconds*milliseconds
    var firstDate = date;
    var secondDate = new Date();

    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime())/oneDay));
    return diffDays;
}

function plotPatientDuration(QRdates){
    let data = calculateDurationBins(QRdates);
    let values = data['months'];
    let minMonth = data['min'];
    let maxMonth = data['max'];

    Highcharts.chart('barchart', {
        chart: {
            type: 'column'
        },
        title: {
            text: null
        },
        xAxis: {
            categories: Array.from(new Array(maxMonth), (x,i) => i + minMonth),
            crosshair: true,
            title: {
                text: 'Months in treatment'
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Num Patients'
            }
        },
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            },
            series: {
                animation: false
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            enabled: false
        },
        series: [{
            name: 'Patients',
            data: values,
            point:{
              events:{
                  click: function (event) {
                        let monthList = Array.from(new Array(maxMonth), (x,i) => i + minMonth);
                        let diffDays = monthList[this.x]*30;
                        let filter = dateToHumanReadable(undefined, diffDays); //from tableController

                        sortTableByCategory(filter); //from tableController
                  }
              }
            }

        }]
    });
}
