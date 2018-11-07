linechart = new LineChart("#line", this, 'Summary', 'all');
spiderchart = new SpiderChart("#chart", this);


function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(axis){
	console.log(axis);
	selectedCategoryLine = new LineChart("#selectedCategory", this, axis, axis)
	//selectedCategoryLine.wrangleData(axis);
}