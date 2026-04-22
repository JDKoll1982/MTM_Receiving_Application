INSERT INTO settings_volvo_recipents (
    first_name,
    last_name,
    recipient_type,
    email
)
VALUES
    ('Jose', 'Rosas', 'To', 'jrosas@mantoolmfg.com'),
    ('Sandy', 'Miller', 'To', 'smiller@mantoolmfg.com'),
    ('Steph', 'Wittmus', 'To', 'swittmus@mantoolmfg.com'),
    ('Debra', 'Alexander', 'CC', 'dalexander@mantoolmfg.com'),
    ('Michelle', 'Laurin', 'CC', 'mlaurin@mantoolmfg.com')
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    last_name = VALUES(last_name),
    email = VALUES(email);

INSERT INTO settings_reporting_recipients (
    first_name,
    last_name,
    recipient_type,
    email
)
VALUES
    ('Michelle', 'Laurin', 'To', 'mlaurin@mantoolmfg.com'),
    ('Charles', 'Ehlenbeck', 'To', 'CEhlenbeck@mantoolmfg.com'),
    ('Debra', 'Alexander', 'To', 'dalexander@mantoolmfg.com'),
    ('Material', 'Handlers', 'To', 'mhandler@mantoolmfg.com'),
    ('Valerie', 'Kingsbury', 'To', 'VKingsbury@mantoolmfg.com'),
    ('Scott', 'Carbon', 'To', 'scarbon@mantoolmfg.com'),
    ('Nick', 'Wunsch', 'To', 'NWunsch@mantoolmfg.com'),
    ('Shawn', 'Snyder', 'To', 'ssnyder@mantoolmfg.com'),
    ('Production', 'lead', 'To', 'Productionlead@mantoolmfg.com'),
    ('Bill', 'Schmidt', 'To', 'BSchmidt@mantoolmfg.com'),
    ('Angela', 'Beeman', 'To', 'abeeman@mantoolmfg.com'),
    ('Amanda', 'Groelle', 'To', 'agroelle@mantoolmfg.com'),
    ('Sandy', 'Miller', 'To', 'smiller@mantoolmfg.com'),
    ('Cristofer', 'Muchowski', 'To', 'CMuchowski@mantoolmfg.com'),
    ('Steph', 'Wittmus', 'To', 'swittmus@mantoolmfg.com'),
    ('Jose', 'Rosas', 'To', 'jrosas@mantoolmfg.com'),
    ('Tim', 'Raddatz', 'To', 'traddatz@mantoolmfg.com'),
    ('Material Handler', 'Lead', 'To', 'mhlead@mantoolmfg.com')
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    last_name = VALUES(last_name),
    email = VALUES(email);