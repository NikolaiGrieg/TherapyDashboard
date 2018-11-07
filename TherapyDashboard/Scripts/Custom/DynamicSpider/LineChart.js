﻿
LineChart = function(_parentElement, controller){
  this.parentElement = _parentElement;
  this.controller = controller
  this.initVis();

};

LineChart.prototype.initVis = function(){
  var vis = this

  // set the dimensions and margins of the graph
  vis.margin = {top: 20, right: 20, bottom: 30, left: 50};
  vis.width = 700 - vis.margin.left - vis.margin.right;
  vis.height = 400 - vis.margin.top - vis.margin.bottom;


  // appends a 'group' element to 'svg'
  // moves the 'group' element to the top left margin
  vis.svg = d3.select("#line").append("svg")
      .attr("width", vis.width + vis.margin.left + vis.margin.right)
      .attr("height", vis.height + vis.margin.top + vis.margin.bottom)
    .append("g")
      .attr("transform",
            "translate(" + vis.margin.left + "," + vis.margin.top + ")");


  vis.wrangleData();
};

LineChart.prototype.wrangleData = function(){
  var vis = this;

  // set the ranges
  vis.xScale = d3.scaleTime().range([0, vis.width]);
  vis.yScale = d3.scaleLinear().range([vis.height, 0]);

  d3.csv("Data/SampleComposit.csv", function(error, data) {
    if (error) throw error;

    // parse the date / time
    var parseTime = d3.timeParse("%Y-%m-%d");//"%d-%b-%y");

    // format the data
    data.forEach(function(d) {
          d.date = parseTime(d.date);
          let sum = 0;
          for (var key in d) {
            if (d.hasOwnProperty(key)) {
                if(key != 'date'){
                  //console.log(key + " -> " + d[key]);
                  sum += +d[key]
                }
            }
          }
          d.close = sum;
    });

    data.sort(function(x, y){
          return d3.ascending(x.date, y.date);
    })

    vis.data = data;

    // Scale the range of the data
    vis.xDomain = d3.extent(vis.data, function(d) { return d.date; });
    vis.yDomain = [0, d3.max(vis.data, function(d) { return d.close; })];

    vis.xScale.domain(vis.xDomain)
    vis.yScale.domain(vis.yDomain)

    // Add the X Axis
    vis.svg.append("g")
        .attr("transform", "translate(0," + vis.height + ")")
        .call(d3.axisBottom(vis.xScale));

    // Add the Y Axis
    let g = vis.svg.append("g")
        .call(d3.axisLeft(vis.yScale));


    // define the line
    vis.valueline = d3.line()
        .x(function(d) { return vis.xScale(d.date); })
        .y(function(d) { return vis.yScale(d.close); });

    // Add the valueline path.
    vis.svg.append("path")
        .data([vis.data])
        .attr("class", "line")
        .attr("d", vis.valueline);

          var focus = g.append('g').style('display', 'none');

    focus.append('line')
        .attr('id', 'focusLineX')
        .attr('class', 'focusLine');
        /*
    focus.append('line')
        .attr('id', 'focusLineY')
        .attr('class', 'focusLine');
        */

    var xAxis = d3.axisBottom(vis.xScale);
    var yAxis = d3.axisLeft(vis.yScale);

    var bisectDate = d3.bisector(function(d) { 
      return d.date; 
    }).left;

    g.append('rect')
        .attr('class', 'overlay')
        .attr('width', vis.width)
        .attr('height', vis.height)
        .on('mouseover', function() { focus.style('display', null); })
        .on('mouseout', function() { focus.style('display', 'none'); })
        .on('mousemove', function() { 
            var mouse = d3.mouse(this);
            var mouseDate = vis.xScale.invert(mouse[0]);
            const dates = vis.data.map(d => d.date)
            var i = bisectDate(vis.data, mouseDate); // returns the index to the current data item
            //var j = d3.bisectLeft(dates, mouseDate)

            var d0 = vis.data[i - 1]
            var d1 = vis.data[i];
            // work out which date value is closest to the mouse
            var d = mouseDate - d0[0] > d1[0] - mouseDate ? d1 : d0;

            var x = vis.xScale(d.date);
            var y = vis.yScale(d.close);

            //console.log("x: " + x + " y: " + y);

            focus.select('#focusLineX')
                .attr('x1', x).attr('y1', vis.yScale(vis.yDomain[0]))
                .attr('x2', x).attr('y2', vis.yScale(vis.yDomain[1]));
                /*
            focus.select('#focusLineY')
                .attr('x1', vis.xScale(vis.xDomain[0])).attr('y1', y)
                .attr('x2', vis.xScale(vis.xDomain[1])).attr('y2', y);
                */

            //send event to controller
            vis.controller.update(i);
            
        });
  });


  vis.updateVis();
};

LineChart.prototype.updateVis = function(){
  var vis = this;
                

};

