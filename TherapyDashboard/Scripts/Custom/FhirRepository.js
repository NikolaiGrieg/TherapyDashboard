//TODO make prototype, initialize from controller?

$(function fhirData(){
	var data = []
	FHIR.oauth2.ready(function (smart) {
	    smart.patient.api.fetchAllWithReferences({ type: "Observation" }, ["Body Mass Index"]).then(function (results, refs) {
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
	var cleaned = []
	var name = data[0].name
	for(let i = 0; i < data.length; i++){
		let observation = data[i];
		if (observation.quantity){
			//console.log(observation)
			cleaned.push(observation)
			if (!names.includes(name)){
				names.push(name);
			}
		}
	}
	
	var categorized = []

	for(let i = 0; i < cleaned.length; i++){
		if (data[i] === name){
			let dataPoint = data[i];
			let time = dataPoint.time
			let quantity = dataPoint.quantity
			let entry = {
				time : quantity
			}
		}
	}

	linechart = new LineChart("#line", this, "AAAAA", 'all', categorized);
}
