using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Text.Handler
{
    [StructLayout(LayoutKind.Explicit)]
    public struct result_t
    {
        [FieldOffset(0)]
        public int start;
        [FieldOffset(4)]
        public int length;
        [FieldOffset(8)]
        public int sPos1;
        [FieldOffset(12)]
        public int sPos2;
        [FieldOffset(16)]
        public int sPos3;
        [FieldOffset(20)]
        public int sPos4;
        [FieldOffset(24)]
        public int sPos5;
        [FieldOffset(28)]
        public int sPos6;
        [FieldOffset(32)]
        public int sPos7;
        [FieldOffset(36)]
        public int sPos8;
        [FieldOffset(40)]
        public int sPos9;
        [FieldOffset(44)]
        public int sPos10;
        //[FieldOffset(12)] public int sPosLow;
        [FieldOffset(48)]
        public int POS_id;
        [FieldOffset(52)]
        public int word_ID;
        [FieldOffset(56)]
        public int word_type;
        [FieldOffset(60)]
        public int weight;
    }
    /*
    struct result_t{
  int start; //start position,词语在输入句子中的开始位置
  int length; //length,词语的长度
  char  sPOS[POS_SIZE];//word type，词性ID值，可以快速的获取词性表
  int	iPOS;//词性标注的编号
  int word_ID; //该词的内部ID号，如果是未登录词，设成0或者-1
  int word_type; //区分用户词典;1，是用户词典中的词；0，非用户词典中的词
  int weight;//word weight,read weight
 };*/

    /// <summary>
    /// In order to correctly init the class, you should put 'NLPIR.dll' and 'Data' file which both 
    /// provided by http://ictclas.nlpir.org/downloads into a childfile named 'NLPIR' of current 
    /// 'bin' file.
    /// </summary>
    public class WordSegHandler
    {
        public bool isValid = false;
        private List<KeyValuePair<string, string>> partitionResult;

        const string path = @".\NLPIR\NLPIR.dll";//设定dll的路径
        //对函数进行申明
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Init")]
        public static extern bool NLPIR_Init(String sInitDirPath, int encoding, String sLicenseCode);

        //特别注意，C语言的函数NLPIR_API const char * NLPIR_ParagraphProcess(const char *sParagraph,int bPOStagged=1);必须对应下面的申明
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcess")]
        public static extern IntPtr NLPIR_ParagraphProcess(String sParagraph, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Exit")]
        public static extern bool NLPIR_Exit();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ImportUserDict")]
        public static extern int NLPIR_ImportUserDict(String sFilename);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcess")]
        public static extern bool NLPIR_FileProcess(String sSrcFilename, String sDestFilename, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcessEx")]
        public static extern bool NLPIR_FileProcessEx(String sSrcFilename, String sDestFilename);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetParagraphProcessAWordCount")]
        static extern int NLPIR_GetParagraphProcessAWordCount(String sParagraph);
        //NLPIR_GetParagraphProcessAWordCount
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessAW")]
        static extern void NLPIR_ParagraphProcessAW(int nCount, [Out, MarshalAs(UnmanagedType.LPArray)] result_t[] result);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_AddUserWord")]
        static extern int NLPIR_AddUserWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_SaveTheUsrDic")]
        static extern int NLPIR_SaveTheUsrDic();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_DelUsrWord")]
        static extern int NLPIR_DelUsrWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Start")]
        static extern bool NLPIR_NWI_Start();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Complete")]
        static extern bool NLPIR_NWI_Complete();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddFile")]
        static extern bool NLPIR_NWI_AddFile(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_AddMem")]
        static extern bool NLPIR_NWI_AddMem(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_GetResult")]
        public static extern IntPtr NLPIR_NWI_GetResult(bool bWeightOut = false);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_Result2UserDict")]
        static extern uint NLPIR_NWI_Result2UserDict();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetKeyWords")]
        public static extern IntPtr NLPIR_GetKeyWords(String sText, int nMaxKeyLimit = 50, bool bWeightOut = false);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileKeyWords")]
        public static extern IntPtr NLPIR_GetFileKeyWords(String sFilename, int nMaxKeyLimit = 50, bool bWeightOut = false);

        public WordSegHandler()
        {
            partitionResult = new List<KeyValuePair<string, string>>();
            if (!NLPIR_Init(@".\NLPIR", 0, ""))//给出Data文件所在的路径，注意根据实际情况修改。
            {
                System.Console.WriteLine("Init ICTCLAS failed!");
                isValid = false;
            }
            else { isValid = true; }
        }

        public List<KeyValuePair<string, string>> GetSegmentation(string text)
        {
            List<KeyValuePair<string, string>> segmentationResult = new List<KeyValuePair<string, string>>();
            IntPtr intPtr = NLPIR_ParagraphProcess(text);//切分结果保存为IntPtr类型
            String str = Marshal.PtrToStringAnsi(intPtr);//将切分结果转换为string

            string[] parArray = str.Split(' ');
            foreach (string curPar in parArray)
            {
                string[] wordC = curPar.Split('/');
                if (wordC.Length != 2) { continue; }
                segmentationResult.Add(new KeyValuePair<string, string>(wordC[1], wordC[0]));
            }

            return segmentationResult;
        }

        public bool ExecutePartition(string text)
        {
            partitionResult.Clear();
            IntPtr intPtr = NLPIR_ParagraphProcess(text);//切分结果保存为IntPtr类型
            String str = Marshal.PtrToStringAnsi(intPtr);//将切分结果转换为string

            string[] parArray = str.Split(' ');
            foreach (string curPar in parArray)
            {
                string[] wordC = curPar.Split('/');
                if (wordC.Length != 2) { continue; }
                partitionResult.Add(new KeyValuePair<string, string>(wordC[1], wordC[0]));
            }

            return true;
        }

        public string[] GetNoStopWords()
        {
            List<string> noStopWords = new List<string>();
            foreach (var kvp in partitionResult)
            {
                if (kvp.Key.StartsWith("w")) 
                { continue; }
                if(kvp.Key.StartsWith("u"))
                { continue; }
                if(kvp.Key.StartsWith("m"))
                { continue; }
                //Console.ReadLine();
                noStopWords.Add(kvp.Value);
            }
            return noStopWords.ToArray();
        }

        public string[] GetNouns()
        {
            List<string> nouns = new List<string>();
            foreach (var kvp in partitionResult)
            {
                if (kvp.Value.Length == 1) { continue; }
                switch (kvp.Key)
                {
                    case "n": nouns.Add(kvp.Value); break;
                    case "nz": nouns.Add(kvp.Value); break;
                    case "nl": nouns.Add(kvp.Value); break;
                    case "ng": nouns.Add(kvp.Value); break;
                    default: break;
                }
            }
            return nouns.ToArray();
        }

        public string[] GetAdjs()
        {
            List<string> adjs = new List<string>();
            foreach (var kvp in partitionResult)
            {
                if (kvp.Value.Length == 1) { continue; }
                switch (kvp.Key)
                {
                    case "a": adjs.Add(kvp.Value); break;
                    case "ad": adjs.Add(kvp.Value); break;
                    case "an": adjs.Add(kvp.Value); break;
                    case "ag": adjs.Add(kvp.Value); break;
                    case "al": adjs.Add(kvp.Value); break;
                    default: break;
                }
            }
            return adjs.ToArray();
        }

        public string[] GetAll()
        {
            List<string> all = new List<string>();
            foreach (var kvp in partitionResult) { all.Add(kvp.Value); }
            return all.ToArray();
        }
    }
}
