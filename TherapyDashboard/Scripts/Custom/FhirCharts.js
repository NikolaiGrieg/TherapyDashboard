function initSelectors(measurements){
    let names = Object.keys(measurements);

    var container = document.getElementById("observationsContainer");

    var listItems = "";

    for (let i = 0; i < names.length; i++){
        let curMeasurement = filteredMeasurements[names[i]];
        let numObs = Object.keys(curMeasurement).length;

        var currentHTML = `
            <li>
                <a href="#" onclick="lineChart('${names[i]}')" id="selectorFor${names[i]}">${names[i]} (${numObs})</a>
            </li>
        `;
        listItems += currentHTML;
    }

    var html = `
            <div class="btn-group">
                <button type="button" class="btn btn-info btn-block dropdown-toggle" 
                data-toggle="dropdown" id="observationSelectorBtn" aria-haspopup="true" aria-expanded="false">
                    Select Observation
                    <span class="caret"></span>
                </button> 
            <ul class="dropdown-menu" style="position: absolute;"> 
            ${listItems}
            </ul> 
            </div>
            `;
    

    var observationsDropdown = document.createElement("div");
    observationsDropdown.innerHTML = html;
    container.appendChild(observationsDropdown);
}

function getFirstWords(str, numWords){
            let words = str.split(" ");
            let firstWords = words.slice(0, numWords).join(" ").replace(",", "");
            return firstWords;
        }

function createSpiderChartSelectors(chartNames){
    //build HTML
    var resHTML = "";
    chartNames.forEach(name => {
        let first3Words = name.split(" ").slice(0, 3).join(" ");
        let button = `	<button type="button" class="btn btn-primary" 
                        onclick="initSpider('${name}')">
                        ${first3Words}
                    </button>
                            `;
        resHTML += button;
    });

    //insert HTML
    var containerDiv = document.createElement("div");
    containerDiv.innerHTML = resHTML;

    var container = document.getElementById("observationsContainer");
    container.appendChild(containerDiv);
}


function lineChart(name, updatePersist=true){
    //check if exist to avoid duplicate
    let container = document.getElementById("line");
    
    let exists = false;
    container.childNodes.forEach(child => {
        let childName = child.id.replace("lineChart", "")
            .replace(/[^\w\s]/gi, '');

        if (childName === name.replace(" ", "").replace(/[^\w\s]/gi, '')) {
            exists = true;
        }
    });
    
    if(!exists){
        new LineChart("#line", this, name, 'all', filteredMeasurements[name], 200, 700, fhir=true);
        initRemovalFunctionality(name, updatePersist, 'observation');
    }
}