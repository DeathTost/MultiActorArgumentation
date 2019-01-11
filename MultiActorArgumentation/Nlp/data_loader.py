import csv
import pickle
from PyPDF2 import PdfFileReader
import re

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

def write_text_to_csv(sentences, file):
    with open(file, "w", newline='') as csv_file:
        writer = csv.writer(csv_file, delimiter=';')
        writer.writerow(['Value', 'Text'])
        for line in sentences:
            writer.writerow(['', line.encode("utf-8")])

def read_csv(file, delimiter = ';'):
    with open(file, "r", newline='') as csv_file:
        reader = csv.reader(csv_file, delimiter=delimiter)
        new_output = []
        next(reader)
        for line in reader:
            new_output.append((line[0], line[1]))
        return new_output
        

def write_paragraphs_to_csv(text, file):
    paragraphs = split_into_paragraphs(text)
    with open (file, "w", newline = '') as csv_file:
        writer = csv.writer(csv_file, delimiter='#')
        writer.writerow(['Value', 'Text'])
        for paragraph in paragraphs:
            writer.writerow(['', paragraph.encode("utf-8")])
            
def split_into_paragraphs(text):
    paragraphs = re.split("\s*Article *[0-9]+\. *|\s*\u00A7 *[0-9]+\. *|\s*Article *[0-9]+\. *\u00A7 *[0-9]+\. ", text)
    paragraphs_not_empty = []
    for p in paragraphs:
        if (p != None) and (p != ""):
            paragraphs_not_empty.append(p)
    return paragraphs_not_empty
    
def read_paragraphs_from_csv(file):
    return read_csv(file, '#')
    
def read_text_of_paragraphs(file):
    paragraph_tuples = read_paragraphs_from_csv(file)
    return [paragraph_text for (paragraph_label, paragraph_text) in paragraph_tuples]
    
def read_labels_of_paragraphs(file):
    paragraph_tuples = paragraph_tuples = read_paragraphs_from_csv(file)
    return [paragraph_label for (paragraph_label, paragraph_text) in paragraph_tuples]
    
def split_into_articles(text):
    articles = re.split("Article *[0-9]+\. *", text)
    return ["Article " + str(i+1) + ". " + articles[i+1] for i in range(len(articles)-1)]
    
def split_articles_into_paragraphs(articles):
    articles_dict = {}
    for a in articles:
        paragraphs = re.split("\u00A7 *[0-9]+\. *", a)
        article_name = paragraphs[0]
        if len(paragraphs) > 1:
            articles_dict[article_name] = ["\u00A7 " + str(i+1) + ". " + paragraphs[i+1] for i in range(len(paragraphs)-1)]
        else:
            article_name_match = re.fullmatch("Article *[0-9]+\. ", article_name)
            print (article_name_match)
            if article_name_match != None:
                article_name_only = article_name_match[0]
                article_content = re.split("Article *[0-9]+\. ", article_name)[1]
                articles_dict[article_name] = article_content
    return articles_dict


#labels = LabelEncoder()
#y = labels.fit_transform(["pos","neg"])
#model = load_model('text_classifier_model')#build(RandomForestClassifier(), ["John got a new job.", "Bob is handsome."], y)
#save_model(model)
#pred = model.predict(["John got a new job."])
#pred3 = predict_arguments(model, ["John got a new job.", "Alice is poor."])
#######################
#text = read_pdf_to_text('Poland_Penal_Code.pdf')
paragraphs = read_labels_of_paragraphs("paragraphs_labeled.csv")
print (paragraphs)
for p in paragraphs:
    print (p)
#print (text)
#paras = split_into_paragraphs(text)
#for p in paras:
#    print (p)
#    print ("---------------")
#write_paragraphs_to_csv(text, "paragraphs.csv")
#articles = split_into_articles(text)
#for a in articles:
#    print (a)
#    print ("---------------")
#articles_dict = split_articles_into_paragraphs(articles)
#for key in articles_dict:
#    print (key)
#    print ("-------------------------")
#    for paragraph in articles_dict[key]:
#        print (paragraph)
#        print ("-------------------------")

#########################################
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