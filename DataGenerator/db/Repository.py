import DataGenerator as gen


def generate_patient_MADRS(dates):
    return gen.generate_MADRS(dt=dates[0], end=dates[1])


def generate_patient_PHQ9(dates):
    return gen.generate_PHQ9(dt=dates[0], end=dates[1])


def generate_patient_sleep_data():
    return gen.generate_sleep_data()
