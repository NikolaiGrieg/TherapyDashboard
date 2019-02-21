
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


function initSpider(){
    createSpiderChart(processedQRResources);
}

//TODO not call this at master view
var processedQRResources;
function initDetailView(){

    let QRs = parseJsonFromStringArray(_QRList);
    
    //group QRs based on Questionnaire
    let groupedQRList = groupQRs(QRs)

    if (QRs){
        processedQRResources = wrangleFhirQRToTimeSeries(QRs); //this overwrites if time already exists, TODO handle
        initQRLineCharts(processedQRResources);
    }
    

    let patient = JSON.parse(_patient);
    renderPatient(patient);
    initBackground(patient);
    //console.log(patient);

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
    filterFhirData(data);
}

function groupQRs(QRs){
    /*
    let QRList = []
    QRs.forEach(QR => {
        //get Q
        console.log(QR)
        let QID = QR.questionnaire.reference;

        //find if Q in QRList

        //upsert
    })
    */
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

                time = time.slice(0, time.length - 6)
                time = time.replace("T", " ");
                
                categorized[time] = quantity;
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
    console.log(patient)
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
    <table class="table table-hover table-striped">
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

function unpackPatientData(patient, filter){
    var entries = {}
    Object.entries(patient).forEach(kvp =>{
        let key = kvp[0];
        let val = kvp[1];
        if (filter.includes(key)){
            //console.log(kvp)
            let entry;
            //Switch?
            if (key == "name"){
                entry = val[0].given[0] + " " + val[0].family;
            }
            else if (key == "telecom"){
                entry = val[0].system + ": " + val[0].value;
            }
            else if(key == "maritalStatus"){ //see https://www.hl7.org/fhir/v3/MaritalStatus/cs.html
                entry = val.text;
            }
            else{
                entry = val;
            }
            entries[key] = entry;
        }
    })
    return entries;
}