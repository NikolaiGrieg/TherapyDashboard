﻿SpiderChart = function(_parentElement, controller, dataPath, name,
    height = 500, width = 500, selectedDiv = "#selectedCategory"){
  this.parentElement = _parentElement;
  this.controller = controller;
  this.dataPath = dataPath;
  this.name = name;
  this.height = height;
  this.width = width;
  this.selectedDiv = selectedDiv;
  this.initVis();
};

SpiderChart.prototype.initVis = function(){
    var vis = this;

    vis.wrangleData(1)
}

//TODO lock axes based on highest data points
SpiderChart.prototype.wrangleData = function(index){
    var vis = this;
    //Data
    d3.csv(vis.dataPath, function(error, data) {
        if (error) throw error;

        // parse the date / time
        var parseTime = d3.timeParse("%Y-%m-%d");//"%d-%b-%y");

        // format the data
        data.forEach(function(d) {
            d.date = parseTime(d.date);
            for (var key in d) {
                if (d.hasOwnProperty(key)) {
                    if (key != 'date'){
                      d[key] = +d[key]
                    }
                }
            }
        });

        //sort on date for bisection later
        data.sort(function(x, y){
              return d3.ascending(x.date, y.date);
        })

        var trace1 = []
        var trace2 = []

        //convert data to traces
        for (var key in data[data.length-1]) {
            if (data[data.length-1].hasOwnProperty(key)) {
                if (key != 'date'){
                    axis = {
                        axis: key, value: +data[data.length-1][key]
                    }
                    trace1.push(axis)
                }
            }
        }
        for (var key in data[index]) {
            if (data[index].hasOwnProperty(key)) {
                if (key != 'date'){
                    axis = {
                        axis: key, value: +data[index][key]
                    }
                    trace2.push(axis)
                }
            }
        }

        vis.data = [trace1, trace2];

        vis.updateVis()
    });
}


SpiderChart.prototype.updateVis = function(){
  var vis = this;
  var RadarChart = {
    draw: function (id, data, options) {
        var cfg = {
            radius: 5,
            w: 600,
            h: 600,
            factor: 1,
            factorLegend: .85,
            levels: 3,
            maxValue: 0,
            radians: 2 * Math.PI,
            opacityArea: 0.5,
            ToRight: 5,
            TranslateX: 80,
            TranslateY: 30,
            ExtraWidthX: 100,
            ExtraWidthY: 100,
            color: d3.scaleOrdinal(d3.schemeCategory10)
        };

        //TODO should always have equal height and width
        if ('undefined' !== typeof options) {
            for (var i in options) {
                if ('undefined' !== typeof options[i]) {
                    cfg[i] = options[i];
                }
            }
        }
        cfg.maxValue = Math.max(cfg.maxValue, d3.max(vis.data, function (i) { return d3.max(i.map(function (o) { return o.value; })) }));
        var allAxis = (vis.data[0].map(function (i, j) { return i.axis }));
        var total = allAxis.length;
        var radius = cfg.factor * Math.min(cfg.w / 2, cfg.h / 2);
        var Format = d3.format('r');
        d3.select(id).select("svg").remove();

        var g = d3.select(id)
            .append("svg")
            .attr("width", cfg.w + cfg.ExtraWidthX)
            .attr("height", cfg.h + cfg.ExtraWidthY)
            .append("g")
            .attr("transform", "translate(" + cfg.TranslateX + "," + cfg.TranslateY + ")");
        ;

        var tooltip;

        //Circular segments
        for (var j = 0; j < cfg.levels - 1; j++) {
            var levelFactor = cfg.factor * radius * ((j + 1) / cfg.levels);
            g.selectAll(".levels")
                .data(allAxis)
                .enter()
                .append("svg:line")
                .attr("x1", function (d, i) { return levelFactor * (1 - cfg.factor * Math.sin(i * cfg.radians / total)); })
                .attr("y1", function (d, i) { return levelFactor * (1 - cfg.factor * Math.cos(i * cfg.radians / total)); })
                .attr("x2", function (d, i) { return levelFactor * (1 - cfg.factor * Math.sin((i + 1) * cfg.radians / total)); })
                .attr("y2", function (d, i) { return levelFactor * (1 - cfg.factor * Math.cos((i + 1) * cfg.radians / total)); })
                .attr("class", "line")
                .style("stroke", "grey")
                .style("stroke-opacity", "0.75")
                .style("stroke-width", "0.3px")
                .attr("transform", "translate(" + (cfg.w / 2 - levelFactor) + ", " + (cfg.h / 2 - levelFactor) + ")");
        }

        //Text indicating at what % each level is
        for (let j = 0; j < cfg.levels; j++) {
            let levelFactor = cfg.factor * radius * ((j + 1) / cfg.levels);
            g.selectAll(".levels")
                .data([1]) 
                .enter()
                .append("svg:text")
                .attr("x", function (d) { return levelFactor * (1 - cfg.factor * Math.sin(0)); })
                .attr("y", function (d) { return levelFactor * (1 - cfg.factor * Math.cos(0)); })
                .attr("class", "legend")
                .style("font-family", "sans-serif")
                .style("font-size", "10px")
                .attr("transform", "translate(" + (cfg.w / 2 - levelFactor + cfg.ToRight) + ", " + (cfg.h / 2 - levelFactor) + ")")
                .attr("fill", "#737373")
                .text(Format((j + 1) * cfg.maxValue / cfg.levels));
        }

        series = 0;

        var axis = g.selectAll(".axis")
            .data(allAxis)
            .enter()
            .append("g")
            .attr("class", "axis");

        axis.append("line")
            .attr("x1", cfg.w / 2)
            .attr("y1", cfg.h / 2)
            .attr("x2", function (d, i) { return cfg.w / 2 * (1 - cfg.factor * Math.sin(i * cfg.radians / total)); })
            .attr("y2", function (d, i) { return cfg.h / 2 * (1 - cfg.factor * Math.cos(i * cfg.radians / total)); })
            .attr("class", "line")
            .style("stroke", "grey")
            .style("stroke-width", "1px");

        axis.append("text")
            .attr("class", "legend")
            .text(function (d) { return d })
            .style("font-family", "sans-serif")
            .style("font-size", "11px")
            .attr("text-anchor", "middle")
            .attr("dy", "1.5em")
            .attr("transform", function (d, i) { return "translate(0, -10)" })
            .attr("x", function (d, i) { return cfg.w / 2 * (1 - cfg.factorLegend * Math.sin(i * cfg.radians / total)) - 60 * Math.sin(i * cfg.radians / total); })
            .attr("y", function (d, i) { return cfg.h / 2 * (1 - Math.cos(i * cfg.radians / total)) - 20 * Math.cos(i * cfg.radians / total); });

        //TODO bisect on axis to find closest point
        axis.on("click", function(d) {
            vis.controller.selectAxis(vis.selectedDiv, d);
        });

        vis.data.forEach(function (y, x) {
            dataValues = [];
            g.selectAll(".nodes")
                .data(y, function (j, i) {
                    dataValues.push([
                        cfg.w / 2 * (1 - (parseFloat(Math.max(j.value, 0)) / cfg.maxValue) * cfg.factor * Math.sin(i * cfg.radians / total)),
                        cfg.h / 2 * (1 - (parseFloat(Math.max(j.value, 0)) / cfg.maxValue) * cfg.factor * Math.cos(i * cfg.radians / total))
                    ]);
                });
            dataValues.push(dataValues[0]);
            g.selectAll(".area")
                .data([dataValues])
                .enter()
                .append("polygon")
                .attr("class", "radar-chart-serie" + series)
                .style("stroke-width", "2px")
                .style("stroke", cfg.color(series))
                .attr("points", function (d) {
                    var str = "";
                    for (var pti = 0; pti < d.length; pti++) {
                        str = str + d[pti][0] + "," + d[pti][1] + " ";
                    }
                    return str;
                })
                .style("fill", function (j, i) { return cfg.color(series) })
                .style("fill-opacity", cfg.opacityArea)
                .on('mouseover', function (d) {
                    z = "polygon." + d3.select(this).attr("class");
                    g.selectAll("polygon")
                        .transition(200)
                        .style("fill-opacity", 0.1);
                    g.selectAll(z)
                        .transition(200)
                        .style("fill-opacity", .7);
                })
                .on('mouseout', function () {
                    g.selectAll("polygon")
                        .transition(200)
                        .style("fill-opacity", cfg.opacityArea);
                });
            series++;
        });
        series = 0;


        vis.data.forEach(function (y, x) {
            g.selectAll(".nodes")
                .data(y).enter()
                .append("svg:circle")
                .attr("class", "radar-chart-serie" + series)
                .attr('r', cfg.radius)
                .attr("alt", function (j) { return Math.max(j.value, 0) })
                .attr("cx", function (j, i) {
                    dataValues.push([
                        cfg.w / 2 * (1 - (parseFloat(Math.max(j.value, 0)) / cfg.maxValue) * cfg.factor * Math.sin(i * cfg.radians / total)),
                        cfg.h / 2 * (1 - (parseFloat(Math.max(j.value, 0)) / cfg.maxValue) * cfg.factor * Math.cos(i * cfg.radians / total))
                    ]);
                    return cfg.w / 2 * (1 - (Math.max(j.value, 0) / cfg.maxValue) * cfg.factor * Math.sin(i * cfg.radians / total));
                })
                .attr("cy", function (j, i) {
                    return cfg.h / 2 * (1 - (Math.max(j.value, 0) / cfg.maxValue) * cfg.factor * Math.cos(i * cfg.radians / total));
                })
                .attr("data-id", function (j) { return j.axis })
                .style("fill", cfg.color(series)).style("fill-opacity", .9)
                .on('mouseover', function (d) {
                    newX = parseFloat(d3.select(this).attr('cx')) - 10;
                    newY = parseFloat(d3.select(this).attr('cy')) - 5;

                    tooltip
                        .attr('x', newX)
                        .attr('y', newY)
                        .text(Format(d.value))
                        .transition(200)
                        .style('opacity', 1);

                    z = "polygon." + d3.select(this).attr("class");
                    g.selectAll("polygon")
                        .transition(200)
                        .style("fill-opacity", 0.1);
                    g.selectAll(z)
                        .transition(200)
                        .style("fill-opacity", .7);
                })
                .on('mouseout', function () {
                    tooltip
                        .transition(200)
                        .style('opacity', 0);
                    g.selectAll("polygon")
                        .transition(200)
                        .style("fill-opacity", cfg.opacityArea);
                })
                .append("svg:title")
                .text(function (j) { return Math.max(j.value, 0) });

        series++;
        });
        //Tooltip
        tooltip = g.append('text')
            .style('opacity', 0)
            .style('font-family', 'sans-serif')
            .style('font-size', '13px');

        

        g.append("text")
          .attr("x", (vis.width / 2))             
          .attr("y", "-2em")
          .attr("text-anchor", "middle")  
          .style("font-size", "16px")   
          .text(vis.name);

        }
    };


    var colorscale = d3.scaleOrdinal(d3.schemeCategory10);

    //Legend titles
    //var LegendOptions = ['Smartphone', 'Tablet'];

    //Options for the Radar chart, other than default
    var mycfg = {
        w: vis.width,
        h: vis.height,
        maxValue: 8,
        levels: 6,
        ExtraWidthX: 150,
        TranslateY: 50
    }

    //Call function to draw the Radar chart
    RadarChart.draw(vis.parentElement, vis.data, mycfg);
}