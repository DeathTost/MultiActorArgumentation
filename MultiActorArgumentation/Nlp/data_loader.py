import nltk
from PyPDF2 import PdfFileReader
from nltk.tokenize import sent_tokenize, word_tokenize
from nltk.corpus import stopwords
from nltk.stem import PorterStemmer
from nltk.stem.wordnet import WordNetLemmatizer
from nltk.corpus import wordnet
from nltk.util import ngrams

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

text = read_pdf_to_text('Poland_Penal_Code.pdf')
print (text)
sentences = sentence_tokenization(text)
tokens = word_tokenization(text)
cleaned_tokens = cleanup(tokens)
stemmed_tokens = stemming(cleaned_tokens)
tagged_tokens = pos_tagging(tokens)
lemmatized_tokens = lemmatization(tagged_tokens)
unigrams = get_ngrams(tokens, 1)
bigrams = get_ngrams(tokens, 2)
trigrams = get_ngrams(tokens, 3)
# basic stuff is from nltk package
# TODO bag-of-words, TF-IDF and other stuff should ne from scikit-learns packages
print ("DONE")
exit()

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