//TODO make prototype, initialize from controller?

//No auth connection

/*
smart.api.search({type: "Patient"}).then(results =>{
	console.log(results)
});
*/


$(function fhirData(){
	var data = []
	var config = {
		serviceUrl: "http://ec2-54-93-230-9.eu-central-1.compute.amazonaws.com/baseDstu3",
		auth: {
		  type: 'none'
		}
	};
	var smart = FHIR.client(config);

		
	/*
	//Get BMI observations for specified patient
	smart.api.search({type: "Observation", query: {
		code : ["39156-5"],
		patient : ["42ab2ed1-eb1d-4501-be82-642e11538eac"]
		}
	}).then(function(results, refs){
		console.log(results)
	});
	*/
	
	/*
	//Get all questionnaire responses
	smart.api.search({type: "QuestionnaireResponse"}).then(results =>{
		console.log(results)
	});
	*/
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
    		patient : ["47994"] //specific patient for now
    	}
	}).then(function (results, refs) {
    	//TODO get all patients for main page
    	console.log(results)
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
})


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
	//console.log(measurements)

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