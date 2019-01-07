import nltk
import string
import pickle
from PyPDF2 import PdfFileReader
from nltk.tokenize import sent_tokenize, word_tokenize, wordpunct_tokenize
from nltk import pos_tag
from nltk.corpus import stopwords
from nltk.stem import PorterStemmer
from nltk.stem.wordnet import WordNetLemmatizer
from nltk.corpus import wordnet
from nltk.util import ngrams
from sklearn.base import BaseEstimator, TransformerMixin
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.feature_extraction.text import TfidfTransformer
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import SGDClassifier
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import classification_report, confusion_matrix, accuracy_score

def read_pdf_to_text(pdf_path):
    pdf_file = open(pdf_path, 'rb')
    read_pdf = PdfFileReader(pdf_file)
    number_of_pages = read_pdf.getNumPages()
    text = ""

    for page_number in range(number_of_pages):
        page = read_pdf.getPage(page_number)
        page_content = page.extractText()
        text += page_content
        #text_file.write(page_content.encode('utf-8'))

    if text != "":
        text = text
    return text

class NLTKPreprocessor(BaseEstimator, TransformerMixin):

    def __init__(self, stopwords=None, punct=None,
                 lower=True, strip=True):
        self.lower      = lower
        self.strip      = strip
        self.stopwords  = stopwords or set(nltk.corpus.stopwords.words('english'))
        self.punct      = punct or set(string.punctuation)
        self.lemmatizer = WordNetLemmatizer()

    def fit(self, X, y=None):
        return self

    def inverse_transform(self, X):
        return [" ".join(doc) for doc in X]

    def transform(self, X):
        return [
            list(self.tokenize(doc)) for doc in X
        ]

    def tokenize(self, document):
        # Break the document into sentences
        for sent in sent_tokenize(document):
            # Break the sentence into part of speech tagged tokens
            for token, tag in pos_tag(wordpunct_tokenize(sent)):
                # Apply preprocessing to the token
                token = token.lower() if self.lower else token
                token = token.strip() if self.strip else token
                token = token.strip('_') if self.strip else token
                token = token.strip('*') if self.strip else token

                # If stopword, ignore token and continue
                if token in self.stopwords:
                    continue

                # If punctuation, ignore token and continue
                if all(char in self.punct for char in token):
                    continue

                # Lemmatize the token and yield
                lemma = self.lemmatize(token, tag)
                yield lemma

    def lemmatize(self, token, tag):
        tag = {
            'N': wordnet.NOUN,
            'V': wordnet.VERB,
            'R': wordnet.ADV,
            'J': wordnet.ADJ
        }.get(tag[0], wordnet.NOUN)

        return self.lemmatizer.lemmatize(token, tag)


def cleanup(tokens):
    stop_words = stopwords.words('english')
    keywords = [word for word in tokens if not word in stop_words]
    return keywords

def sentence_tokenization(text):
    sen_tokens = sent_tokenize(text)
    return sen_tokens

def word_tokenization(text):
    word_tokens = word_tokenize(text)
    return word_tokens

def stemming(words):
    ps = PorterStemmer()
    stemmed_words = []
    for w in words:
        stemmed_words.append(ps.stem(w))
    return stemmed_words

def lemmatization(tagged_words):
    lemmatizer = WordNetLemmatizer()
    lemmatized_words = []
    for word, tag in tagged_words:
        wntag = get_wordnet_pos(tag)
        if wntag is None:  # not supply tag in case of None
            lemma = lemmatizer.lemmatize(word)
        else:
            lemma = lemmatizer.lemmatize(word, pos=wntag)
        lemmatized_words.append((lemma,tag))
    return lemmatized_words

def pos_tagging(tokens):
    return nltk.pos_tag(tokens)

def get_wordnet_pos(word_tag):

    if word_tag.startswith('J'):
        return wordnet.ADJ
    elif word_tag.startswith('V'):
        return wordnet.VERB
    elif word_tag.startswith('N'):
        return wordnet.NOUN
    elif word_tag.startswith('R'):
        return wordnet.ADV
    else:
        return None

def get_ngrams(tokens, n ):
    n_grams = ngrams(tokens, n)
    return [ ' '.join(grams) for grams in n_grams]














def save_model(model):
    with open('text_classifier_model', 'wb') as picklefile:
        pickle.dump(model, picklefile)

def load_model():
    with open('text_classifier_model', 'rb') as training_model:
        return pickle.load(training_model)

def classify_naive_bayes(train, labels):
    classifier = MultinomialNB().fit(train, labels)
    return classifier

def classify_svm(train, labels):
    classifier = SGDClassifier().fit(train, labels)
    return classifier

def classify_random_forest(train, labels):
    classifier = RandomForestClassifier(n_estimators=1000, random_state=0).fit(train, labels)
    return classifier

def identity(arg):
    return arg

def predict(classifier, test):
    return classifier.predict(test)

def build(classifier, X, y=None):
        model = Pipeline([
            ('preprocessor', NLTKPreprocessor()),
            ('vectorizer', TfidfVectorizer(
                tokenizer=identity, preprocessor=None, lowercase=False
            )),
            ('classifier', classifier),])

        model.fit(X, y)
        return model


#labels = LabelEncoder()
#y = labels.fit_transform(["pos","neg"])
#model = build(RandomForestClassifier(), ["John got a new job.", "Bob is handsome."], y)
#pred = model.predict(["John got a new job."])







#text = read_pdf_to_text('Poland_Penal_Code.pdf')
#print (text)
#sentences = sentence_tokenization(text)
#tokens = word_tokenization(text)
#cleaned_tokens = cleanup(tokens)
#stemmed_tokens = stemming(cleaned_tokens)
#tagged_tokens = pos_tagging(tokens)
#lemmatized_tokens = lemmatization(tagged_tokens)
#unigrams = get_ngrams(tokens, 1)
#bigrams = get_ngrams(tokens, 2)
#trigrams = get_ngrams(tokens, 3)




#TF
#vectorizer = CountVectorizer(max_features=1500, min_df=5, max_df=0.7, stop_words=stopwords.words('english'))
#X = vectorizer.fit_transform(sentences).toarray()
#IDF
#tfidfconverter = TfidfTransformer()
#X = tfidfconverter.fit_transform(X).toarray()
#TF + IDF
#tfidfconverter = TfidfVectorizer(max_features=1500, min_df=5, max_df=0.7, stop_words=stopwords.words('english'))
#X = tfidfconverter.fit_transform(sentences).toarray()

#count_vect = CountVectorizer(stop_words='english',ngram_range = (1,1))
#x_counts = count_vect.fit_transform(tokens)

#tfidf_transformer = TfidfTransformer()
#x_tfidf = tfidf_transformer.fit_transform(x_counts)



# basic stuff is from nltk package
# TODO bag-of-words, TF-IDF and other stuff should ne from scikit-learns packages
#print ("DONE")
#exit()

# 0. Load data from pdf as text
# 1. Gather data and label train examples
# 2. Clean your dataset
# 2.a) Remove irrelevant characters
# 2.b) Tokenize text to words [TOKENIZATION]
# 2.c) Remove words that are not relevant (stopwords etc.) [REMOVE STOPWORDS]
# 2.d) Convert all characters to lowercase
# 2.e) Lemmatization {STEMMING AND LEMMATIZATION]
# 3. Find a good data representation [BAG OF WORDS, TF-IDF, WORD EMBEDDINGS]
# 4. Find relevant features {MODEL BUILDING]
# 5. Classification [BAYES, RANDOM TREE, SVM MAYBE]
# 6. Inspect the results (confusion matrix) [MODEL EVALUATION]