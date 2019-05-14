# TherapyDashboard

## Application description:

### Master View:  
![Picture of master view](https://github.com/NikolaiGrieg/TherapyDashboard/blob/master/Pictures/Dashboard_master_14_5.png)


### Detail view:  
![Picture of detail view](https://github.com/NikolaiGrieg/TherapyDashboard/blob/master/Pictures/DetailView_14_5.png)


## For development:
### To start the program (current iteration):
1: Start mongoDB on default port (27017)  
2: Start FHIR server and set URL of the FHIR base endpoint in /Services/FHIRRepository.cs  
2.5: Seed FHIR server with data.  
3: Set parameters for default functions for the updateGlobalState method in /Services/FHIRRepository.cs  
3(cont): The string parameter for default function takes a resource ID of a questionnaire on the resource server.  
3.5 Alternatively to 3: Set custom calculation functions in updateGlobalState method in /Services/FHIRRepository.cs
4: Launch application (VS or IIS)

### To set up IIS on windows server:
See sections "Install IIS" and "Publish to IIS" at  
https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/visual-studio-web-deployment/deploying-to-iis

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
valueQuantity.value must be int or float  

# License
[MIT](https://choosealicense.com/licenses/mit/)
