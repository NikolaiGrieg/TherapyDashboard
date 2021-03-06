﻿
LineChart = function(_parentElement, controller, name, elements,
   forms, height = 200, width = 700, fhir = false, id = name){
  this.parentElement = _parentElement;
  this.controller = controller;
  this.name = name;
  this.elements = elements;
  this.initHeight = height;
  this.initWidt = width;
  this.data = JSON.parse(JSON.stringify(forms)); //deep clone
  this.fhir = fhir;
  this.id = id;
  this.axisNameLength = 30;
  this.initVis();

};

LineChart.prototype.initVis = function(){
    var vis = this;

  //hack to solve width in spider
  if (vis.parentElement === "#aggregateSpiderController"){
      vis.initWidt = document.getElementById("aggregateSpiderController").clientWidth; 
  }

  // set the dimensions and margins of the graph
  vis.margin = {top: 40, right: 50, bottom: 30, left: 50};
  vis.width = vis.initWidt - vis.margin.left - vis.margin.right; 
  vis.height = vis.initHeight - vis.margin.top - vis.margin.bottom;


  vis.outerDiv = d3.select(vis.parentElement).append("div");
  // appends a 'group' element to 'svg'
  // moves the 'group' element to the top left margin
  vis.svg = vis.outerDiv.append("svg")
      .attr("id", "lineChart" + vis.id.replace(/[^\w\s]/gi, '').replace(" ", ""))
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

  $(function() {

    // parse the date / time
    var parseTime = d3.timeParse("%Y-%m-%d"); // possibly some precision loss

    var data = [];

    //wrangle data 
    for (var key in vis.data) { 
       if (vis.data.hasOwnProperty(key)) {
          var entry = vis.data[key];
          if (typeof entry === 'object'){
            entry['date'] = key;
          }
          else{
            let val = entry;
              entry = {
                  value: val,
                  date: key
              };
          }
          
          data.push(entry);
       }
    }

    // format the data
    if (vis.elements === 'all'){
          data.forEach(function(d) {
          d.date = parseTime(d.date);
          let sum = 0;
          for (var key in d) {
            if (d.hasOwnProperty(key)) {
                if(key !== 'date' && key !== '$oid'){
                    sum += +d[key];
                }
            }
          }
          d.close = sum;
      });
    }
    else{
        data.forEach(function (d) {
            d.date = parseTime(d.date);
            let sum = 0;
            if (d.close !== null) {
                delete d.close;
            }
            for (var key in d) {
                if (d.hasOwnProperty(key)) {
                    if (key !== 'date') {
                        if (key.startsWith(vis.elements)) {
                            d.close = +d[key];
                            vis.name = key; //hack to solve wrong naming of axes with names > 30 chars
                        }
                        else {
                            delete d[key];
                        }
                    }
                }
            }
        });
    }

      data.sort(function (x, y) {
          return d3.ascending(x.date, y.date);
      });

    vis.data = data;

    // Scale the range of the data
    vis.xDomain = d3.extent(vis.data, function(d) { return d.date; });
    vis.yDomain = d3.extent(vis.data, function(d) { return d.close; }); 
    //replace with this for absolute range:
    //[0, d3.max(vis.data, function(d) { return d.close; })]; 

    vis.xScale.domain(vis.xDomain);
    vis.yScale.domain(vis.yDomain);

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


    if (vis.parentElement === "#line"){ 
      focus.append('line')
        .attr('id', 'focusLineX')
        .attr('class', 'focusLine');
        
    }
    else{
      focus.append('line')
        .attr('id', 'focusLineX')
        .attr('class', 'focusLineOrange');
    }

    //horizontal blue line
    if (vis.parentElement === "#line"){     
      focus.append('line')
          .attr('id', 'focusLineY')
          .attr('class', 'focusLine');
    }


    var xAxis = d3.axisBottom(vis.xScale);
    var yAxis = d3.axisLeft(vis.yScale);

    var bisectDate = d3.bisector(function(d) { 
      return d.date; 
    }).left;

    /* Add circles in the line */
    if (vis.data.length < 80){ 
      var circleOpacity = '0.85';
      var circleRadius = 3;
    

        g.selectAll("circle-group")
            .data(vis.data).enter()
            .append("g")
            .style("fill", (d, i) => d3.hcl(-97, 32, 52))//steelblue
            .selectAll("circle")
            .data(vis.data).enter()
            .append("g")
            .attr("class", "circle")
            .append("circle")
            .attr("cx", d => vis.xScale(d.date))
            .attr("cy", d => vis.yScale(d.close))
            .attr("r", circleRadius)
            .style('opacity', circleOpacity);
    }

    //overlay focus selector
    g.append('rect')
        .attr('class', 'overlay')
        .attr('width', vis.width)
        .attr('height', vis.height)
        .on('mouseover', function() { focus.style('display', null); })
        .on('mouseout', function() { focus.style('display', 'none'); })
        .on('mousemove', function() { 
            var mouse = d3.mouse(this);
            var mouseDate = vis.xScale.invert(mouse[0]);
            const dates = vis.data.map(d => d.date);
            var i = bisectDate(vis.data, mouseDate); // returns the index to the current data item
            var j = d3.bisectLeft(dates, mouseDate);

            var d0 = vis.data[i - 1];
            var d1 = vis.data[i];

            // work out which date value is closest to the mouse
            var d = mouseDate - d0[0] > d1[0] - mouseDate ? d1 : d0;

            var x = vis.xScale(d.date);
            var y = vis.yScale(d.close);

            focus.select('#focusLineX')
                .attr('x1', x).attr('y1', vis.yScale(vis.yDomain[0]))
                .attr('x2', x).attr('y2', vis.yScale(vis.yDomain[1]));
                
            focus.select('#focusLineY')
                .attr('x1', vis.xScale(vis.xDomain[0])).attr('y1', y)
                .attr('x2', vis.xScale(vis.xDomain[1])).attr('y2', y);
                

            //send event to controller
            if (vis.parentElement === "#aggregateSpiderController"){
                vis.controller.update(i-1); //Hack to fix some indexing problems from another place
            }
        });

    //Chart Title
    vis.svg.append("text")
      .attr("x", (vis.width / 2))             
      .attr("y", 0 - (vis.margin.top / 2))
      .attr("text-anchor", "middle")  
      .style("font-size", "16px")   
      .text(vis.name);
    });


  vis.updateVis();
};

LineChart.prototype.updateVis = function(){
  var vis = this;
                
};


