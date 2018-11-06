
// set the dimensions and margins of the graph
var margin = {top: 20, right: 20, bottom: 30, left: 50},
width = 700 - margin.left - margin.right,
height = 400 - margin.top - margin.bottom;

// parse the date / time
var parseTime = d3.timeParse("%d-%b-%y");

// set the ranges
var xScale = d3.scaleTime().range([0, width]);
var yScale = d3.scaleLinear().range([height, 0]);

// define the line
var valueline = d3.line()
    .x(function(d) { return xScale(d.date); })
    .y(function(d) { return yScale(d.close); });

// append the svg obgect to the body of the page
// appends a 'group' element to 'svg'
// moves the 'group' element to the top left margin
var svg = d3.select("#line").append("svg")
    .attr("width", width + margin.left + margin.right)
    .attr("height", height + margin.top + margin.bottom)
  .append("g")
    .attr("transform",
          "translate(" + margin.left + "," + margin.top + ")");

// Get the data
d3.csv("Data/data.csv", function(error, data) {
  if (error) throw error;

  // format the data
  data.forEach(function(d) {
        d.date = parseTime(d.date);
        d.close = +d.close;
  });

  data = data.sort(function(x, y){
   return d3.ascending(x.date, y.date);
  })

  // Scale the range of the data
  var xDomain = d3.extent(data, function(d) { return d.date; });
  var yDomain = [0, d3.max(data, function(d) { return d.close; })];

  xScale.domain(xDomain)
  yScale.domain(yDomain)

  // Add the valueline path.
  svg.append("path")
      .data([data])
      .attr("class", "line")
      .attr("d", valueline);


  var xAxis = d3.axisBottom(xScale);
  var yAxis = d3.axisLeft(yScale);


  // Add the X Axis
  svg.append("g")
      .attr("transform", "translate(0," + height + ")")
      .call(d3.axisBottom(xScale));

  // Add the Y Axis
  let g = svg.append("g")
      .call(d3.axisLeft(yScale));

  var focus = g.append('g').style('display', 'none');
                
  focus.append('line')
      .attr('id', 'focusLineX')
      .attr('class', 'focusLine');
  focus.append('line')
      .attr('id', 'focusLineY')
      .attr('class', 'focusLine');

  var bisectDate = d3.bisector(function(d) { 
    return d.date; 
  }).left;

  g.append('rect')
      .attr('class', 'overlay')
      .attr('width', width)
      .attr('height', height)
      .on('mouseover', function() { focus.style('display', null); })
      .on('mouseout', function() { focus.style('display', 'none'); })
      .on('mousemove', function() { 
          var mouse = d3.mouse(this);
          var mouseDate = xScale.invert(mouse[0]);
          const dates = data.map(d => d.date)
          var i = bisectDate(data, mouseDate); // returns the index to the current data item
          var j = d3.bisectLeft(dates, mouseDate)

          var d0 = data[i - 1]
          var d1 = data[i];
          // work out which date value is closest to the mouse
          var d = mouseDate - d0[0] > d1[0] - mouseDate ? d1 : d0;

          var x = xScale(d.date);
          var y = yScale(d.close);

          //console.log("x: " + x + " y: " + y);

          focus.select('#focusLineX')
              .attr('x1', x).attr('y1', yScale(yDomain[0]))
              .attr('x2', x).attr('y2', yScale(yDomain[1]));
          focus.select('#focusLineY')
              .attr('x1', xScale(xDomain[0])).attr('y1', y)
              .attr('x2', xScale(xDomain[1])).attr('y2', y);
          
      });
});