linechart = new LineChart("#line", this, 'Summary', 'all');
spiderchart = new SpiderChart("#chart", this);


function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(axis){
	console.log(axis);
	var selectedCategoryLine = new LineChart("#selectedCategory", this, axis, axis, height=250)

	//TODO make button actually remove chart
	//TODO fix positioning in the html, possibly make linecharts less wide
	var btn = document.createElement("BUTTON");        
	var t = document.createTextNode("Remove");
	btn.classList.add('btn');       
	btn.classList.add('btn-primary');
	btn.appendChild(t);
	btn.style.marginTop = "100px"; //TODO should be about half of chart height, or some better way to inject                               
	btn.onclick = function(){
		d3.select("#lineChart" + axis).remove();
		this.remove();
	}

	document.getElementById("removeButton").appendChild(btn);

	//selectedCategoryLine.wrangleData(axis);
}