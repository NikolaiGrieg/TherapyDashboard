//console.log(dataPath);
//TODO change name, this is no longer a controller

//called from FhirRepository
//Currently not possible to persistently remove, TODO fix
function initQRLineChart(resources, name){
	linechart = new LineChart("#line", this, name, 'all', resources);
	initRemovalFunctionality(name, false, 'QRSummary')
}

function update(index){
	if (typeof spiderchart !== 'undefined'){ 
		spiderchart.wrangleData(index)
	}
}

function renderPatient(patient){
	let h3 = document.getElementById("nameHeader");
	//console.log(patient)
	let name = patient.name[0].given[0] + " " + patient.name[0].family;
	h3.innerHTML = name;
}

//called from SpiderChart
function selectAxis(parent, axis, data, updatePersist=true){
	var height = 200;
	var selectedCategoryLine = new LineChart(parent, this, axis, axis, data, height=height)
	initRemovalFunctionality(axis, updatePersist, 'QRAxis')
}

function initRemovalFunctionality(name, updatePersist=true, chartType=undefined){
	let chartId = "lineChart" + name.replace(/[^\w\s]/gi, '').replace(" ", "");
	let chartDiv = document.getElementById(chartId).parentElement;
	//console.log(chartDiv);

	var btn = document.createElement("BUTTON");        
	var t = document.createTextNode("Close");
	btn.classList.add('btn');       
	btn.classList.add('btn-primary');
	btn.appendChild(t);
	//btn.style.float = 'right';
	btn.style.opacity = 0.8
	btn.style.position = 'absolute';
	btn.style.left = '650px';//todo extract hardcoded value, corresponds to width of linechart
         
	btn.onclick = function(){
		console.log("removing " +chartId);
		d3.select("#" + chartId).remove();
		chartDiv.remove();

		let patient = JSON.parse(_patient);
		$.post("/Patient/UnsaveChart/" + patient.id, {'chartName': name});

		this.remove();
	}

	chartDiv.prepend(btn);

	//ajax to update persisted charts
	if(updatePersist){
		let patient = JSON.parse(_patient);
		$.post("/Patient/SaveChart/" + patient.id, {
			'chartName': name,
			'chartType': chartType
		});
	}
}

function createBarChart(parent){
	var container = document.getElementById(parent);
	$("#" + parent).append("<div id='barChart'></div>"); //overwrites previous barchart if any

	barChart = new BarChart('barChart', this)
}

function createSpiderChart(resources, name){
	var container = document.getElementById('modalContainer');
	container.hidden = false
	container.style.display = "block";

	var background = document.getElementById('modalBackground');
	background.hidden = false;
	background.style.display = "block";

	spiderchart = new SpiderChart("#spiderChart", this, resources, "",
	 height=400, width=400, selectedDiv = '#line');

	//we only want 1 linechart in the modal container
	if($('#aggregateSpiderController').children().length == 0){
		new LineChart("#aggregateSpiderController", this, name, 'all', resources,
			undefined, undefined, undefined, "modalLineChart");
	}
	else{
		d3.select("#lineChart" + "modalLineChart").remove();
		new LineChart("#aggregateSpiderController", this, name, 'all', resources,
			undefined, undefined, undefined, "modalLineChart");
	}
}
function hideSpiderChart(){
	document.getElementById('modalContainer').style.display = "none";
	document.getElementById('modalBackground').style.display = "none";
}

function parseJsonFromStringArray(strings){
	let objects = []
	if (strings){
		strings.forEach(str => {
			let obj = JSON.parse(str);
			objects.push(obj);
		})
		return objects;
	}
	return null;
}
