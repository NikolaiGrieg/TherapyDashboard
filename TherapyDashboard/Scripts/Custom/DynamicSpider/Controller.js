//console.log(dataPath);
linechart = new LineChart("#line", this, 'Summary', 'all', dataPath);
spiderchart = new SpiderChart("#chart", this, dataPath);


function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(axis){
	console.log(axis);
	var height = 250;
	var selectedCategoryLine = new LineChart("#selectedCategory", this, axis, axis, dataPath, height=height)

	//TODO make button actually remove chart
	//TODO fix positioning in the html, possibly make linecharts less wide
	var btn = document.createElement("BUTTON");        
	var t = document.createTextNode("Remove " + axis);
	btn.classList.add('btn');       
	btn.classList.add('btn-primary');
	btn.appendChild(t);
	btn.style.marginTop = height/1.5 +"px"; //TODO include chart margins here                              
	btn.onclick = function(){
		d3.select("#lineChart" + axis).remove();
		this.remove();
	}

	document.getElementById("removeButton").appendChild(btn);

	//selectedCategoryLine.wrangleData(axis);
}