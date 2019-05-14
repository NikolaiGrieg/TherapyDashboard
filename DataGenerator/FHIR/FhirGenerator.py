from fhirclient import client
from fhirclient import models
import fhirclient.models.fhirdate as fhirdate
import fhirclient.models.questionnaireresponse as qresponse
import fhirclient.models.fhirreference as fhirref
import fhirclient.models.observation as obs
from fhirclient.models.bundle import Bundle
from fhirclient.models.questionnaire import Questionnaire

from datetime import datetime, date
import random

import asyncio
settings = {
    'app_id': 'my_web_app',
    "scope": "user/*.*",
    'api_base': 'http://localhost:8080/hapi/baseDstu3',
    # ''http://ec2-18-191-106-51.us-east-2.compute.amazonaws.com/baseDstu3/'
    # 'https://launch.smarthealthit.org/v/r3/sim/eyJoIjoiMSIsImoiOiIxIn0/fhir' #'http://localhost:8080/hapi/baseDstu3'#
}
smart = client.FHIRClient(settings=settings)

import json
import db.Repository as repo


# print(data)

def pretty(js):
    """ pretty print a json object """
    return json.dumps(js, indent=2, ensure_ascii=False)


###Questionnaire
import fhirclient.models.questionnaire as q



def create_sleep_observations(patient_ref):
    form_data = repo.generate_patient_sleep_data()

    for measurement in form_data.items():  # (date, val)
        date = measurement[0]
        value = measurement[1]

        observation = obs.Observation()
        observation.status = "registered"  # <code>
        ref_patient = fhirref.FHIRReference()
        ref_patient.reference = "Patient/" + str(patient_ref)
        observation.subject = ref_patient

        observation.code = models.codeableconcept.CodeableConcept()
        code = models.coding.Coding()
        code.system = "SNOMED CT"
        code.code = "248263006"
        code.display = "Sleep Duration"
        observation.code.coding = [code]

        quantity = models.quantity.Quantity()
        quantity.value = value
        quantity.unit = "h"
        quantity.system = "http://unitsofmeasure.org"
        quantity.code = "h"
        observation.valueQuantity = quantity

        observation.effectiveDateTime = fhirdate.FHIRDate(date)

        result = observation.create(smart.server)
        print(result)


def createQuestionnaire(name):
    questionnaire = q.Questionnaire()
    questionnaire.status = "draft"
    questionnaire.name = name
    items = []

    ph_patients = get_patients()
    date_dict = generate_start_dict(ph_patients)
    cur_dates = date_dict["2"]

    if name == "MADRS":
        form_data = repo.generate_patient_MADRS(cur_dates)
        questionnaire.title = "Montgomery And Åsberg Depression Rating Scale"
    elif name == "PHQ9":
        form_data = repo.generate_patient_PHQ9(cur_dates)
        questionnaire.title = "Patient Health Questionnaire"
    measurement = next(iter(form_data.items()))
    categories = measurement[1]

    for i, (key, val) in enumerate(categories.items()):
        questionnaire_item = q.QuestionnaireItem()
        questionnaire_item.text = key
        questionnaire_item.linkId = str(i)
        questionnaire_item.type = "integer"
        items.append(questionnaire_item)

    questionnaire.item = items

    print(pretty(questionnaire.as_json()))

    result = questionnaire.create(smart.server)
    print(result)


### QuestionnaireResponse
def getQuestionnaire(name):
    query = FHIRSearch(resource_type=Questionnaire, struct={'name': name})
    Q_bundle = query.perform(smart.server)
    Q_resource = extract_resources(Q_bundle.entry)[0]

    return "Questionnaire/" + str(Q_resource.id)


def create_QR_for_single_patient(patient_ref, form_data, name):
    for measurement in form_data.items():
        date = measurement[0]
        categories = measurement[1]

        qr = qresponse.QuestionnaireResponse()
        qr.status = "completed"

        qr.authored = fhirdate.FHIRDate(date)

        qr.questionnaire = models.fhirreference.FHIRReference()
        qr.questionnaire.reference = getQuestionnaire(name)

        ref_patient = fhirref.FHIRReference()
        ref_patient.reference = patient_ref
        qr.subject = ref_patient

        answer_set = []

        for i, (key, val) in enumerate(categories.items()):
            qr_item = qresponse.QuestionnaireResponseItem()
            qr_item.text = key
            qr_item.answer = [qresponse.QuestionnaireResponseItemAnswer()]
            qr_item.answer[0].valueInteger = int(val)
            qr_item.linkId = str(i)  # Should correspond to item from Questionnaire
            answer_set.append(qr_item)

        qr.item = answer_set

        result = qr.create(smart.server)
        print(result)

        # measurement = next(iter(form_data.items()))


from fhirclient.models.fhirsearch import FHIRSearch
from fhirclient.models.patient import Patient


def populate_all_patients():
    patients = get_patients()
    date_dict = generate_start_dict(patients)
    for patient in patients:  # TODO make async
        cur_dates = date_dict[patient.id]
        form_data = repo.generate_patient_MADRS(cur_dates)
        create_QR_for_single_patient("Patient/" + str(patient.id), form_data, "MADRS")

    for patient in patients:
        cur_dates = date_dict[patient.id]
        form_data = repo.generate_patient_PHQ9(cur_dates)
        create_QR_for_single_patient("Patient/" + str(patient.id), form_data, "PHQ9")


def get_next_page(bundle):
    links = bundle.link
    for link in links:
        if link.relation == 'next':
            return smart.server.request_json(link.url)


def get_next_url(links):
    for link in links:
        if link.relation == 'next':
            return link.url


def generate_start_dict(patients):
    date_dict = {}
    for patient in patients:
        dt = datetime(2018, 5, 1).toordinal()
        mid = datetime(2018, 12, 1).toordinal()
        end = datetime(2019, 5, 1).toordinal()
        random_start = datetime.fromordinal(random.randint(dt, mid))
        random_end = datetime.fromordinal(random.randint(mid, end))
        date_dict[patient.id] = [random_start, random_end]
    return date_dict


def get_patients():
    query = FHIRSearch(resource_type=Patient)
    patient_bundle = query.perform(smart.server)
    patients = extract_resources(patient_bundle.entry)
    next_page_url = get_next_url(patient_bundle.link)

    while next_page_url is not None:
        response = smart.server.request_json(next_page_url)
        next_page_bund = Bundle(response)
        next_page_resources = extract_resources(next_page_bund.entry)
        patients.extend(next_page_resources)

        next_page_url = get_next_url(next_page_bund.link)

    return patients


def extract_resources(entry):
    resources = []
    if entry != None:
        for bund_entry in entry:
            resources.append(bund_entry.resource)
        return resources
    else:
        return None


#createQuestionnaire("MADRS")
populate_all_patients()

#create_sleep_observations(2)
