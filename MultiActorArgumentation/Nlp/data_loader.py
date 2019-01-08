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
from collections import Counter
import math
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

    if text != "":
        text = text
    return text

def save_model(model, file):
    with open(file, 'wb') as picklefile:
        pickle.dump(model, picklefile)

def load_model(file):
    with open(file, 'rb') as training_model:
        return pickle.load(training_model)




#labels = LabelEncoder()
#y = labels.fit_transform(["pos","neg"])
#model = load_model('text_classifier_model')#build(RandomForestClassifier(), ["John got a new job.", "Bob is handsome."], y)
#save_model(model)
#pred = model.predict(["John got a new job."])
#pred3 = predict_arguments(model, ["John got a new job.", "Alice is poor."])

#text = read_pdf_to_text('Poland_Penal_Code.pdf')
#sentence = [pair[1] for pair in pred3]
#print (evaluate_sentence(sentence[0], sentence[1], 3))
#count = len(sentence_tokenization(text))
#label = [x % 2 for x in range(0,count)]
#newModel = build(MultinomialNB(), sentence_tokenization(text), label)
#resultPredict = predict_arguments(newModel, sentence_tokenization(text))
#resultPredict = predict_arguments(newModel, ["John got a new job.", "Alice is poor."])
#print (evaluate_sentence("Bob has wife.","Bob has wife and job.", 3))



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