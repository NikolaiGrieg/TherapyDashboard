//TODO rename file

function calculateDurationBins(data){
    //get highest month
    let maxMonth = 0;
    let monthDict = {}
    Object.values(data).forEach(date => {
        let diffDays = getDiffDays(date);
        let diffMonths = Math.floor(diffDays/30) //approximate month
        if (diffMonths > maxMonth){
            maxMonth = diffMonths;
        }
        if(monthDict[diffMonths]){
            monthDict[diffMonths] += 1
        }
        else{
            monthDict[diffMonths] = 1
        }
    })

    let months = [];
    for (let i = 0; i < maxMonth+1; i++){ //months are 0 indexed
        if (monthDict[i]){
            months.push(monthDict[i])
        }
        else{
            months.push(0);
        }
    }

    return months;
}

//TODO fix duplicate function between here and tablecontroller
function getDiffDays(date){
    var oneDay = 24*60*60*1000; // hours*minutes*seconds*milliseconds
    var firstDate = date;
    var secondDate = new Date();

    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime())/(oneDay)));
    return diffDays;
}

function plotPatientDuration(QRdates){
    let data = calculateDurationBins(QRdates);
    Highcharts.chart('barchart', {
        chart: {
            type: 'column'
        },
        title: {
            text: null
        },
        xAxis: {
            categories: [...Array(12).keys()],
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
            data: data

        }]
    });
}
