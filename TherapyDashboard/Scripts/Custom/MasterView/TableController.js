function initTable() {
    initFHIRData(); 
    enableTableSort();
    initSearch();
}


function initFHIRData(){
    let patIDs = Object.keys(_summaries);
    let summaryStrings = Object.values(_summaries);
    let flagStrings = Object.values(_flags);
    let patNames = Object.values(_patientNames);
    let lastChecked = wrangleLastChecked(_lastChecked);

    buildTable(patNames, summaryStrings, flagStrings, patIDs, lastChecked);

    let warningIDs = Object.keys(_warnings);
    let warningParams = Object.values(_warnings);
    let warningNames = []

    //TODO is this safe? potential desync between which patient has which params
    warningIDs.forEach(id =>{
        warningNames.push(_patientNames[id])
    })
    renderWarnings(warningNames, warningIDs, warningParams);

    let pieChartData = calculatePieChartData(summaryStrings);
    plotSummariesPieChart(pieChartData);
}

function wrangleLastChecked(_lastChecked, humanReadable=true){
    let dateMap = {}
    Object.entries(_lastChecked).forEach(kvp => {
        
        let patID = kvp[0];
        let dateStr = kvp[1]
                .replace(/\D/g,''); //remove non numerical symbols

        let date = new Date(parseInt(dateStr));
        if(humanReadable){
            let readableDate = dateToHumanReadable(date);
            dateMap[patID] = readableDate;
        }
        else{
            dateMap[patID] = date;
        }
        
    })
    
    return dateMap;
}

//adapted from https://stackoverflow.com/questions/2627473/how-to-calculate-the-number-of-days-between-two-dates
function getDiffDays(date){
    var oneDay = 24*60*60*1000; // hours*minutes*seconds*milliseconds
    var firstDate = date;
    var secondDate = new Date();

    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime())/(oneDay)));
    return diffDays;
}

//TODO test, appears to be correct
function dateToHumanReadable(date){
    var diffDays = getDiffDays(date);

    let daysStr;
    if(diffDays == 0){
        daysStr = "Today"
    }
    else if (diffDays < 7){
        daysStr = diffDays + " days ago";
    }
    else{
        diffWeeks = diffDays / 7
        daysStr = diffWeeks + "weeks ago";
    }
    return daysStr;
}

//TODO handle vertical overflow
//TODO: sort depending on severity (highest number?)
//TODO make this generic to work with flags as well
function renderWarnings(patNames, patIDs, parameters){
    const table = document.getElementById("warningTable");
    table.innerHTML = ""
    let listItems = "";

    //TODO some matching between IDs, and not use index
    for (let i = 0; i < patIDs.length; i++){
        var currPatNameHTML = `
            <tr class="table-active">
                <td scope="row">
                    <a href="Patient/${patIDs[i]}">
                        <span class="normalText">${patNames[i]}</span>
                    </a>
                </td>
                `
        var currPatWarningParamsHTML = "";
        if (parameters[i].length === 1){
            currPatWarningParamsHTML = `
                <td scope="row">
                    <span class="normalText">${parameters[i]}</span>
                </td>
            </tr>
        `
        }
        else {
            //create list item for each parameter
            let currParamsListHTML = "";
            parameters[i].forEach(param => {
                let paramHTML = `
                <tr class="table-active">
                    <td scope="row">
                        <span class="normalText">${param}</span>
                    </td>
                </tr>
                `
                currParamsListHTML += paramHTML;
            })

            // integrate list in popup box
            currPatWarningParamsHTML = `
                <td scope="row" class="popupTrigger">
                    <span class="normalText">${parameters[i].length} warnings</span>
                    <div class="popup">
                        <table class="table table-hover table-striped">
                            <tbody>
                                ${currParamsListHTML}
                            </tbody>
                        </table>
                    </div>
                </td>
            </tr>
        `
        }
        
        currentHTML = currPatNameHTML + currPatWarningParamsHTML;
        listItems += currentHTML;
    }
    table.innerHTML = listItems;
    addPopupListener();
}

//adapted function from https://stackoverflow.com/questions/3559467/description-box-using-onmouseover
function addPopupListener(){
    $(".popupTrigger").mouseover(function() {
        $(this).children(".popup").show();
    }).mouseout(function() {
        $(this).children(".popup").hide();
    });
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

function buildTable(patNames, summaries, flags, patIDs, lastChecked){
    const table = document.getElementById("masterTableBody");
    table.innerHTML = ""
    let listItems = "";

    //TODO some matching between IDs, and not use index
    for (let i = 0; i < patIDs.length; i++){

        let lastCheckedCurrent = "Never"
        if(Object.keys(lastChecked).includes(patIDs[i])){
            lastCheckedCurrent = lastChecked[patIDs[i]];
        }

        var currentHTML = `
            <tr class="table-active">
                <td scope="row">
                    <a href="Patient/${patIDs[i]}">
                        <span class="normalText">${patNames[i]}</span>
                    </a>
                </td>
                <td scope="row">
                    <span class="normalText">_Module</span>
                </td>
                <td scope="row">
                    <span class="normalText">${flags[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">${summaries[i]}</span>
                </td>
                <td scope="row">
                    <span class="normalText">${lastCheckedCurrent}</span>
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

function _sortOnDate(){
    let lastChecked = wrangleLastChecked(_lastChecked);
    //TODOD impl
}

function clearFilter() {
    $(function(){
        $("#masterTableBody tr").filter(function() {
            this.style.display = "table-row"
        });
    });
}

function applyDateFilter(isDone){
    clearFilter();
    let lastChecked = wrangleLastChecked(_lastChecked, false);
    let threshold = 7;

    //get list of lastChecked within 7 days
    let filteredPatients = [];
    Object.entries(lastChecked).forEach(kvp =>{
        let patID = kvp[0];
        let date = kvp[1];
        let diffDays = getDiffDays(date);
        if (diffDays < threshold){
            let patName = _patientNames[patID]; //global variable.. 
            filteredPatients.push(patName);
        }
    })

    //update gui
    $(function(){
        $("#masterTableBody tr").filter(function() {
            var patientColumn = this.children[0]
            var patNameCurrent = patientColumn.innerText;
            if(isDone){
                $(this).toggle(!filteredPatients.includes(patNameCurrent));
            }
            else{
                $(this).toggle(filteredPatients.includes(patNameCurrent));
            }
        });
    });
}

function filterDone() {
    applyDateFilter(true);
}


function filterNotDone() {
    applyDateFilter(false);
}

initTable();