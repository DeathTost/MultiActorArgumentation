from data_processing import NLTKPreprocessor
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import SGDClassifier
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import confusion_matrix
from sklearn import metrics

def identity(arg):
    return arg

def predict_arguments(model, test):
    result = []
    predicate = model.predict(test)
    for i in range(0,len(predicate)):
        result.append((predicate[i], test[i]))

    return result

def encode_labels(train_labels):
    labels = LabelEncoder()
    return labels.fit_transform(train_labels)


def build(classifier, X, y=None):
        model = Pipeline([
            ('preprocessor', NLTKPreprocessor()),
            ('vectorizer', TfidfVectorizer(
                tokenizer=identity, preprocessor=None, lowercase=False
            )),
            ('classifier', classifier),])

        model.fit(X, y)
        return model

def build_MultinomialNB(X, y=None):
    return build(MultinomialNB(), X, y)

def build_RandomForest(X, y=None):
    return build(RandomForestClassifier(), X, y)

def build_SVM(X, y=None):
    return build(SGDClassifier(), X, y)
	
def display_metrics(test_labels, predicted_labels):
    print(confusion_matrix(test_labels, predicted_labels))
    print(metrics.classification_report(test_labels, predicted_labels))	

def get_positive_paragraphs(paragraphs):
    result = []
    for label, paragraph in paragraphs:
        if label == '1':
            result.append(paragraph)
    return result

def get_negative_paragraphs(paragraphs):
    result = []
    for label, paragraph in paragraphs:
        if label == '2':
            result.append(paragraph)
    return result