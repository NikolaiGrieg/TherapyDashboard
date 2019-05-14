import numpy as np
import math



import datetime

dt = datetime.datetime(2018, 6, 1)
end = datetime.datetime(2018, 12, 30)
step = datetime.timedelta(days=7)
default_max_timesteps = 30


def generate_MADRS(dt=dt, end=end, step=step):
    cols = {}
    categories = ['Reported sadness', 'Inner tension', 'Reduced sleep', 'Reduced appetite', 'Concentration difficulties',
                  'Lassitude', 'Inability to feel', 'Pessimistic thoughts', 'Suicidal thoughts',
                  'date']
    dates = []
    max_val = 6
    min_val = 0
    while dt < end:
        dates.append(dt.strftime('%Y-%m-%d'))
        dt += step
    for i in range(len(dates)):
        decrement_fraction = 1 / max(default_max_timesteps, len(dates))
        mu = max_val - (min_val + 2 * (i * decrement_fraction))  # starts at 6, goes towards 2
        variance = 1 - (i * decrement_fraction)
        sigma = math.sqrt(variance)
        col = np.random.normal(mu, sigma, 9)
        col = [min(max(x, min_val), max_val) for x in col]
        #row = [min(x, max_val) for x in row]
        col_dict = {}
        for j in range(len(col)):
            col_dict[categories[j]] = col[j]

        cols[dates[i]] = col_dict

    return cols

def generate_PHQ9(dt=dt, end=end, step=step):
    cols = {}
    text = ['Little interest or pleasure in doing things', 'Feeling down, depressed, or hopeless',
            'Trouble falling or staying asleep, or sleeping too much', 'Feeling tired or having little energy',
            'Poor appetite or overeating', 'Feeling bad about yourself or '
            'that you are a failure or have let yourself or your family down',
            'Trouble concentrating on things, such as reading the newspaper or watching television',
            'Moving or speaking so slowly that other people could have noticed. Or the opposite being'
            ' so figety or restless that you have been moving around a lot more than usual',
            'Thoughts that you would be better off dead, or of hurting yourself']
    max_val = 4
    min_val = 0
    dates = []
    while dt < end:
        dates.append(dt.strftime('%Y-%m-%d'))
        dt += step
    for i in range(len(dates)):
        decrement_fraction = 1 / max(default_max_timesteps, len(dates))
        mu = max_val - (min_val + 1 * (i * decrement_fraction))  # starts at 4, goes towards 1
        variance = 0.5 - (i * decrement_fraction * 0.5)
        sigma = math.sqrt(variance)
        col = np.random.normal(mu, sigma, 9)
        col = [min(max(x, min_val), max_val) for x in col]
        #row = [min(x, max_val) for x in row]
        col_dict = {}
        for j in range(len(col)):
            col_dict[text[j]] = col[j]

        cols[dates[i]] = col_dict

    return cols


def generate_sleep_data(dt=dt, end=datetime.datetime(2018, 10, 30), step=datetime.timedelta(days=1)):
    rows = {}

    dates = []
    max_val = 10 #hours
    min_val = 6
    while dt < end:
        dates.append(dt.strftime('%Y-%m-%d'))
        dt += step
    for i in range(len(dates)):
        decrement_fraction = 1 / len(dates)
        mu = 8
        variance = 1.20 - (i * decrement_fraction*1.20)
        sigma = math.sqrt(variance)
        sleep_duration = np.random.normal(mu, sigma, 1)[0]
        sleep_duration = max(sleep_duration, min_val)
        sleep_duration = min(sleep_duration, max_val)
        rows[dates[i]] = sleep_duration

    return rows