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
	let chartId = "lineChart" + name.replace(/\s/g, '');
	let chartDiv = document.getElementById(chartId).parentElement;
	//console.log(chartDiv);

	var btn = document.createElement("BUTTON");        
	var t = document.createTextNode("X");
	btn.classList.add('btn');       
	btn.classList.add('btn-danger');
	btn.appendChild(t);
	btn.style.float = 'right';
	btn.style.opacity = 0.8
         
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

/*
function createEventLine(parent){
	var margin = {top: 20, right: 20, bottom: 30, left: 60},
	width = 960 - margin.left - margin.right,
	height = 200 - margin.top - margin.bottom;

	var colorOf = d3.scaleOrdinal(d3.schemeCategory10)
		
	var y = d3.scaleBand()
		.domain([height, 0], 0.3);

	var x = d3.scaleTime()
		.range([5, width]);

	var yAxis = d3
		.axisLeft(y);
		
	var xAxis = d3
		.axisBottom(x)
		.tickFormat(d3.timeFormat("%m/%d/%Y %Hh"));
		
	var parseDate = d3.timeFormat("%m/%d/%Y %H:%M");

	var svg = d3.select("body").append("svg")
		.attr("width", width + margin.left + margin.right)
		.attr("height", height + margin.top + margin.bottom)
	  .append("g")
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");

	var raw = d3.select("#csvdata").text();

	var data = d3.csv("#csvdata", function(error, data) {
		if (error) throw error;

		data.forEach(function(d) {
			d.time = parseDate(d.time);
		});

		y.domain(data.map(function(d) { return d.type; }));
		x.domain(d3.extent(data, function(d) { return d.time; }));
		colorOf.domain(data.map(function(d) { return d.type; }));

		svg.append("g")
		  .attr("class", "x axis")
		  .attr("transform", "translate(0," + height + ")")
		  .call(xAxis);

		svg.append("g")
		  .attr("class", "y axis")
		  .call(yAxis);

		svg.selectAll(".bar")
		  .data(data)
		.enter().append("rect")
		  .attr("class", "bar")
		  .attr("y", function(d) { return y(d.type); })
		  .attr("height", y.bandwidth())
		  .attr("x", function(d) { return x(d.time); })
		  .attr("width", 1)
		  .style("fill", function(d) { return colorOf(d.type); });
	});

	//var data = d3.csv.parse(raw);



}
*/