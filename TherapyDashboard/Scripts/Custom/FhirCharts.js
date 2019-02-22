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

function createSpiderChartSelectors(chartNames){
	//build HTML
	var resHTML = "";
	chartNames.forEach(name => {
		let button = `	<button type="button" class="btn btn-primary" 
	                    onclick="initSpider('${name}')">
	                    ${name}
	                </button>
                            `
        resHTML += button;
	})

	//insert HTML
	var containerDiv = document.createElement("div");
	containerDiv.innerHTML = resHTML;

	var container = document.getElementById("observationsContainer");
	container.appendChild(containerDiv);
}

function createChartSelector(obsSelector) {
	let name = obsSelector.id.replace("selectorFor", "");
	var container = document.getElementById("observationsContainer");

	//Logic for determining local chart options go here

	//create chart selector
	let prev = document.getElementById("selectorContainer");
	if (prev){
		prev.parentNode.removeChild(prev); //remove previous selection
	}
	var selectorContainer = document.createElement("div");
	selectorContainer.id = "selectorContainer";

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

	//clean up TODO extract this
	//destroy selector button
	let selector = document.getElementById("selectorContainer");
	selector.parentNode.removeChild(selector);

	changeSelectedObservationBtnText("Select Observation")
	
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
	let containerName = "lineChart" + divName //TODO more generic names so it works on freq/event
	var chart = document.getElementById(containerName);

	if(chart){
		disabledCharts[name] = chart;

		var parent = chart.parentNode;
		parent.removeChild(chart);

	}

	let selector = document.getElementById("selectorContainer");
	selector.parentNode.removeChild(selector);

	changeSelectedObservationBtnText("Select Observation")
	
}