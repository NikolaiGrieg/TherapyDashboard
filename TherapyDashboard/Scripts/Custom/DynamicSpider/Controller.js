//console.log(dataPath);
linechart = new LineChart("#line", this, 'Summary', 'all', dataPath);
spiderchart = new SpiderChart("#chart", this, dataPath);


function createSpiderChart(parent){
	var spiderchart = new SpiderChart(parent, this, dataPath);
}

function update(index){
	spiderchart.wrangleData(index)
}

function selectAxis(parent, axis){
	console.log(axis);
	var height = 250;
	var selectedCategoryLine = new LineChart(parent, this, axis, axis, dataPath, height=height)

	//TODO make button actually remove chart
	//TODO fix positioning in the html, possibly make linecharts less wide
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

	//selectedCategoryLine.wrangleData(axis);
}

function createBarChart(parent){

	Highcharts.chart(parent, {
	    chart: {
	        type: 'column'
	    },
	    title: {
	        text: null
	    },
	    yAxis: {
	        title: {
	            text: 'Times logged in'
	        }
	    },
	    xAxis: {
	        title: {
	            text: 'Week'
	        }
	    },

	    plotOptions: {
	        series: {
	            label: {
	                connectorAllowed: false
	            },
	            pointStart: 32
	        }
	    },

	    series: [{
	        name: 'Logins',
	        data: [3, 0, 1, 2, 0, 4, 1, 1]
	    }],

	    legend: {
	        enabled: false
	    },

	    credits: {
	        enabled: false
	    },
	});
}