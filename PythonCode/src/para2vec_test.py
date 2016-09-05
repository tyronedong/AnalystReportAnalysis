__author__ = 'Administrator'

from gensim.models import Doc2Vec

model = Doc2Vec.load('D:\workingwc\Stock\AnalystReportAnalysis\Python\model\doc2vec.model')
print model
