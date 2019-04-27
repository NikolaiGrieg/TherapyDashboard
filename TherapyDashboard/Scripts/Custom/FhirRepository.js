
var data = []

function wrangleFhirQRToTimeSeries(resources){
    var series = {};
    resources.forEach(resource => {
        let timeDict = QRResourceToTimeDict(resource);
        let date = Object.keys(timeDict)[0]
        series[date] = timeDict[date]
    })
    return series;
}

function initSpider(chartName){
    let QRs = groupedQRList[chartName]
    let processedQRResources = wrangleFhirQRToTimeSeries(QRs);
    createSpiderChart(processedQRResources, chartName);
}


var groupedQRList = {}
function initDetailView(){
    let QRs = parseJsonFromStringArray(_QRList);
    //group QRs based on Questionnaire

    QRs.forEach(QR => {
        let qid = QR.questionnaire.reference.slice(14); //remove "Questionnaire/"
        let name = _qMap[qid];
        //console.log(name)
        //if name not already key
        if (Object.keys(groupedQRList) === undefined || !Object.keys(groupedQRList).includes(name)){
            if (name){
                groupedQRList[name] = [QR]
            }
        }
        else{
            let bucket = groupedQRList[name];
            bucket.push(QR)
        }
    })
    //console.log(groupedQRList);

    Object.entries(groupedQRList).forEach(kvp => {
        let qName = kvp[0]
        let QRs = kvp[1]
        
        if(QRs.length > 1){
            let processedQRResources = wrangleFhirQRToTimeSeries(QRs); //this overwrites if time already exists, TODO handle
            initQRLineChart(processedQRResources, qName);
        }
    })
    

    let patient = JSON.parse(_patient);
    renderPatient(patient);
    initBackground(patient);

    let observations = []
    _observations.forEach(str => {
        let obs = JSON.parse(str);
        observations.push(obs);
    })

    observations.forEach(obs =>{
        let entry;
        if (obs.component){
            entry = {
                patient : obs.subject.reference,
                measurement : obs.code.coding[0].display,
                time : obs.effectiveDateTime,
                component : obs.component,
            }
        }
        else if (obs.valueQuantity){ //TODO FIX for valueInteger++
            entry = {
                patient : obs.subject.reference,
                measurement : obs.code.coding[0].display,
                time : obs.effectiveDateTime,
                quantity : obs.valueQuantity.value
            }
        }
        if(entry){
            data.push(entry) 
        }
    })
    createSpiderChartSelectors(Object.keys(groupedQRList))
    filterFhirData(data);
    initPersistedCharts(_persistedCharts);
}

function initPersistedCharts(chartNames){
    console.log(chartNames)
    Object.entries(chartNames).forEach(kvp=> {
        let name = kvp[0];
        let type = kvp[1];

        if (type === 'observation'){
            lineChart(name, false);
        }
        else if (type === 'QRAxis'){
            let parent = '#line';

            //find QR data
            let axisData = null;
            let QRNames = Object.keys(groupedQRList);
            QRNames.forEach(key=> {
                let procQRs = wrangleFhirQRToTimeSeries(groupedQRList[key]);
                let firstQR = Object.values(procQRs)[0];
                Object.keys(firstQR).forEach(category => {
                    if(category.startsWith(name) || name.startsWith(category)){
                        axisData = procQRs;
                        return;
                    }
                })
                if (axisData != null){
                    return;
                }
            })

            selectAxis(parent, name, axisData, false)
        }
        else{
            console.log("unexpected type: "+ type)
        }
        
    })
}

function QRResourceToTimeDict(resource){
    let date = resource.authored;
    let form = {};
    resource.item.forEach(listItem => {
        let value = listItem.answer[0].valueInteger;
        let category = listItem.text;

        form[category] = value;
    })
    let timeDict = {
        [date]: form
    }
    return timeDict;
}


const getMeasurementNames = data =>{
    let measurements = []
    for (let i = 0; i < data.length; i++){
        if (!measurements.includes(data[i].measurement)){
            measurements.push(data[i].measurement)
        }
    }
    return measurements
}

var filteredMeasurements = {};
function filterFhirData(data){

    //Find unique measurements
    let measurements = getMeasurementNames(data);

    for (let i = 0; i < measurements.length; i++){
        var cleaned = []
        var measurement = measurements[i]

        //TODO have this in linear time instead
        for(let j = 0; j < data.length; j++){
            let observation = data[j];
            if (observation.quantity){
                //console.log(observation)
                cleaned.push(observation)
            }
        }
        var categorized = {}

        for(let j = 0; j < cleaned.length; j++){
            if (cleaned[j].measurement === measurement){
                let dataPoint = cleaned[j];
                let time = dataPoint.time
                let quantity = dataPoint.quantity

                let dt = new Date(time);
                let timeStr = dt.getFullYear() + "-" + (dt.getMonth()+1) + "-" + dt.getDate(); 

                categorized[timeStr] = quantity;
            }
        }

        //remove observations with 1 data point
        if (Object.keys(categorized).length > 1){
            filteredMeasurements[measurement] = categorized;
            //let linechart = new LineChart("#line", this, measurement, 'all', categorized, 200, 700, fhir=true);
        }
        
    }
    initSelectors(filteredMeasurements);
}

//This function works somewhat (missing some unpacking), but the returned FHIR data doesnt appear to be very complete
function initBackground(patient){
    //console.log(patient)
    let listItems = "";
    let filter = ["name", "gender", "birthDate", "telecom", "maritalStatus"]; //should come from backend

    let data = unpackPatientData(patient, filter);
    //console.log(data);

    Object.entries(data).forEach(kvp => {
        var currentHTML = `
            <tr class="table-active">
                <th scope="row">
                    <span class="normalText">${kvp[0]}</span>
                </th>
                <td scope="row">
                    <span class="normalText">${kvp[1]}</span>
                </td>
            </tr>
        `
        listItems += currentHTML;
    })
    

    var html = `
    <table class="table table-hover table-striped table-fit">
        <tbody>
            ${listItems}  
        </tbody>
    </table>
    `

    var backgroundTable = document.createElement("div");
    backgroundTable.innerHTML = html;

    let container = document.getElementById("background");
    container.appendChild(backgroundTable);
}

//TODO clean up outputs
function unpackPatientData(patient, filter){
    var entries = {}
    Object.entries(patient).forEach(kvp =>{
        let key = kvp[0];
        let val = kvp[1];
        if (filter.includes(key)){
            let entry;
            //Switch?
            if (key == "name"){
                entry = val[0].given[0] + " " + val[0].family;
                entries['Name'] = entry;
            }
            else if (key == "telecom"){
                entry = val[0].system + ": " + val[0].value;
                entries['Telecom'] = entry;
            }
            else if(key == "maritalStatus"){ //see https://www.hl7.org/fhir/v3/MaritalStatus/cs.html
                entry = val.text;
                entries['Marital Status'] = entry;
            }
            else{
                entry = val;
                let displayKey = key.charAt(0).toUpperCase() + key.slice(1) //capitalize first letter
                    .split(/(?=[A-Z])/).join(" "); //add spaces between capitalized words

                entries[displayKey] = entry;
            }
        }
    })
    return entries;
}