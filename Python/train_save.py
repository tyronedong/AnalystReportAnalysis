import gensim, logging
from optparse import OptionParser

parser = OptionParser(usage="""usage: %prog [options] inputfilename""")
parser.add_option("--inputfile", dest="inputfile",  type="string", default="\\", help="input documents"                                                                                 "test_1.jpg)")
parser.add_option("--outputfile", dest="outputfile", default="\\", type="string", help="output model file")
(option, args) = parser.parse_args()
input_filename = option.inputfile
output_filename = option.outputfile

logging.basicConfig(format='%(asctime)s : %(levelname)s : %(message)s', level=logging.INFO)

documents = gensim.models.doc2vec.TaggedLineDocument(input_filename)
model = gensim.models.Doc2Vec(documents, size=100, window=8, min_count=5, workers=4)
model.save(output_filename)
