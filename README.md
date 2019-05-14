# Therapy Dashboard
This application is the Design Science artifact made by Nikolai Alexander Grieg for a Master Thesis in Software Engineering at the Western Norway University of Applied Sciences and the University of Bergen.  

## Application description:
The application is intended to provide Visual Analytics and Decision Support for therapists in Internet-based Cognitive Behavioral Therapy. One overview page over all the patients (Master View) and one page for each patient (Detail View) is presented. The data for this dashboard can be retrieved from any FHIR server conforming to [DSTU3](https://www.hl7.org/fhir/STU3/index.html).

### Master View:  
This page shows some status variables for each patient, and two graphs describing the entire patient population.  
![Picture of master view](https://github.com/NikolaiGrieg/TherapyDashboard/blob/master/Pictures/Dashboard_master_14_5.png)


### Detail view:  
This page shows detailed information about a single patient. The line chart shows the sum over all questions in a series of [FHIR QuestionnaireResponse Resources](https://www.hl7.org/fhir/questionnaireresponse.html), or the values of a serie of [FHIR Observation Resources](https://www.hl7.org/fhir/observation.html), as selected by the user.
![Picture of detail view](https://github.com/NikolaiGrieg/TherapyDashboard/blob/master/Pictures/DetailView_14_5.png)  

#### Interactive spider chart displaying one patients [MADRS scores](https://onlinelibrary.wiley.com/doi/abs/10.1111/j.1600-0447.1978.tb02357.x)  
This figure is a part of the Detail View, and is presented on user click. The blue trace is the latest QuestionnaireResponse resource for this questionnaire, and the orange trace can be selected by the user by hovering the line chart.
![Picture of interactive spider chart](https://github.com/NikolaiGrieg/TherapyDashboard/blob/master/Pictures/interactive_spiderchart_madrs_english.png)


## For development:
### To start the program (current iteration):
1: Start [MongoDB](https://www.mongodb.com/download-center/community) on default port (27017)  
2: Start a FHIR server and set URL of the FHIR base endpoint in /Services/FHIRRepository.cs  
2.5: (Optional) Seed FHIR server with data. See below for generating data.  
3: Set parameters for default functions for the updateGlobalState method in /Services/FHIRRepository.cs The string parameter for default function takes a resource ID of a questionnaire on the resource server.  
3.5 Alternatively to 3: Set custom calculation functions in updateGlobalState method in /Services/FHIRRepository.cs  
4: Launch application (Visual Studio or IIS)  
5: (Optional) Schedule updates: In order to keep the data in the view up to date with respect to the resource server, a scheduler like windows scheduler can be used. Build the application (TherapyDashboard) after updating , and then point the scheduler to run the file /DashboardUpdateScheduler/DashboardUpdateScheduler/bin/Debug/DashboardUpdateScheduler.exe. 

### Generating data:
#### Patient resources:
To generate patient resources in FHIR format, the [Synthea](https://github.com/synthetichealth/synthea) library can be used.  
After cloning the Synthea repository, edit the [bundle configurations](https://github.com/synthetichealth/synthea/wiki/HL7-FHIR) to "exporter.fhir.transaction_bundle = true".  The produced json files can then be uploaded to the base endpoint of the resource server.

#### QuestionnaireResponses, Questionnaires and Observations:
Patient resources are required before these resources can be generated. First set the endpoint to the FHIR server in /DataGenerator/FHIR/FhirGenerator.py in the api_base field. Run the following in a command line in the folder with the requirements.txt file: "pip install -r requirements txt". Then run /DataGenerator/FHIR/FhirGenerator.py, this process will run for a long time depending on how many patients you generated.  

### Setting up a FHIR server:
There are multiple ways to do this, the easiest is probably to use the [HAPI JPA server](https://github.com/hapifhir/hapi-fhir-jpaserver-starter).

### To set up IIS on windows server:
See sections "Install IIS" and "Publish to IIS" in [the microsoft docs](https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/deploying-to-iis).  

### To replace calculation functions:
Implement any of the given interfaces in the /Services folder: IAggregationFunction, IFlagFunction, IWarningFunction.  
Then replace an object of the corresponding interface in HomeController Index() with the custom function object.  
Each of the required functions in the interfaces will take in a List\<QuestionnaireResponse\> and return string or List\<string\>

### Required fields in FHIR Resources, in addition to FHIR mandatory fields:
#### Questionnaire:
Name = string  
Title = string

#### QuestionnaireResponse:
Items = List with item.answer.valueX where X is Integer or Decimal && item.text = string  
Questionnaire.reference = canonical questionnaire uri - ex: "Questionnaire/{questonnaire id}"  
Authored = Date or DateTime  
Author = Reference to Patient resource - ex: "Patient/{patient id}"

#### Observation: 
valueQuantity.value = int or float  

# License
[MIT](https://choosealicense.com/licenses/mit/)
