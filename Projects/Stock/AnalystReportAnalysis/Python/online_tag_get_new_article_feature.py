#!/usr/bin/python
# -*- coding: utf-8 -*-

from gensim import models, matutils
import numpy as np
from numpy import float32 as REAL
import os
import time
import LDAModule
import threading
import multiprocessing
import lda.model
import copy


def train_new_article_and_get_content_doc2vec(model):

	#t1 = time.time()
	
	#t2 = time.time()
	f = file('temp/article_content_cut_without_stopwords.txt', 'r')
	#print "Doc2vec Open File Time : " + str(time.time() - t2)
	
	#t2 = time.time()
	line = f.readline()
	#print "Doc2vec Read File Time : " + str(time.time() - t2)
	
	#t2 = time.time()
	line = line.decode('utf-8').split()
	#print "Doc2vec Split Time : " + str(time.time() - t2)
	
	#t2 = time.time()
	f.close()
	#print "Doc2vec Close File Time : " + str(time.time() - t2)
	
	#sentences = []
	#string = 'sent_1'
	#sentence = models.doc2vec.LabeledSentence(words = line, tags = [string])
	#sentences.append(sentence)
	
	#model.update_vocab(sentences)
	#model.min_alpha_yet_reached = 0.025
	#model.train(sentences)
	
	#t = time.time()
	vec = model.infer_vector(line)
	#print "Doc2vec infer vector Time : " + str(time.time() - t)
	
	#t = time.time()
	vec = matutils.unitvec(vec)
	#print "Doc2vec unitvec Time : " + str(time.time() - t)
	
	#t = time.time()
	vec = list(vec)
	#print "Doc2vec List Time : " + str(time.time() - t)
	
	#t = time.time()
	#model.init_sims()
	#print "Doc2vec init sims Time : " + str(time.time() - t)
	
	#print "Content Doc2vec Time ! : " + str(time.time() - t1)
	
	return vec


def get_title_word2vec(model):

	f = file('temp/article_title_cut_without_stopwords.txt', 'r')
	
	line = f.readline()
	line = line.decode('utf-8').split()
	mean = []
	for word in line:
		try:
			mean.append( model.syn0norm[model.vocab[word].index] )
		except KeyError, e:
			pass
	if len(mean) == 0:
		mean.append( np.zeros(128) )
	mean = matutils.unitvec( np.array(mean).mean(axis=0)).astype(REAL)
	vec = list(mean)
	
	f.close()
	
	return vec
	
	
#def get_doc2vec(model):
	
	#vectors = model.docvecs.doctag_syn0
	
	#print model.docvecs
	#print "len(vectors) = " + str(len(vectors))
	#print "len(model.docvecs) = " + str(len(model.docvecs))

	#vec = list(matutils.unitvec(np.array([ vectors[ 514790 ] ]).mean(axis=0)).astype(REAL))
	
	#vec = model.docvecs['sent_1']
	
	#return vec


def get_lda(model):

	#os.popen('GibbsLDA++-0.2/src/lda -inf -dir lda_model/ -model model-final -dfile article_content_for_lda.txt')
	
	#model = LDAModule.prepare()
	#print "The End of Preprocessing"
	LDAModule.train_new_article(model)
	#print "The End of Processing"

	f = file('lda_model/article_content_for_lda.txt.theta', 'r')
	
	line = f.readline()
	vec = line.decode('utf-8').split()
	
	f.close()
	
	return vec
	
	
def get_lda_python(lda_model):

	vec = lda_model.inference()
	
	#f = file('lda_model/article_content_for_lda.txt.theta', 'r')
	#
	#line = f.readline()
	#vec = line.decode('utf-8').split()
	#
	#f.close()
	
	return vec
	
	
def prepare():
	
	lda_model = LDAModule.prepare()
	#lda_model = lda.model.Model()
	#lda_model.init()
	doc2vec_model_for_content = models.Doc2Vec.load('train_result_title_content_128/doc2vec')
	doc2vec_model_for_title = copy.deepcopy(doc2vec_model_for_content)
	doc2vec_model_for_title.init_sims()
	return lda_model, doc2vec_model_for_title, doc2vec_model_for_content


#class Doc2vecTitleThread(threading.Thread):
#class Doc2vecTitleThread(multiprocessing.Process):
#
#	def __init__(self, doc2vec_model):
#	
#		#threading.Thread.__init__(self)
#		multiprocessing.Process.__init__(self)
#		self.doc2vec_model = doc2vec_model
#		self.title_word2vec = []
#	
#	def run(self):
#			
#		t = time.time()
#		self.title_word2vec = get_title_word2vec(self.doc2vec_model)
#		print "Title Word2vec Time : " + str(time.time() - t)		
		

#class Doc2vecContentThread(threading.Thread):
#class Doc2vecContentThread(multiprocessing.Process):
#
#	def __init__(self, doc2vec_model):
#	
#		#threading.Thread.__init__(self)
#		multiprocessing.Process.__init__(self)
#		self.doc2vec_model = doc2vec_model
#		self.content_doc2vec = []
#	
#	def run(self):
#	
#		t = time.time()
#		self.content_doc2vec = train_new_article_and_get_content_doc2vec(self.doc2vec_model)
#		print "Content Doc2vec Time : " + str(time.time() - t)
		

#class LDAThread(threading.Thread):
#class LDAThread(multiprocessing.Process):
#
#	def __init__(self, lda_model):
#		
#		#threading.Thread.__init__(self)
#		multiprocessing.Process.__init__(self)
#		self.lda_model = lda_model
#		self.vec_lda = []
#	
#	def run(self):
#	
#		t = time.time()
#		self.vec_lda = get_lda(self.lda_model)
#		#self.vec_lda = get_lda_python(self.lda_model)
#		print "LDA Time : " + str(time.time() - t)


def main(lda_model, doc2vec_model_for_title, doc2vec_model_for_content):

	#print "Tag get new article feature begin"
	
	#doc2vec_title_thread = Doc2vecTitleThread(doc2vec_model_for_title)
	#doc2vec_content_thread = Doc2vecContentThread(doc2vec_model_for_content)
	#lda_thread = LDAThread(lda_model)
	
	#doc2vec_title_thread.start()
	#doc2vec_content_thread.start()
	#lda_thread.start()
	
	#doc2vec_title_thread.join()
	#doc2vec_content_thread.join()
	#lda_thread.join()

	#t = time.time()
	title_word2vec = get_title_word2vec(doc2vec_model_for_title)
	#print "Title Word2vec Time : " + str(time.time() - t)
	
	#t = time.time()
	content_doc2vec = train_new_article_and_get_content_doc2vec(doc2vec_model_for_content)
	#print "Content Doc2vec Time : " + str(time.time() - t)
	
	#t = time.time()
	vec_lda = get_lda(lda_model)
	#print "LDA Time : " + str(time.time() - t)
	
	f = file('temp/new_article_feature_128+128+100', 'w')
	
	#for idx, value in enumerate(lda_thread.vec_lda):
	#for idx, value in enumerate(doc2vec_title_thread.title_word2vec + doc2vec_content_thread.content_doc2vec + lda_thread.vec_lda):
	for idx, value in enumerate(title_word2vec + content_doc2vec + vec_lda):
		f.write(str(idx + 1) + ':' + str(value) + ' ')
	f.write('\n')
	
	f.close()
	
	#print "Tag get new article feature end"


if __name__ == '__main__':
	
	t = time.time()
	lda_model, doc2vec_model_for_title, doc2vec_model_for_content = prepare()
	print "Feature Preprocessing Time : " + str(time.time() - t)
	
	t = time.time()
	main(lda_model, doc2vec_model_for_title, doc2vec_model_for_content)
	print "Feature Processing Time : " + str(time.time() - t)
	
