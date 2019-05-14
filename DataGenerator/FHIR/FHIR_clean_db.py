from fhirclient import client
from fhirclient.models.fhirsearch import FHIRSearch
from fhirclient.models.observation import Observation
from fhirclient.models.questionnaireresponse import QuestionnaireResponse
from fhirclient.models.bundle import Bundle

settings = {
    'app_id': 'my_web_app',
    "scope": "user/*.*",
    'api_base': 'http://localhost:8080/hapi/baseDstu3',
# ''http://ec2-18-191-106-51.us-east-2.compute.amazonaws.com/baseDstu3/'
    # 'https://launch.smarthealthit.org/v/r3/sim/eyJoIjoiMSIsImoiOiIxIn0/fhir' #'http://localhost:8080/hapi/baseDstu3'#
}
smart = client.FHIRClient(settings=settings)


def extract_resources(entry):
    resources = []
    for bund_entry in entry:
        resources.append(bund_entry.resource)
    return resources

def extract_observations(entry):
    resources = []
    for bund_entry in entry:
        if bund_entry.resource.code.coding[0].display == "Sleep Duration":
            resources.append(bund_entry.resource)
    return resources

def get_next_url(links):
    for link in links:
        if link.relation == 'next':
            return link.url


def get_all_qrs():
    query = FHIRSearch(resource_type=QuestionnaireResponse)
    QR_bundle = query.perform(smart.server)
    QR_resources = extract_resources(QR_bundle.entry)
    next_page_url = get_next_url(QR_bundle.link)

    while next_page_url is not None:
        response = smart.server.request_json(next_page_url)
        next_page_bund = Bundle(response)
        next_page_resources = extract_resources(next_page_bund.entry)
        QR_resources.extend(next_page_resources)

        next_page_url = get_next_url(next_page_bund.link)

    print(QR_resources)
    return QR_resources


def delete_all_qrs():
    for qr in get_all_qrs():
        resp = qr.delete(smart.server)
        print(resp)


# get_all_qrs()
delete_all_qrs()


def get_all_custom_obs():
    query = FHIRSearch(resource_type=Observation)
    obs_bundle = query.perform(smart.server)
    obs_resources = extract_observations(obs_bundle.entry)
    next_page_url = get_next_url(obs_bundle.link)

    while next_page_url is not None:
        response = smart.server.request_json(next_page_url)
        next_page_bund = Bundle(response)
        next_page_resources = extract_observations(next_page_bund.entry)
        obs_resources.extend(next_page_resources)

        next_page_url = get_next_url(next_page_bund.link)

    print(obs_resources)
    return obs_resources


def delete_all_custom_obs():
    for obs in get_all_custom_obs():
        resp = obs.delete(smart.server)
        print(resp)


#delete_all_custom_obs()
