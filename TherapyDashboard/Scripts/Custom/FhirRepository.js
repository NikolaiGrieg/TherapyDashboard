//TODO make prototype, initialize from controller?

//TODO RENAME, this is the new controller
//TODO refactor to one file for handling data on specific, and one for master
//TODO refactor out QR handling
var data = []
var config = {
    serviceUrl: "http://localhost:8080/hapi/baseDstu3", //"http://ec2-54-93-230-9.eu-central-1.compute.amazonaws.com/baseDstu3",
    auth: {
      type: 'none'
    }
};
var smart = FHIR.client(config);
var tempCurrentPatient = ["325"];  

//TODO maybe encapsulate this in class
async function getQRResources(patientID){
    let results = await smart.api.fetchAllWithReferences({ 
        type: "QuestionnaireResponse", query: {
            patient : patientID 
        }
    });
    let QRResources = await pageChainSearch(results);
    let processedQRResource = wrangleQR(QRResources);
    return processedQRResource;
}

//TODO do these methods on backend instead, and return with model.
var QRResourceData;
async function getQRRForAllPatients(){
    //possible filter here for therapist access
    if (!QRResourceData){
        console.log("fetcing all QRs")
        QRResourceData = [];
        let results = await getPatientResources();
            
        for (let i = 0; i < results.length; i++){
            let resource = results[i];
            let QRs = await getQRResources(resource.id);
            QRResourceData.push(QRs);
        }
        return QRResourceData
    }
    else{
        console.log("using cached QRs")
        return QRResourceData;
    }
    
}


async function tempGetQRResNoCache(patientID){
    let results = await smart.api.fetchAllWithReferences({ 
        type: "QuestionnaireResponse", query: {
            patient : patientID 
        }
    });
    let QRResources = await pageChainSearch(results);
    if (QRResources){
        let processedQRResource = wrangleQR(QRResources);
        return processedQRResource;
    }
    return undefined;
}

var patientResourceData;
async function getPatientResources(limit=10){
	if (!patientResourceData){
        console.log("fetching patients")
		let results = await smart.api.fetchAllWithReferences({ //TODO difference between this and api.search?
	        type: "Patient"
	    });
	    let allPatients = await pageChainSearch(results, limit)
        console.log("patients fetched")
	    patientResourceData = unpackBundleArray(allPatients);
	    return patientResourceData;
	}
	else{
		return patientResourceData;
	}
}

///accepts a resource bundle, and returns a list of all bundles in search pages
//should work for generic resources
//default limit is 100 pages = 1000 resources
async function pageChainSearch(results, limit=100){

    if (results.data.total === 0){ //no forms found
        return undefined
    }
    let intermediateResultList = [results];
    let nextPageUrl = getNextUrl(results, true)

    //get all remaining resource pages
    while (nextPageUrl){
        //undocumented function in 
        //https://raw.githubusercontent.com/smart-on-fhir/client-js/master/dist/fhir-client.js
        let results = await $.getJSON(nextPageUrl);
        intermediateResultList.push(results)
        if(intermediateResultList.length < limit){
        	nextPageUrl = getNextUrl(results, false);
        }
        else{
        	nextPageUrl = undefined;
        }
    }
    return intermediateResultList;
}

//TODO better name
function wrangleQR(intermediateResources){
	//wrangle into d3 accepted format
    if (intermediateResources){
        let intermediateResultList = unpackBundleArray(intermediateResources);
        let timeDict = wrangleFhirQRToTimeSeries(intermediateResultList);

        let processedResults = {};
        Object.entries(timeDict).forEach(resource => {
            let date = resource[0];
            let val = resource[1];
            processedResults[date] = val;
        })
        //console.log(processedResults)
        return processedResults;
    }
}

//called from singletonhandlers, accepts input as returned from pageChainSearch
function unpackBundleArray(bundles){
	let intermediateResultList = []
	for (let i = 0; i < bundles.length; i++){
		let unpackedRes;
		if (i === 0){
			unpackedRes = unpackBundle(bundles[i], true)
		}
		else{
			unpackedRes = unpackBundle(bundles[i], false)
		}
		unpackedRes.forEach(res =>{
			intermediateResultList.push(res)
		})
		
	}
	return intermediateResultList;
}

function wrangleFhirQRToTimeSeries(resources){
    var series = {};
    resources.forEach(resource => {
        let timeDict = QRResourceToTimeDict(resource);
        let date = Object.keys(timeDict)[0]
        series[date] = timeDict[date]
    })
    return series;
}

//used in pageChainSearch
function getNextUrl(resource, first){ //can return undefined
    let links;
    if(first){
        links = resource.data.link;
    }
    else {
        links = resource.link;
    }
    for (let i = 0; i < links.length; i++){
        if (links[i].relation === "next") {
            return links[i].url;
        }
    }
}

/// Gets all QR resources (all pages) for the current patient, and wrangles it into chart data
//TODO maybe load data initially (async), now it loads on button (composite kat) click
//TODO possibly get all pages async? not sure how to get the links
function initSpider(){
    getQRResources(tempCurrentPatient).then(timeSeries => {
        //console.log(timeSeries)
        createSpiderChart(timeSeries);
    });
}

//TODO not call this at master view

function initDetailView(){

    getQRResources(tempCurrentPatient).then(results =>{
        initQRLineCharts(results);
    });
    
    /*
    smart.api.search({type: "Patient"}).then(results =>{
        console.log(results)
        //TODO get actual patient instead of first
        let pat = results.data.entry[0];

        //initBackground(pat.resource)
    });
    */
    
    
    smart.api.fetchAllWithReferences({ 
        type: "Observation", query: {
            patient : tempCurrentPatient //specific patient for now
        }
    }).then(function (results, refs) {
        //TODO get all patients for main page
        //console.log(results)
        results.data.entry.forEach(obs =>{
            //console.log(obs)
            obs = obs.resource;
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
            
            data.push(entry)
        });
        filterFhirData(data);
    });
}

function unpackBundle(bundle, first){
    var resources = []
    if (first){
        bundle.data.entry.forEach(listItem => {
            resources.push(listItem.resource);
        });
    }
    else{
        bundle.entry.forEach(listItem => {
            resources.push(listItem.resource);
        });
    }
    return resources;
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
//TODO get background information from patient
//available data should contain: Age, gender, maritalStatus, language(?), missing module, children, education
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

    let listItems = "";
    let keys = Object.keys(patient);

    let filter = ["name", "gender", "birthDate", "telecom"];

    for (let i = 0; i < keys.length; i++){
        if(filter.includes(keys[i])){
            let curVal = patient[keys[i]]
            var currentHTML = `
                <tr class="table-active">
                    <th scope="row">
                        <span class="normalText">${keys[i]}</span>
                    </th>
                    <td scope="row">
                        <span class="normalText">${curVal}</span>
                    </td>
                </tr>
            `
            listItems += currentHTML;
        }
    }
    

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