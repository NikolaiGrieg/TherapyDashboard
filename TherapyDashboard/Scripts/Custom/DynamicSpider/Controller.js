//console.log(dataPath);
linechart = new LineChart("#line", this, 'Summary', 'all', dataPath);
spiderchart = new SpiderChart("#chart", this, dataPath);


function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(parent, axis){
	console.log(axis);
	var height = 250;
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
	document.getElementById('spiderChart').hidden = false
	document.getElementById("spiderChart").style.display = "block";

	var spiderchart = new SpiderChart("#spiderChart", this, dataPath,
	 height=500, width=500, selectedDiv = '#line');
}
function hideSpiderChart(){
	document.getElementById('spiderChart').style.display = "none";
}