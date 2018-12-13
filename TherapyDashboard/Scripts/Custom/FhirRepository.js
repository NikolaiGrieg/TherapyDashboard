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
		
		//Get all questionnaire responses
		smart.api.search({type: "QuestionnaireResponse"}).then(results =>{
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
	        fhirCharts(data);
	    });
	});
})

function initSelectors(measurements){
	let names = Object.keys(measurements);

	var container = document.getElementById("observationsContainer");

	var listItems = "";

	for (let i = 0; i < names.length; i++){
		let curMeasurement = filteredMeasurements[names[i]];
		let numObs = Object.keys(curMeasurement).length;

		var currentHTML = `
			<li>
			    <a href="#" onclick="createChartSelector(this)" id="selectorFor${names[i]}">${names[i]} (${numObs})</a>
			</li>
		`
		listItems += currentHTML;
	}

	//TODO have button take the name of clicked observation
	var html = `
			<div class="btn-group">
			<button type="button" class="btn btn-info btn-block dropdown-toggle" 
			data-toggle="dropdown" id="observationSelectorBtn" aria-haspopup="true" aria-expanded="false">
			Select Observation
			<span class="caret"></span>
			</button> 
			<ul class="dropdown-menu" style="position: absolute;"> 
			${listItems}
			</ul> 
			</div>
			`;
	

	var observationsDropdown = document.createElement("div");
	observationsDropdown.innerHTML = html;
	container.appendChild(observationsDropdown);

}

function createChartSelector(obsSelector) {
	let name = obsSelector.id.replace("selectorFor", "");
	var container = document.getElementById("observationsContainer");

	//Logic for determining chart options go here

	//create chart selector
	var selectorContainer = document.createElement("div");
	selectorContainer.id = "selectorContainerFor" + name;

	var html = `
			<div class="btn-group">
			<button type="button" class="btn btn-info btn-block dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
			Select Chart
			<span class="caret"></span>
			</button> 
			<ul class="dropdown-menu"> 
			<li>
			    <a href="#" onclick="lineChart(this)" id="lineFor${name}">Line</a>
			</li> 
			<li>
			    <a href="#" onclick="frequencyChart(this)" id="frequencyFor${name}">Frequency</a>
			</li> 
			<li>
			    <a href="#" onclick="eventChart(this)" id="eventFor${name}">Events</a>
			</li>  
			<li>
			    <a href="#" onclick="disableChart(this)" id="removeFor${name}">None</a>
			</li> 
			</ul> 
			</div>
			`;

	selectorContainer.innerHTML = html;
	container.appendChild(selectorContainer);

	changeSelectedObservationBtnText(name);
}

function changeSelectedObservationBtnText(text){
	let btn = document.getElementById("observationSelectorBtn");
	//let curText = btn.innerText;
	//console.log(curText);

	btn.innerHTML = text + `<span class="caret"></span>`;
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

function lineChart(a) {
	let name = a.id.replace("lineFor", "");
	var linechart;
	if(Object.keys(disabledCharts).includes(name)){
		linechart = disabledCharts[name];
		var parent = document.getElementById("line");
		parent.appendChild(linechart);
	}
	else{
		linechart = new LineChart("#line", this, name, 'all', filteredMeasurements[name], 200, 700, fhir=true);
	}

	//clean up
	//destroy selector button
	let selector = document.getElementById("selectorContainerFor" + name);
	selector.parentNode.removeChild(selector);

	changeSelectedObservationBtnText("Select Chart")
	
}

function frequencyChart(a){
	//TODO
}

function eventChart(a){
	//TODO
}

var disabledCharts = {}

function disableChart(a){
	//TODO just set inactive, don't actually remove
	let name = a.id.replace("removeFor", "")
	let divName = name.replace(/\s/g, ''); //remove spaces to match correct div
	let containerName = "lineChart" + divName
	var chart = document.getElementById(containerName);

	disabledCharts[name] = chart;

	var parent = chart.parentNode;
	parent.removeChild(chart);

	//destroy selector button
	let selector = document.getElementById("selectorContainerFor" + name);
	selector.parentNode.removeChild(selector);

	changeSelectedObservationBtnText("Select Chart")
}

var filteredMeasurements = {};
//TODO get background information from patient
//available data should contain: Age, gender, maritalStatus, language(?), missing module, children, education
function fhirCharts(data){

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
