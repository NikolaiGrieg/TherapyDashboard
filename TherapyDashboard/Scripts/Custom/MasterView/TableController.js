function initTable() {
    initFHIRData(); 
    enableTableSort();
    initSearch();
}


async function initFHIRData(){
    let patIDs = Object.keys(_summaries);
    let summaryStrings = Object.values(_summaries);
    let patNames = Object.values(_patientNames);
    buildTable(patNames, summaryStrings, patIDs);


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

function buildTable(patNames, summaries, patIDs){
    const table = document.getElementById("masterTableBody");
    table.innerHTML = ""
    let listItems = "";

    //TODO some matching between IDs, and not use index
    for (let i = 0; i < patIDs.length; i++){
        var currentHTML = `
            <tr class="table-active">
                <td scope="row">
                    <a href="Patient/${patIDs[i]}">
                        <span class="normalText">${patNames[i]}</span>
                    </a>
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