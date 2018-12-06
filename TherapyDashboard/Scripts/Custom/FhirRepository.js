//TODO make prototype, initialize from controller?

$(function fhirData(){
	var data = []
	FHIR.oauth2.ready(function (smart) {
		
		//Get BMI observations for specified patient
		smart.api.search({type: "Observation", query: {
			code : ["39156-5"],
			patient : ["42ab2ed1-eb1d-4501-be82-642e11538eac"]
			}
		}).then(function(results, refs){
			console.log(results)
		});
		
		//Get all questionnaire responses
		smart.api.search({type: "QuestionnaireResponse"}).then(function(results, refs){
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
	        fhirCharts(data)
	    });
	});
})

function fhirCharts(data){

	//TODO iterate over all measurements
	var cleaned = []
	var measurement = data[0].measurement
	for(let i = 0; i < data.length; i++){
		let observation = data[i];
		if (observation.quantity){
			//console.log(observation)
			cleaned.push(observation)
			/*
			if (!names.includes(name)){
				names.push(name);
			}
			*/
		}
	}
	

	var categorized = {}

	for(let i = 0; i < cleaned.length; i++){
		if (cleaned[i].measurement === measurement){
			let dataPoint = cleaned[i];
			let time = dataPoint.time
			let quantity = dataPoint.quantity

			time = time.slice(0, time.length - 6)
			time = time.replace("T", " ");
			

			categorized[time] = quantity;
			
		}
	}

	linechart = new LineChart("#line", this, measurement, 'all', categorized, 200, 700, fhir=true);
}
