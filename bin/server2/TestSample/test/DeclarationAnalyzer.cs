/*
 * Manual Page:
 * 
 * FunctionArgExtractor.GetTextInsideParen(tokens);  //if tokens is a function, it will return its argument list.
 * DeclarationAnalyzer da=new DeclarationAnalyzer();
 * da.Initilize(tokens);                             //Set tokens to analyze
 * da.Run();                                         //run analysis
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 6  2014 
 * created by Shikai Jin
 * add support for ref, out, in, params ...
 * 
 * ver 1.1 Oct. 7 2014
 * add support for keyword this   in  like : void myfunction(this int a)
 * Fix some out of range bugs
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
/*
 * Extract all input argument of a function given in a semiexp;
 * 
 */
    public class FunctionArgExtractor
    {
        /*
         * Get strings inside a pair of ( ).
         *  
         */
        public static List<string> GetTextInsideParen(List<string> strlist)
        {
            List<string> ret = new List<string>();
            int firstp = strlist.FindIndex(0, v => v == "(");//find the index of (
            if (firstp == -1)
                return ret;
            int secondp = strlist.FindIndex(firstp, v => v == ")");//find the index of )
            if(secondp==-1||secondp<=firstp+1)
                return ret;
            //iterate the strings between the ( and ).
            for (int i = firstp + 1; i < secondp; i++)
            {
                ret.Add(strlist[i]);        
            }
            return ret;
        }
        /*
         * 
         * Get the types of input arguments.
         * return as a list of string
         */        
        public static List<string> GetArgTypeTable(List<string> strlist)
        {
            List<string> ret = new List<string>();

            List<string> textinside = GetTextInsideParen(strlist);
            if (textinside.Count == 0)
                return ret;
            //used to record a single argument
            List<string> oneargdeclared = new List<string>() ;
            //used to rectify some defects within semiexpression
            textinside=TokensUtil.Merge(textinside);
            //iterate the strings between ( and )
            for (int i = 0; i < textinside.Count; i++)
            {
                //detect the dividing , between each argument
                if (textinside[i] != "," )
                {
                    oneargdeclared.Add(textinside[i]);
                    if(i != textinside.Count-1)
                    continue;
                }
                //Parse a single argument
                DeclarationAnalyzer da = new DeclarationAnalyzer();
                da.Initilize(oneargdeclared);
                if (false == da.Run())
                    return new List<string>();
                ret.Add(da.Type);
                oneargdeclared = new List<string>();
            }
            return ret;
        }
    }
    /*
     * Parse a declaration
     * example:
     * public void Func(asdasdasd)     a single name func and type of void will be detected
     * public int mmm,nnnn=3,ggg=2;    three names and one type int will be detected 
     *  
     */    
    public class DeclarationAnalyzer
    {
        //public
        int _currentcursor = 0;
        string _accessbility = "";
        string _type = "";
        List<string> _names = new List<string>();
        bool _isstatic = false;
        bool _isvirtual = false;
        List<string> _semi = null;


        public String Accessbility { get { return _accessbility; } }
        public String Type { get { return _type; } }
        public List<string> Names { get { return _names; } }
        public bool IsStatic { get { return _isstatic; } }
        public bool IsVirtual { get { return _isvirtual; } }
        //public List<string> ArgTable = null;
        /*
         * Preprocess the semiexpression
         *  
         */
        public void Initilize(List<string> semi)
        {
            semi = TokensUtil.EraseNewLine(semi);
            semi = TokensUtil.Merge(semi);
            semi = TokensUtil.EraseBetween(semi, "{", "}");
            _currentcursor = 0;
            _accessbility = "";
            _type = "";
            _names = new List<string>();
            _isstatic = false;
            _isvirtual = false;
            _semi = semi;

        }
        /*
         * Match the type
         *  
         */
        bool eatType()
        {
            if (_currentcursor >= _semi.Count)
                return false;
            _type = _semi[_currentcursor];
            _currentcursor++;
            return true;
        }
        /*
         * 
         * match the first token right after the type 
         */
        bool eatFirstName()
        {
            if (_currentcursor >= _semi.Count)
                return false;
            if (_semi[_currentcursor] != "(")
            {
                _names.Add(_semi[_currentcursor]);
                _currentcursor++;
            }
            else
            {
                if (_currentcursor - 1 >= 0)
                    _names.Add(_semi[_currentcursor - 1]);
            }
            return true;
        }
        /*
         * the token right after a comma must be a variable name
         *  
         */
        bool eatNames()
        {
            if(false==eatFirstName())
                return false;
            while (true)
            {
                if (_currentcursor >= _semi.Count)
                    break;
                //if()
                if (_currentcursor - 1 >= 0)
                {
                    if (_semi[_currentcursor - 1] == ",")
                        _names.Add(_semi[_currentcursor]);
                }
                _currentcursor++;
            }
            return true;
        }
        /*
         * eat tokens like [STAThread] etc.
         *  
         */
        bool eatPrefixBeforePrefix()
        {
            if (_currentcursor >= _semi.Count)
                return false;
            if (_semi[_currentcursor] == "[")
            {
                while (true)
                {
                    if (_currentcursor >= _semi.Count)
                        break;
                    if (_semi[_currentcursor] == "]")
                        break;
                    _currentcursor++;
                }
                _currentcursor++;
            }
            return true;
        }
        /*
         * eat all key words that can be put before a variable type or function
         *  
         */
        bool eatPrefix()
        {
            if (_semi.Count < 2)
                return true;
            for (int i = 0; i < _semi.Count; i++)
            {
                switch (_semi[i])
                {
                    case "public":
                    case "private":
                    case "protected":
                        _accessbility = _semi[i];
                        _currentcursor++;
                        break;
                    case "static":
                        _isstatic = true;
                        _currentcursor++;
                        break;
                    case "virtual":
                    case "override":
                        _isvirtual = true;
                        _currentcursor++;
                        break;
                    case "extern":
                    case "explicit":
                    case "implicit":
                    case "operator":
                    case "const":
                    case  "out":
                    case "ref":
                    case "param":
                    case "in":
                    case "params":
                    case "readonly":
                    case "sealed":
                    case "volatile":
                    case"unsafe":
                    case "this":
                    case "dynamic":
                        _currentcursor++;
                        break;
                }
            }
            if (_accessbility == "")
            {
                _accessbility = "protected";
            }
            return true;
        }
        /*
         * obsolete function that get argument types of a function
         *  
         */
        List<string> ExtractArgueType(List<string> atext)
        {
            List<string> ret = new List<string>();

            string[] shouldberemoved = { "out", "ref", "params", "const" };
            List<string> text = new List<string>();
            foreach (string token in atext)
            {
                if (!shouldberemoved.Contains(token))
                {
                    text.Add(token);
                }
            }

            int firstp = atext.FindIndex(0, v => v == "(");
            if (firstp == -1)
                return ret;
            int secondp = atext.FindIndex(firstp, v => v == ")");

            if (firstp == -1 || secondp == -1 || firstp >= secondp)
                return ret;
            else
            {

                int reindex = 0;
                //int settoadd = 1;
                for (int i = firstp + 1; i < secondp; i++)
                {

                    if (reindex % 3 == 0)
                    {
                        string an_arg = "";
                        //System.Console.WriteLine(text[i]);
                        an_arg += text[i];
                        ret.Add(an_arg);
                        //System.Console.WriteLine(an_arg);
                    }
                    reindex++;
                }
            }
            return ret;
        }
        /*
         * run the matching
         *  
         */
        public bool Run()
        {
            if (false == eatPrefixBeforePrefix())
                return false;
            if (false == eatPrefix())
                return false;
            if (false == eatType())
                return false;

            if (false == eatNames())
                return false;
            return true;
            //ArgTable = ExtractArgueType(_semi);
        }
    }
#if(TEST_DECLARATIONANALYZER)
    class TestDecla
    {
        static void Main(string[] args)
        {
            List<string> tokens =new List<string>(){"void","Functionname","(",
                           "List","<","string",">","args1",",",
                            "int","arg2",",",
                            "ref","Dictionary","<","string",",","string",">","arg3",",",
                            "out","string","arg4",",",
                             "params","UserDefinedType","arg5",
                            ")",
                            "{"};

            //System.Console.WriteLine("Before Merge:");
            foreach (string token in tokens)
            {
                System.Console.Write("{0} ", token);
            }
            System.Console.WriteLine("");
            System.Console.WriteLine("");


            List<string> aa=FunctionArgExtractor.GetArgTypeTable(tokens);
            System.Console.WriteLine("Argumet Types:");
            for (int i = 0; i < aa.Count;i++ )
            {
                System.Console.WriteLine("#{0}: {1} ", i,aa[i]);
            }
            System.Console.WriteLine("");
            System.Console.ReadLine();
        }
    }
#endif
}
