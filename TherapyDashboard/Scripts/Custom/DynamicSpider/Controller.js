//console.log(dataPath);
//TODO change name, this is no longer a controller

function initQRLineCharts(resources){
	var temp_a = forms; //forms should be same format as resources.
	//console.log(temp_a)
	linechart = new LineChart("#line", this, 'MADRS-S', 'all', resources);
}

//spiderchart = new SpiderChart("#chart", this, dataPath);




function update(index){
	if (typeof spiderchart !== 'undefined'){ //TODO disable update calls from background charts
		spiderchart.wrangleData(index)
	}
}

function selectAxis(parent, axis){
	console.log(axis);
	var height = 200;
	var selectedCategoryLine = new LineChart(parent, this, axis, axis, forms, height=height)

	//Remove button for selected category linecharts
	/*
	var btn = document.createElement("BUTTON");        
	var t = document.createTextNode("Remove");
	btn.classList.add('btn');       
	btn.classList.add('btn-primary');
	btn.appendChild(t);
	btn.style.marginTop = height/1.5 +"px"; //TODO include chart margins here                              
	btn.onclick = function(){
		d3.select("#lineChart" + axis.replace(/\s/g, '')).remove();
		this.remove();
	}

	document.getElementById("removeButton").appendChild(btn);
	*/
	//selectedCategoryLine.wrangleData(axis);
}

function createBarChart(parent){
	var container = document.getElementById(parent);
	$("#" + parent).append("<div id='barChart'></div>"); //overwrites previous barchart if any

	barChart = new BarChart('barChart', this)
}

//called from html?
function createSpiderChart(resources){
	/*
	var container = document.getElementById(parent);
	$("#" + parent).append("<div id='spiderChart'></div>"); //overwrites previous barchart if any
	*/
	var container = document.getElementById('modalContainer');
	container.hidden = false
	container.style.display = "block";

	spiderchart = new SpiderChart("#spiderChart", this, resources, "MADRS-S",
	 height=400, width=400, selectedDiv = '#line');

	//Avoids adding duplicate summary charts to modal container
	if($('#aggregateSpiderController').children().length == 0){
		linechart = new LineChart("#aggregateSpiderController", this, 'Summary', 'all', resources);
	}
	
}
function hideSpiderChart(){
	document.getElementById('modalContainer').style.display = "none";

}

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