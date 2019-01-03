import nltk

def methodA():
    train = [
    (dict(a=1,b=1,c=1), 'y'),
    (dict(a=1,b=1,c=1), 'x'),
    (dict(a=1,b=1,c=0), 'y'),
    (dict(a=0,b=1,c=1), 'x'),
    (dict(a=0,b=1,c=1), 'y'),
    (dict(a=0,b=0,c=1), 'y'),
    (dict(a=0,b=1,c=0), 'x'),
    (dict(a=0,b=0,c=0), 'x'),
    (dict(a=0,b=1,c=1), 'y'),
    ]
    test = [
    (dict(a=1,b=0,c=1)), # unseen
    (dict(a=1,b=0,c=0)), # unseen
    (dict(a=0,b=1,c=1)), # seen 3 times, labels=y,y,x
    (dict(a=0,b=1,c=0)), # seen 1 time, label=x
    ]

    classifier = nltk.classify.NaiveBayesClassifier.train(train)
    classifier.classify_many(test)

    return test
