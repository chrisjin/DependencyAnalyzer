/*
 * Manual Page:
 * 
 * List<string> newtokens = TokensUtil.Cope(tokens);       //copy each token to a new list
 * List<string> newtokens = TokensUtil.EraseMacro(tokens); //erase macro that is, tokens begin with a #
 * List<string> newtokens = TokensUtil.EraseNewLine(tokens);       //erase \n
 * List<string> newtokens = TokensUtil.EraseStrings(tokens);       //erase "yada yada"
 * List<string> newtokens = seTokensUtilMerge(tokens);              //combine tokens like "List", "<", "int", ">" to "List<int>"
 * bool flag = TokensUtil.isComment(tokens)                //check if it is a comment                    
 * 
 * Maintenance History:
 * 
 * Ver 1.0  Oct. 6  2014 
 * created by Shikai Jin
 * 
 * Ver 2.0  Oct. 7  2014
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class TokensUtil
    {
        //copy a token list to a new one
        public static List<string> Copy(List<string> tokens)
        {
            List<string> newtokens = new List<string>();
            foreach (string t in tokens)
            {
                newtokens.Add(t);
            }
            return newtokens;
        }
        public static bool isComment(string tok)
        {
            if (tok.Length > 1)
                if (tok[0] == '/')
                    if (tok[1] == '/' || tok[1] == '*')
                        return true;
            return false;
        }
        //remove comments
        public static List<string> EraseComments(List<string> tokens)
        {
            List<string> ret = EraseStrings(tokens);
            List<string> todelete = new List<string>();
            foreach (string token in ret)
            {
                if (isComment(token))
                {
                    todelete.Add(token);
                }
            }
            foreach (string str in todelete)
            {
                ret.Remove(str);
            }
            return ret;
        }
        // remove \n
        public static List<string> EraseNewLine(List<string> tokens)
        {
            List<string> ret = new List<string>();
            foreach (string token in tokens)
            {
                string tmp = token.Replace("\n", "");
                if (tmp.Length > 0)
                {
                    ret.Add(tmp);
                }
            }
            return ret;
        }
        //remove quote
        public static List<string> EraseStrings(List<string> _tokens)
        {
            List<string> tokens = Copy(_tokens);
            for (int j = 0; j < tokens.Count; j++)
            {
                string token = tokens[j];
                List<int> start = new List<int>();
                List<int> end = new List<int>();
                bool beginflag = false;
                for (int i = 0; i < token.Length; i++)
                {
                    if (i > 0)
                    {
                        if (token[i - 1] == '\\')
                            continue;
                    }
                    if (beginflag == false)
                    {
                        char tmp = token[i];
                        if (tmp == '\"')
                        {
                            start.Add(i);
                            beginflag = true;
                        }
                    }
                    else
                    {
                        char tmp = token[i];
                        if (tmp == '\"')
                        {
                            end.Add(i);
                            beginflag = false;
                        }
                    }
                }
                if (start.Count == end.Count)
                {
                    for (int i = 0; i < start.Count; i++)
                    {
                        if (end[i] > start[i])
                            _tokens[j] = token.Remove(start[i], end[i] - start[i] + 1);
                    }
                }
            }
            return tokens;
        }
        //List < string > are different tokens. This function merges them to one token
        static public List<string> Merge(List<string> text)
        {
            List<string> ret = new List<string>();
            string tmp = "";
            bool inarrows = false;
            for (int i = 0; i < text.Count; i++)
            {
                if (text[i] == ".")
                {
                    tmp += text[i];
                    continue;
                }
                if (i > 0)
                {
                    if (text[i - 1] == ".")
                    {
                        tmp += text[i];
                        continue;
                    }
                    if (text[i - 1] == ">" || text[i - 1] == "]")
                    {
                        inarrows = false;
                    }
                }
                if (i + 1 < text.Count)
                {
                    if (text[i + 1] == ".")
                    {
                        tmp += text[i];
                        continue;
                    }
                    if (text[i + 1] == "<" || text[i + 1] == "[")
                    {
                        inarrows = true;
                    }
                }


                if (inarrows == true)
                {
                    tmp += text[i];
                    continue;
                }
                if (tmp.Length > 0)
                {
                    ret.Add(tmp);
                    tmp = "";
                }
                ret.Add(text[i]);
            }
            return ret;
        }
        // erase string tokens between to string patterns
        public static List<string> EraseBetween(List<string> list, string str1, string str2)
        {
            return EraseBetween(list, c => c == str1, c => c == str2);
        }
        // erase string tokens between to string patterns
        public static List<string> EraseBetween(List<string> list,
            Predicate<string> s1, Predicate<string> s2)
        {
            List<string> ret = new List<string>();
            bool able_to_record = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (s1(list[i]))
                {
                    able_to_record = false;
                }
                if (able_to_record)
                {
                    ret.Add(list[i]);
                }
                if (s2(list[i]))
                {
                    able_to_record = true;
                }
            }
            return ret;
        }
    }
    //----< test stub >--------------------------------------------------
#if(TEST_TOKENSUTIL)
    class TestTokenutil
    {
        static void ShowTokens(List<string> ts)
        {
            foreach (string token in ts)
            {
                System.Console.Write("{0} ", token);
            }
            System.Console.WriteLine("");
        }
        static void TestEraseNewLine()
        {
            List<string> tokens = new List<string>() { "a", "\n", "b", "\n", "c" };
            System.Console.WriteLine("Testing EraseNewLine");

            System.Console.WriteLine("Before:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
            tokens = TokensUtil.EraseNewLine(tokens);


            System.Console.WriteLine("After:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
        }
        //static void EraseStrings
        static void TestMerge()
        {
            List<string> tokens = new List<string>(){"void","Functionname","(",
                           "List","<","string",">","args1",",",
                            "int","arg2",",",
                            "ref","Dictionary","<","string",",","string",">","arg3",")","{"};

            System.Console.WriteLine("Testing Merge");
            System.Console.WriteLine("Before:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
            tokens = TokensUtil.Merge(tokens);


            System.Console.WriteLine("After:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
        }
        static void TestInBetween()
        {
            List<string> tokens = new List<string>() {"asd","dff","sdd","[","rev1","rev2","]","sdad","ccc" };
            System.Console.WriteLine("Testing Between");
            System.Console.WriteLine("Before:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
            tokens = TokensUtil.EraseBetween(tokens,c=>c=="[",c=>c=="]");


            System.Console.WriteLine("After:");
            ShowTokens(tokens);
            System.Console.WriteLine("");
        }
        static void Main(string[] args)
        {
            TestMerge();
            TestEraseNewLine();
            TestInBetween();
            System.Console.ReadLine();
            
        }
    }
#endif
}
