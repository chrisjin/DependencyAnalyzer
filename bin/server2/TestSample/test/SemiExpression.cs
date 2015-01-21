/*
 * Manual Page:
 * 
 * -----------------------------------------------------------------------------------------------------
 * class SemiExpression
 * SemiExpression se=new SemiExpression()
 * se.Contains(token);                      //check if se contains token 
 * se.FindToken(token);                     //find first specific token
 * se.FindLastToken(token);                 //find last specific token
 * se.FindFirstToken(token);                //find first specific token
 * se.Add(token);                           //add a token to the token list 
 * se.EraseMacro();                         //erase macro that is, tokens begin with a #
 * se.EraseNewLine();                       //erase \n
 * se.EraseStrings();                       //erase "yada yada"
 * se.Merge();                              //combine tokens like "List", "<", "int", ">" to "List<int>"
 * se.IsInRange();                       //check whether a is within the range.
 * -----------------------------------------------------------------------------------------------------
 * 
 * -----------------------------------------------------------------------------------------------------
 * class SemiExtractor:
 * SemiExtractor se=new SemiExtractor();     
 * se.Open(filename);                         //open a file stream
 * SemiExpression semiexp=se.GetNextSemi();   //get next semi detected in the file stream
 * 
 * -----------------------------------------------------------------------------------------------------
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 4  2014 created by Shikai Jin 
 * 
 * ver 1.1  Oct. 7  2014
 * function IsInRange()
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CStoker;
using CSsemi;
namespace CodeAnalyzer
{
    //public class Tokenizer
    //{
    //    public bool Open(string filename)
    //    { 
    //        return Toker.openFile(filename);
    //    }
    //    public string GetNextToken()
    //    {
    //        return Toker.getTok();
    //    }
    //    CToker Toker=new CToker();
    //}
/*
 *
 * Get semiexpressions one by one from a file.
 */
    public class SemiExtractor
    {
        public SemiExtractor()
        {
            Semi.displayNewLines = false;
        }
        //open file stream
        public bool Open(string filename)
        {
            return Semi.open(filename);
        }            
        //get next semi expression
        public SemiExpression GetNextSemi()
        {

            if(!Semi.getSemi())
                return null;
            SemiExpression ret = new SemiExpression();
            int num = Semi.count;
            for (int i = 0; i < num; i++)
            {
                ret.Add(Semi[i]);
            }
            ret.LineCount = (uint)Semi.lineCount;
            ret.EraseStrings();
            ret.EraseComments();
            ret.EraseMacro();
            ret.EraseNewLine();
            if (ret.Count >= 2)
            {
                string lastt = ret[ret.Count - 1];
                string equa = ret[ret.Count - 2];
                if (lastt == "{" && equa == "=")
                {
                    if (!Semi.getSemi())
                        return null;
                    for (int i = 0; i < Semi.count; i++)
                    {
                        ret.Add(Semi[i]);
                    }
                    Semi.getSemi();
                    ret.Add(";");
                }
            }

            //ret.Print();
            return ret;
        }
        private CSemiExp Semi=new CSemiExp();
    }
    /*
     *
     * keep the info of a semiexpression
     * mainly a list of tokens.
     */
    public class SemiExpression
    {
        List<string> _tokens = new List<string>();
        //getter of tokens
        public List<string> Tokens
        {
            get
            {
                return _tokens;
            }
        }
        //linecount
        uint _linecount = 0;
        //getter and setter of linecount
        public uint LineCount
        {
            get
            {
                return _linecount;
            }
            set
            {
                _linecount = value;
            }
        }
        //debug
        public void Print()
        {
            foreach(string e in Tokens)
            {
                System.Console.Write(e);
            }
        }
        //if contains a certain string
        public bool Contains(string str)
        {
            return Tokens.Contains(str);
        }
        //check whether a is within the range.
        public bool IsInRange(int a)
        {
            if (a >= 0 && a < this.Count)
                return true;
            else
                return false;
        }
        //get number tokens
        public int Count
        {
            get
            {
                return Tokens.Count;
            }
        }
        //a bunch of search functons
        public int FindToken(string token)
        {
            return Tokens.FindIndex(t => t == token);
        }
        public int FindLastToken(string token)
        {
            return Tokens.FindLastIndex(t => t == token);
        }
        public int FindFirstToken(string token)
        {
            return Tokens.FindIndex(t => t == token);
        }

        //add a token to the end
        public void Add(string token)
        {
            Tokens.Add(token);
        }
        public void Add(List<string> token)
        {
            Tokens.AddRange(token);
        }
        //overload []
        public string this[int i]
        {
            get { return _tokens[i]; }
            set { _tokens[i] = value; }
        }
        //remove macro
        public void EraseMacro()
        {
            int sharp = Tokens.FindIndex(0,v=>v=="#");
            if (sharp != -1)
            {
                int line = Tokens.FindIndex(sharp, v => v == "\n");
                if (line != -1 && sharp < line)
                {
                    Tokens.RemoveRange(sharp, line-sharp+1);
                }
            }
        }
        //remove strings in the code
        public void EraseStrings()
        {
            _tokens = TokensUtil.EraseStrings(_tokens);
        }
        //remove comments in case there is a class in comments detected.
        public void EraseComments()
        {
            _tokens = TokensUtil.EraseComments(_tokens);
        }
        //remove /n
        public void EraseNewLine()
        {
            _tokens = TokensUtil.EraseNewLine(_tokens);
        }
        //List < string > are different tokens. This function merges them to one token
        public void Merge()
        {
            _tokens = TokensUtil.Merge(_tokens);
        }

    }
    //----< test stub >--------------------------------------------------
#if(TEST_SEMIEXPRESSION)
    class TestSemi
    {
        static void Main(string[] args)
        {
            List<string> tokens = new List<string>(){"void","Functionname","(",
                           "List","<","string",">","args1",",",
                            "int","arg2",",",
                            "ref","Dictionary","<","string",",","string",">","arg3",",",
                            "out","string","arg4",",",
                             "params","UserDefinedType","arg5",
                            ")",
                            "{"};

            SemiExpression semi=new SemiExpression();
            foreach (string tok in tokens)
            {
                semi.Add(tok);
            }
            semi.Merge();

            IsFunction isf = new IsFunction();
            bool isfunc=isf.Test(semi);
            System.Console.WriteLine("This semi is a function? " + isfunc.ToString());
            System.Console.WriteLine("");
            OutputManager.DisplayTypes((Scope)isf.Arg);
            System.Console.ReadLine();
        }
    }
#endif
}
