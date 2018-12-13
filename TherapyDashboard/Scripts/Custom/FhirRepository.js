//TODO make prototype, initialize from controller?

$(function fhirData(){
	var data = []
	FHIR.oauth2.ready(function (smart) {
		
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

		smart.api.search({type: "Patient"}).then(results =>{
			console.log(results)
		});
		
	    smart.api.fetchAllWithReferences({ type: "Observation" }).then(function (results, refs) {
	    	//TODO get all patients for main page
	        results.forEach(function (obs) {
	            //console.log(obs)
	            var entry;
	            if (obs.component){
	                entry = {
	                    patient : obs.subject.reference,
	                    measurement : obs.code.coding[0].display,
	                    time : obs.effectiveDateTime,
	                    component : obs.component,
	                }
	            }
	            else{
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
