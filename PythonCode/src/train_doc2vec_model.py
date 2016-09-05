import gensim, logging
import multiprocessing
from optparse import OptionParser

parser = OptionParser(usage="""usage: %prog [options] inputfilename""")
parser.add_option("--inputfile", dest="inputfile",  type="string", default="\\", help="input documents"                                                                                 "test_1.jpg)")
parser.add_option("--outputfile1", dest="outputfile1", default="\\", type="string", help="output model file")
parser.add_option("--outputfile2", dest="outputfile2", default="\\", type="string", help="output model file c format")
(option, args) = parser.parse_args()
input_filename = option.inputfile
output_filename1 = option.outputfile1
output_filename2 = option.outputfile2

logging.basicConfig(format='%(asctime)s : %(levelname)s : %(message)s', level=logging.INFO)

documents = gensim.models.doc2vec.TaggedLineDocument(input_filename)
model = gensim.models.Doc2Vec(documents, size=200, window=8, min_count=5, workers=multiprocessing.cpu_count())
model.save(output_filename1)
model.save_word2vec_format(output_filename2, binary=False)
