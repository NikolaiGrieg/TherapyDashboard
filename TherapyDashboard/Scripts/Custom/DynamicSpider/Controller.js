//console.log(dataPath);
linechart = new LineChart("#line", this, 'Summary', 'all', dataPath);
//spiderchart = new SpiderChart("#chart", this, dataPath);


function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(parent, axis){
	console.log(axis);
	var height = 200;
	var selectedCategoryLine = new LineChart(parent, this, axis, axis, dataPath, height=height)

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

function createSpiderChart(){
	/*
	var container = document.getElementById(parent);
	$("#" + parent).append("<div id='spiderChart'></div>"); //overwrites previous barchart if any
	*/
	var container = document.getElementById('modalContainer');
	container.hidden = false
	container.style.display = "block";

	spiderchart = new SpiderChart("#spiderChart", this, dataPath, "MADRS-S",
	 height=400, width=400, selectedDiv = '#line');

	//Avoids adding duplicate summary charts to modal container
	if($('#aggregateSpiderController').children().length == 0){
		linechart = new LineChart("#aggregateSpiderController", this, 'Summary', 'all', dataPath);
	}
	
}
function hideSpiderChart(){
	document.getElementById('modalContainer').style.display = "none";

}