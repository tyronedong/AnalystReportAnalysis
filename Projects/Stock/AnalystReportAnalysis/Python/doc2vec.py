# import modules & set up logging
import gensim, logging

def train_save():
	logging.basicConfig(format='%(asctime)s : %(levelname)s : %(message)s', level=logging.INFO)

	# documents = gensim.models.doc2vec.TaggedLineDocument('E:\data\seg_result_1.txt')
	# sentences = [['first', 'sentence'], ['second', 'sentence']]
	# train word2vec on the two sentences
	documents = gensim.models.doc2vec.TaggedLineDocument('E:\seg_result.txt')
	model = gensim.models.Doc2Vec(documents, size=100, window=8, min_count=1, workers=4)

	model.save('D:\workingwc\Stock\AnalystReportAnalysis\Python\model\doc2vec.model')


def load():
	model = gensim.models.Doc2Vec.load('D:\workingwc\Stock\AnalystReportAnalysis\Python\model\doc2vec.model')
	print len(model.docvecs)

def out():
	model = gensim.models.Doc2Vec.load('D:\workingwc\Stock\AnalystReportAnalysis\Python\model\doc2vec.model')
	# out = file('review_pure_text_vector.txt', 'w')
	for idx, docvec in enumerate(model.docvecs):
	    for value in docvec:
			# out.write(str(value) + ' ')
		# out.write('\n')
		print idx
		print docvec
	# out.close()
# train()
load()
# out()
