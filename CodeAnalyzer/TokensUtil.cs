/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
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
/*
 *   Build command:
 *   devenv ./DependencyAnalyzer.sln /rebuild debug
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
        public static bool Identical(List<string> a,List<string> b)
        {
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
        //copy a token list to a new one
        public static List<string> Copy(List<string> tokens)
        {
            List<string> newtokens = new List<string>();
            foreach (string t in tokens)
            {
                newtokens.Add((string)t.Clone());
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
        static string RemoveBetween(string a,Predicate<Char> con)
        {
            StringBuilder sb = new StringBuilder();
            bool inq = false;
            for (int i = 0; i < a.Length; i++)
            {
                if (con(a[i]) && inq == false)
                    inq = true;
                if (inq == false)
                    sb.Append(a[i]);
                if (con(a[i]) && inq == true)
                    inq = false;
            }
            return sb.ToString();
        }
        public static List<string> EraseStrings(List<string> _tokens)
        {
            List<string> tokens = new List<string>();
            foreach(string str in _tokens)
            {
                string newstr=str.Replace("\\\"","");
                newstr=RemoveBetween(newstr,(c)=>{return c=='\"';});
                if(newstr.Length>0)
                    tokens.Add(newstr);
            }
            return _tokens;
        }

        //List < string > are different tokens. This function merges them to one token
        static bool _shouldeatdot(List<string> text, int index)
        {
            if (text[index] == ".")
                return true;
            if (index > 0)
            {
                if (text[index - 1] == ".")
                    return true;

            }
            if (index + 1 < text.Count)
            {
                if (text[index + 1] == ".")
                    return true;
            }
            return false;
        }
        static string _eatDotRef(List<string> text,ref int index)
        {
            string ret="";
            for (; index < text.Count; index++)
            {
                if (_shouldeatdot(text, index))
                    ret += text[index];
                else
                    break;
            }
            return ret;
        }

        static void _eatTemplate_statusupdate(List<string> text, int index, ref bool inarrows)
        {
            if (index > 0)
            {
                if (text[index - 1] == ">" || text[index - 1] == "]")
                {
                    inarrows = false;
                }
            }
            if (index + 1 < text.Count)
            {
                if (text[index + 1] == "<" || text[index + 1] == "[")
                {
                    inarrows = true;
                }
            }
            if (text[index] == "<")
                inarrows = true;
        }
        static string _eatTemplate(List<string> text, ref int index)
        {
            string ret = "";
            bool inarrows = false;
            for (; index < text.Count; index++)
            {
                _eatTemplate_statusupdate(text, index, ref inarrows);
                if (inarrows)
                    ret += text[index];
                else
                    break;
            }
            return ret;
        }

        static string _eat(List<string> text, ref int index)
        {
            string ret = "";
            if(index<text.Count)
                ret += _eatDotRef(text, ref index);
            if (index < text.Count)
                ret += _eatTemplate(text, ref index);
            return ret;
        }
        static public List<string> Merge(List<string> text)
        {
            List<string> ret = new List<string>();
            int index=0;
            while (index < text.Count)
            {
                string tmp = _eat(text,ref index);
                if (tmp.Length > 0)
                    ret.Add(tmp);
                if (index < text.Count)
                {
                    ret.Add(text[index]);
                    index++;
                }
            }
            return ret;
        }
        //static public List<string> Merge(List<string> text)
        //{
        //    List<string> debug = Copy(text);
            

        //    List<string> ret = new List<string>();
        //    string tmp = "";
        //    bool inarrows = false;
        //    for (int i = 0; i < text.Count; i++)
        //    {
        //        if (text[i] == ".")
        //        {
        //            tmp += text[i];
        //            continue;
        //        }
        //        if (i > 0)
        //        {
        //            if (text[i - 1] == ".")
        //            {
        //                tmp += text[i];
        //                continue;
        //            }
        //            if (text[i - 1] == ">" || text[i - 1] == "]")
        //            {
        //                inarrows = false;
        //            }
        //        }
        //        if (i + 1 < text.Count)
        //        {
        //            if (text[i + 1] == ".")
        //            {
        //                tmp += text[i];
        //                continue;
        //            }
        //            if (text[i + 1] == "<" || text[i + 1] == "[")
        //            {
        //                inarrows = true;
        //            }
        //        }

        //        if (inarrows == true)
        //        {
        //            tmp += text[i];
        //            continue;
        //        }
        //        if (tmp.Length > 0)
        //        {
        //            ret.Add(tmp);
        //            tmp = "";
        //        }
        //        ret.Add(text[i]);
        //    }

        //    //List<string> d1 = Copy(ret);
        //   // List<string> _ret = _Merge(debug);
        //    try
        //    {
        //        List<string> aaaa = new List<string>();
        //        string inou = text[0];
        //        for (int i = 0; i < text.Count; i++)
        //        {
        //            aaaa.Add((string)inou.Clone());
        //        }
        //        _Merge(aaaa);
        //    }
        //    catch (Exception e)
        //    { 
                
        //    }
        //    //if (Identical(_ret, ret))
        //    //    System.Console.WriteLine("T");
        //    //else
        //    //    System.Console.WriteLine("F");

        //    return ret;
        //}
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
