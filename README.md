# TherapyDashboard
## To start the program (current iteration):
1: Start mongoDB on default port (27017)  
2: Start FHIR server on http://localhost:8080/hapi/baseDstu3 (or change url in /Services/FHIRRepository.cs)  
3: Launch application (VS or IIS)

## To replace calculation functions:
Implement any of the given interfaces in the /Services folder: IAggregationFunction, IFlagFunction, IWarningFunction.  
Then replace an object of the corresponding interface in HomeController Index() with the custom function object.  
Each of the required functions in the interfaces will take in a List\<QuestionnaireResponse\> and return string or List\<string\>

## Required fields in FHIR Resources, in addition to FHIR mandatory fields (currently subject to change):
### Questionnaire:
Name = string  
Title = string

### QuestionnaireResponse:
Items = List with item.answer.valueX where X is Integer or Decimal && item.text = string  
Questionnaire.reference = canonical questionnaire uri - ex: "Questionnaire/{questonnaire id}"  
Authored = Date or DateTime  
Author = Reference to Patient resource - ex: "Patient/{patient id}"

### Observation: 
valueQuantity.value must be int or float  
