function initTable() {
    initFHIRData(); //comment this to use mongodb, uncomment for fhir
    enableTableSort();
    initSearch();
}

//TODO rename, controls the entire view
function initFHIRData(){
    getPatients().then(results => {
        //console.log(results)

        //TODO wrangle patients
        //TODO move this to data wrangling class
        let patNames = []
        let patIDs = []
        results.forEach(res =>{
            let name = "" + res.name[0].given[0] + " " + res.name[0].family;
            patNames.push(name)
            patIDs.push(res.id)
        })

        //TODO save resources on server, request new (date based) resources in search query
        //processing, TODO move
        createSummaries(results).then(summaries=>{
            createFlags(results).then(flags =>{
                buildTable(summaries, patNames, patIDs, flags)
            })

            let pieChartData = calculatePieChartData(summaries);
            plotSummariesPieChart(pieChartData);
        })

    });
}

async function createFlags(patientResources){
    let flags = []
    for (let i = 0; i < patientResources.length; i++){
        //TODO execute all these simultaneously, currently n RTTs.
        let flag = await calculateFlag(patientResources[i]);
        flags.push(flag);
    }
    return flags;
}

async function calculateFlag(resource){
    //TODO inject parameters
    
    let id = resource.id

    //get all QRs 
    //TODO could maybe use only last 2 forms here, probably still use cached data from table call
    let QRs = await tempGetQRResNoCache(id);//TODO use cached version IMPORTANT
    if (QRs){
        //console.log(QRs)
        let dates = Object.keys(QRs)
        var minDate = dates.reduce(function (a, b) { return a < b ? a : b; }); 
        var maxDate = dates.reduce(function (a, b) { return a > b ? a : b; });

        //TODO for now just use max latest measurement, implment delta checks later
        let lastQR = QRs[maxDate];
        let maxKey = Object.keys(lastQR).reduce((a, b) => lastQR[a] > lastQR[b] ? a : b);

        return maxKey;
    }
    else{
        return "";
    }
    
}

function calculatePieChartData(summaries){
    let steady = 0;
    let improving = 0;
    let declining = 0;

    summaries.forEach(str => {
        //switch?
        if (str === "Declining"){
            declining++;
        }
        else if (str === "Improving"){
            improving++;
        }
        else if (str === "Steady"){
            steady++;
        }
    })

    let data = [{
        name: 'Steady',
        y: steady,
        sliced: true,
        selected: true
    }, {
        name: 'Improving',
        y: improving
    }, {
        name: 'Declining',
        y: declining
    }]
    
    return data
    
}

function buildTable(summaries, patNames, patIDs, flags){
    const table = document.getElementById("masterTableBody");
    table.innerHTML = ""
    let listItems = "";
    for (let i = 0; i < patNames.length; i++){
        var currentHTML = `
            <tr class="table-active">
                <td scope="row">
                    <span class="normalText">${patNames[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">${patIDs[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">${flags[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">${summaries[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">NYI</span>
                </td>
                <td scope="row">
                    <span class="normalText">NYI</span>
                </td>
            </tr>
        `
        listItems += currentHTML;
    }
    table.innerHTML = listItems
}

async function createSummaries(patientResources){
    let summaries = []
    console.log("fetching QRs for summaries")
    for (let i = 0; i < patientResources.length; i++){
        //TODO execute all these simultaneously, currently n RTTs.
        let sum = await getPatientSummary(patientResources[i]);
        summaries.push(sum);
    }
    return summaries;
}


async function getPatientSummary(resource){
    //get patient ID
    let id = resource.id

    //get all QRs
    let QRs = await tempGetQRResNoCache(id);
    if (!QRs){
        return "No forms"
    }

    //process QRs
    //TODO FIX/IMRPOVE
    let dates = Object.keys(QRs)
    var minDate = dates.reduce(function (a, b) { return a < b ? a : b; }); 
    var maxDate = dates.reduce(function (a, b) { return a > b ? a : b; });

    
    let firstQR = QRs[minDate]; //can not assume order is kept in json
    //maybe search through dates
    let cumulativeFirst = 0
    Object.entries(firstQR).forEach(entry =>{
        let val = entry[1]
        cumulativeFirst += val
    })

    let lastQR = QRs[maxDate];
    let cumulativeLast = 0
    Object.entries(lastQR).forEach(entry =>{
        let val = entry[1]
        cumulativeLast += val
    })

    //TODO inject parameters for this calculation
    let totalChange = cumulativeFirst - cumulativeLast;
    if (totalChange <= 48){
        return "Declining"
    }
    else if (totalChange > 48 && totalChange < 50){
        return "Steady"
    }
    else{
        return "Improving"
    }
    return totalChange
}

//TODO refactor out
async function getPatients(){
    let allPatients = getPatientResources(limit=10);
    return allPatients;
}

function enableTableSort() {
    //adapted from https://stackoverflow.com/questions/14267781/sorting-html-table-with-javascript
    const getCellValue = (tr, idx) => tr.children[idx].innerText || tr.children[idx].textContent;

    const comparer = (idx, asc) => (a, b) => ((v1, v2) =>
        v1 !== '' && v2 !== '' && !isNaN(v1) && !isNaN(v2) ? v1 - v2 : v1.toString().localeCompare(v2)
    )(getCellValue(asc ? a : b, idx), getCellValue(asc ? b : a, idx));

    //TODO doesn't work for the first element in the table
    document.querySelectorAll('th').forEach(th => th.addEventListener('click', (() => {
        const table = document.getElementById("masterTable");
        Array.from(table.tBodies[0].querySelectorAll('tr:nth-child(n+1)'))
            .sort(comparer(Array.from(th.parentNode.children).indexOf(th), this.asc = !this.asc))
            .forEach(tr => table.tBodies[0].appendChild(tr));
    })));
}

function initSearch(){
    $(function(){
      $("#patientFilter").on("keyup", function() {
        var value = $(this).val().toLowerCase();
        $("#masterTableBody tr").filter(function() {
          $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
      });
    });
}

function clearFilter() {
    $(function(){
        $("#masterTableBody tr").filter(function() {
            this.style.display = "table-row"
        });
    });
}

function filterDone() {
    $(function(){
        var threshold = 7;
        $("#masterTableBody tr").filter(function() {
            var colSixVal = this.children[6]
            var lastCheckedCurrent = parseInt(colSixVal.innerText[0])
            $(this).toggle(lastCheckedCurrent > threshold)
        });
    });
}

function filterNotDone() {
    $(function(){
        var threshold = 7;
        $("#masterTableBody tr").filter(function() {
            var colSixVal = this.children[6]
            var lastCheckedCurrent = parseInt(colSixVal.innerText[0])
            $(this).toggle(lastCheckedCurrent <= threshold)
        });
    });
}

initTable();