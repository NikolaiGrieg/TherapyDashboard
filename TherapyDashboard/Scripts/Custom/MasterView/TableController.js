function initTable() {
    initFHIRData(); 
    enableTableSort();
    initSearch();
}


async function initFHIRData(){
    /*
    getPatientResources().then(results => {
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
    */
    let patIDs = Object.keys(summaries);
    let summaryStrings = Object.values(summaries);
    buildTable(summaryStrings, patIDs);


    let pieChartData = calculatePieChartData(summaryStrings);
    plotSummariesPieChart(pieChartData);
}

function calculatePieChartData(summaries){
    let steady = 0;
    let improving = 0;
    let declining = 0;

    summaries.forEach(str => {
        //switch?
        if (str === "declining"){
            declining++;
        }
        else if (str === "improving"){
            improving++;
        }
        else if (str === "steady"){
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

function buildTable(summaries, patIDs){
    const table = document.getElementById("masterTableBody");
    table.innerHTML = ""
    let listItems = "";

    //TODO some matching between IDs, and not use index
    for (let i = 0; i < patIDs.length; i++){
        var currentHTML = `
            <tr class="table-active">
                <td scope="row">
                    <span class="normalText">_Name</span>
                </td>
                <td scope="row">
                    <span class="normalText">${patIDs[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">_Flag</span>
                </td>
                <td scope="row">
                    <span class="normalText">${summaries[i]}</span>
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