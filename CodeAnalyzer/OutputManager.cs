/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * OutputManager.DisplayTypes(scope)            //Display all the children of scope recursively
 * OutputManager.DisplayReplationships(scope)   //Display all replationships related to it and its children
 * OutputManager.WriteTypes2XML(scope,filename) //Write all children of scope to filename in format of XML
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 5  2014 created by Shikai Jin 
 * 
 * Ver 2.0 Oct 9 2014 
 * new way to display relationship
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
using System.IO;
using System.Xml.Linq;
using Util;
namespace CodeAnalyzer
{
    /*
     * Display module
     * responsible for displaying stuffs in the repository in a certain user-friendly manner
     * 
     */
    public class OutputManager
    {
        /*
         * 
         *Only display the following types in the repository   
         */
        static SType[] _concernedtypes = {/*SType.ARGUMENT,SType.MEMBER ,*/ SType.DELEGATE,SType.CLASS, SType.ENUM, SType.STRUCT, SType.INTERFACE, SType.NAMESPACE, SType.FUNCTION };
        /*
         * 
         *Display indents at the begining of each line
         */
        static void DisplayIndent(int num)
        {
            string str = "";
            for (int i = 0; i < num; i++)
            {
                str += "  ";
            }
            System.Console.Write(str);
        }
        /*
         * 
         *add paddings to the right of a string
         *
         */
        static void DisplayStringWithPaddingRight(string str, int padding)
        {
            System.Console.Write(str.PadRight(padding));
            if (str.Length >= padding)
                System.Console.Write(" ");
        }
        /*
         * 
         *Get  function argument
         */
        static string GetFunctionArgumentList(Scope s)
        {
            List<string> l = new List<string>();
            for (int i = 0; i < s.Child.Count; i++)
            {
                if(s.Child[i].Type==SType.ARGUMENT)
                    l.Add(s.Child[i].Name);
            }
            return "(" + String.Join(",", l) + ")";
        }
        /*
         * 
         *Display a function
         */
        static void DisplayFunction(Scope s)
        {
            if (s.IsGeneralFunction())
            {
                System.Console.Write("func ");
                //display its name
                //DisplayStringWithPaddingRight(s.Name, 17);
                string arglist = GetFunctionArgumentList(s);
                if (arglist.Length>0)
                {
                    DisplayStringWithPaddingRight(s.Name+" "+GetFunctionArgumentList(s), 40);
                }
                //System.Console.WriteLine("");
                //display function complexities and sizes
                //DisplayIndent(_indent);
                if (s.Property.ContainsKey(PROPERTYKEY.COMPLEXITY))
                    DisplayStringWithPaddingRight("cc: " + s.Property[PROPERTYKEY.COMPLEXITY], 7);
                DisplayStringWithPaddingRight("Size: " + (s.BeginLine.ToString() +"-"+ s.EndLine.ToString()), 10);
                //display function argument to tell apart the same function with different argumrnt table

                System.Console.WriteLine();
            }
        }
        /*
         * Display a concerned type
         *   
         */
        static void DisplayScope(Scope s)
        {

            if (s.IsGeneralFunction())
                DisplayFunction(s);
            else
                System.Console.WriteLine(s.Type.ToString().ToLower() + " " + s.Name);
        }
        /*
         * display all types recursively in the repository
         *   
         */
        static public void DisplayTypes(Scope s)
        {
            bool ShouldStayHereToWrite = _concernedtypes.Contains(s.Type);

            if (ShouldStayHereToWrite)
            {
                DisplayIndent(_indent);
                DisplayScope(s);
                _indent++;
            }
            //recursive call
            foreach (Scope child in s.Child)
            {
                DisplayTypes(child);
            }
            if (ShouldStayHereToWrite)
            {
                _indent--;
            }

        }

        //show one relationship
        static void ShowRelationship(string str, List<string> re)
        {
            if (re.Count > 0)
            {
                DisplayIndent(3); Console.Write(str.PadRight(13));
                Console.WriteLine(String.Join(",",re));
            }
        }
        //show all the relationship with one class
        static void ShowRelationForClass(Scope s,
            List<string> retinher, List<string> retaggr,
            List<string> retcom, List<string> retusin)
        {
            int toshow = retinher.Count + retaggr.Count + retcom.Count + retusin.Count;
            if (toshow > 0)
            {
                System.Console.WriteLine(s.Type.ToString().ToLower() + " " + s.Name + ":");
                ShowRelationship("Inheritance: ", retinher);
                ShowRelationship("Aggragation: ", retaggr);
                ShowRelationship("Composition: ", retcom);
                ShowRelationship("Using: ", retusin);
            }
        }


        static int _indent = 0;



/////////////////////////////////////////////////////////////////////////////////////////////////////////////
        static string NAME = "_name";
        static string PROPERTY = "_property";
        static SType GetElementType(XElement ele)
        {
            SType st;
            try
            {
                st = (SType)Enum.Parse(typeof(SType), ele.Name.ToString());
            }
            catch (Exception e)
            {
                st = SType.NOT;
                DebugLog.Instance.Write(e);
            }
            return st;
        }
        static public Scope LoadScopeFromXML(XElement ele)
        {
            Scope s=new Scope();
            List<XElement> elelist=ele.Elements().ToList();
            if (elelist.Count < 2)
                return null;
            s.Type = GetElementType(ele);
            s.Name = elelist[0].Value;
            
            XElement pro=elelist[1];
            foreach (var childele in pro.Elements())
            {
                s.Property[childele.Name.ToString()] = childele.Value.ToString();
            }
            if (elelist.Count > 2)
            {
                for (int i = 2; i < elelist.Count; i++)
                {
                    Scope child = LoadScopeFromXML(elelist[i]);
                    if(child!=null)
                        s.AddChild(child);
                }
            }
            return s;
        }
        static public void MergeElementToRepository(XElement ele)
        {
            
        }

        static public void WriteScope2XML(Scope s, XElement ele,Func<Scope, bool> a = null)
        {
            if(a!=null)
            {
                if (!a(s))
                    return;
            }

            XElement newele = new XElement(s.Type.ToString());
            newele.Add(new XElement(NAME, s.Name));
            XElement property = new XElement(PROPERTY);
            newele.Add(property);
            foreach (var pair in s.Property)
            {
                property.Add(new XElement(pair.Key,pair.Value));
            }
            ele.Add(newele);
            foreach (Scope child in s.Child)
            {
                WriteScope2XML(child,newele,a);
            }
        }
        static public void WriteRepository2XML(StreamWriter sw)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Repository");
            doc.Add(root);
            WriteScope2XML(TypeRepository.Instance.RootScope,root);
            sw.Write(doc.ToString());
        }
        static public void WriteRepository2XML(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    WriteRepository2XML(sw);
                }
            }
        }
        static public void WriteRepository2XML(out string name)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    WriteRepository2XML(sw);
                }
                name = Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        //static public void WriteRelationship2XML(Scope s)

    }
    //TEST STUB!
#if(TEST_OUTPUTMANAGER)
    class TestOutputMgr
    {
        static void Main(string[] args)
        {
            OnAddAndEnterScope addenter = new OnAddAndEnterScope();
            OnAddScope addscope = new OnAddScope();
            OnLeanvingScope leavingscope = new OnLeanvingScope();
            addenter.Do(Scope.Make(SType.NAMESPACE, "anana", 0, 0));
            addenter.Do(Scope.Make(SType.CLASS, "Aname", 0, 0));
            leavingscope.Do(null);
            addenter.Do(Scope.Make(SType.CLASS, "Bname", 0, 0));
            addscope.Do(Scope.Make(SType.FUNCTION, "Func1", 0, 0));
            addscope.Do(Scope.Make(SType.FUNCTION, "Func2", 0, 0));
            leavingscope.Do(null);
            OutputManager.DisplayTypes(TypeRepository.Instance.RootScope);
            OutputManager.WriteTypes2XML(TypeRepository.Instance.RootScope,"Test_Output_Mgr.xml");
            System.Console.ReadLine();
        }

    };
#endif
}
