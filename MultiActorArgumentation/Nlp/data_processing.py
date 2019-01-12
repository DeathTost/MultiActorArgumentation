import nltk
import string
from nltk.tokenize import sent_tokenize, word_tokenize, wordpunct_tokenize
from nltk import pos_tag
from nltk.corpus import stopwords
from nltk.stem import PorterStemmer
from nltk.stem.wordnet import WordNetLemmatizer
from nltk.corpus import wordnet
from nltk.util import ngrams
from sklearn.base import BaseEstimator, TransformerMixin
from collections import Counter
import math

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
        for sent in sent_tokenize(document[2:]):
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

def jaccard_distance(a, b):
    """Calculate the jaccard distance between sets A and B"""
    a = set(a)
    b = set(b)
    return 1.0 * len(a & b) / len(a | b)


def cosine_similarity_ngrams(a, b):
    vec1 = Counter(a)
    vec2 = Counter(b)

    intersection = set(vec1.keys()) & set(vec2.keys())
    numerator = sum([vec1[x] * vec2[x] for x in intersection])

    sum1 = sum([vec1[x] ** 2 for x in vec1.keys()])
    sum2 = sum([vec2[x] ** 2 for x in vec2.keys()])
    denominator = math.sqrt(sum1) * math.sqrt(sum2)

    if not denominator:
        return 0.0
    return float(numerator) / denominator

def evaluate_sentence(pattern, test, n):
    pattern_tokens = lemmatization(pos_tagging(cleanup(word_tokenization(pattern))))
    pattern_tokens = [text for (text, label) in pattern_tokens if text not in string.punctuation]
    test_tokens = lemmatization(pos_tagging(cleanup(word_tokenization(test))))
    test_tokens= [text for (text, label) in test_tokens if text not in string.punctuation]
    score = []
    for i in range (1,n+1):
        pattern_ngrams = get_ngrams(pattern_tokens, i)
        test_ngrams = get_ngrams(test_tokens, i)
        score.append(jaccard_distance(pattern_ngrams, test_ngrams))

    return score

def evaluate_sentences(pattern_sentences, test_sentences, n):
    result = []
    for pattern in pattern_sentences:
        for test in test_sentences:
            result.append(evaluate_sentence(pattern, test, n))
    return result