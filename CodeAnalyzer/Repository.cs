/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * TypeRepository tr=TypeRepository.Instance; //Get the instance
 * TypeRepository.Instance.Reset();           //Set to the orginal status
 * TypeRepository.Instance.AddScope(scope);   //add a scope as a child to  tr.CurrentScope  
 * TypeRepository.Instance.EnterScope(scope); //change CurrentScope to scope
 * TypeRepository.Instance.LeaveScope();      //change CurrentScope to its parent
 * TypeRepository.Instance.PruneInvalidComposing(); // remove all non-struct compositions
 * TypeRepository.Instance.PruneBoring();           // remove all relationships between built-in types
 * TypeRepository.Instance.PruneIrrelevant();       // remove all relationships between unfound types
 * TypeRepository.Instance.RootScope;               // Getter of root scope
 * TypeRepository.Instance.CurrentScope;            // Getter of current scope
 * MergeNamespace MergeProject    //merge new project and namespace
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 4  2014 created by Shikai Jin 
 * 
 * Ver 2.0 Oct 7   
 * URL related
 * GetUrlInCurrentScope(string)
 * GetPureName(string)
 * 
 * Ver 2.1 Oct.9 
 * Add the feature that types like A.B can be tracked 
 * by checking the namespace info in FileHeaderInfo class in Parser.cs
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
/*
 *this repository is for all concerned stuffs including both types and replationships. 
 * it's organized in a tree
 */

    public class TypeRepository
    {
        //List<Scope> Namespaces = new List<Scope>();
        Object _locker=new Object();
        private static TypeRepository _instance = null;
        static public TypeRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TypeRepository();
                    _instance.Reset();
                }
                return _instance;
            }
        }

        //Set to the orginal status
        public void Reset()
        {
            _instance._rootscope = Scope.MakeRoot();
            _instance.CurrentScope = _instance._rootscope;
        }
        //add a scope as a child to  tr.CurrentScope  
        public Scope AddScope(Scope s)
        {
            Scope foundwithsamesig = null;
            foreach (Scope child in CurrentScope.Child)
            {
                if (child.HasSameSignature(s))
                {
                    foundwithsamesig = child;
                }
            }
            if (foundwithsamesig == null)
            {
                CurrentScope.AddChild(s);
                return s;
            }
            else
                return foundwithsamesig;
        }
        public void MergeNamespace(Scope s)
        {
            if (s.Type != SType.NAMESPACE)
                return;
            lock (_locker)
            {
                Scope stoadd = AddScope(s);
                if (stoadd != s)
                {
                    Scope cur = this.CurrentScope;
                    EnterScope(stoadd);
                    foreach (Scope add in s.Child)
                    {
                        AddScope(add);
                    }
                    this.CurrentScope = cur;
                }
            }
        }
        public void MergeProject(Scope s)
        {
            if (s.Type != SType.PROJECT)
                return;
            lock (_locker)
            {
                Scope cur = this.CurrentScope;
                EnterScope(this.RootScope);
                Scope stoadd = AddScope(s);
                if (stoadd != s)
                {
                    
                    EnterScope(stoadd);
                    foreach (Scope add in s.Child)
                    {
                        AddScope(add);
                    }
                    
                }
                this.CurrentScope = cur;
            }    
        }
        //change CurrentScope to s
        public void EnterScope(Scope s)
        {
            CurrentScope = s;
        }
        public void GoToRoot()
        {
            CurrentScope = RootScope;
        }
        
        //change CurrentScope to its parent
        public void LeaveScope()
        {
            CurrentScope = CurrentScope.Parent;
        }
        private Scope _rootscope;
        public Scope RootScope
        {
            get { lock (_locker) { return _rootscope; } }
        }
        Scope _currentscope;
        public Scope CurrentScope 
        { 
            get{ lock(_locker){ return _currentscope; }}
            set { lock (_locker) { _currentscope = value; } } 
        }

        ////List<string> _structs = Scope.FindChildWithType(, c=>c==SType.STRUCT);
       //List<string> _structnames = null;
        //List<string> _types=null;
        //List<string> _boringtypes=null;
       List<string> GetAllStruct()
        {
            List<Scope> scopes = Scope.FindChildWithType(this.RootScope, c => (c == SType.STRUCT || c == SType.ENUM));
            List<string> _structnames = new List<string>();
            foreach (Scope s in scopes)
            {
                _structnames.Add(s.Name);
            }
            return _structnames;
        }
       public string GetPackageName(string url)
       {
           var tokens = url.Split('.');

           if (tokens.Length >= 2)
           {
               List<Scope> packages = Scope.FindChild(RootScope,
                   (s) =>
                   {
                       return (s.Type == SType.PACKAGE && s.Name == tokens[1]);
                   });
               if (packages.Count >= 1)
                   return packages[0].Property[PROPERTYKEY.FILENAME];
           }
           return "";
       }

        //get name without the array sign [] and replace every <yada,ydada> by the same form <T,T>
        string GetPureName(string str)
        {
            StringBuilder ret=new StringBuilder();
            bool InTemp = false, InBracket = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '[') InBracket = true;
                if (InTemp == false && InBracket == false)
                    ret.Append(str[i]);
                if (str[i] == ']') InBracket = false;
                
                if (str[i] == '<')
                {
                    InTemp = true;
                }
                if (InTemp == true)
                { 
                    if(str[i]==',') ret.Append("T,");
                    if (str[i] == '>')
                    {
                        ret.Append("T>");
                        InTemp = false;
                    }
                }

            }
            return ret.ToString();
        }
        //AAAA.BBBB.CCCC   it will divide this string into AAAA and .BBBB.CCCC
        void GetNameToFind(string fullarg,out string tofind,out string nametoconcat)
        {
            StringBuilder strtofind = new StringBuilder();
            StringBuilder strconcat = new StringBuilder();
            int cursor = 0;
            for (; cursor < fullarg.Length; cursor++)
            {
                if (fullarg[cursor] == '.')
                    break;
                strtofind.Append(fullarg[cursor]);
            }
            for (; cursor < fullarg.Length; cursor++)
            {
                strconcat.Append(fullarg[cursor]);
            }
            tofind = strtofind.ToString();
            nametoconcat = strconcat.ToString();
        }
        //Get the full name of str in the current scope
        public string GetUrl(string str)
        {
            List<string> urllist = new List<string>();
            urllist.Add(str);
            Scope tmp = CurrentScope;
            while (true)
            {
                if (tmp == null)
                    break;
                if(tmp.Name.Length>0)
                    urllist.Add(tmp.Name);
                tmp = tmp.Parent;
            }
            urllist.Reverse();
            string ret = String.Join(".", urllist);
            return ret;
        }
        //Complete Urls for scope s
        void CompleteInfo(Scope s)
        {
            if (ScopeCategory.GeneralVariables.Contains(s.Type))
            {
                TypeRepository.Instance.EnterScope(s.Parent);
                string urlt = FindFullUrl(s.Property[PROPERTYKEY.TYPE]);
                if (urlt.Length > 0)
                    s.Property[PROPERTYKEY.TYPEURL] = urlt;
                s.Property[PROPERTYKEY.URL] = GetUrl(s.Name);
            }
            else 
            { 
            }
        }
        //recursively run completeInfo
        void CompleteUrlForEachScope(Scope s)
        {
            CompleteInfo(s);
            foreach (Scope child in s.Child)
            {
                CompleteUrlForEachScope(child);
            }
        }
        //judge if a type is struct
        void CompleteStructInfo(Scope s)
        {
            List<string> _structnames = GetAllStruct();
                GetAllStruct();
                _CompleteStructInfo(s, _structnames);
        }
        public void Complete()
        {
            lock (_locker)
            {
                Scope tmp=TypeRepository.Instance.CurrentScope;
                CompleteUrlForEachScope(RootScope);
                CompleteStructInfo(RootScope);
                EnterScope(tmp);
            }
        }
        //recursively call ComleteStructInfo
        void _CompleteStructInfo(Scope s, List<string> _structnames)
        {
            if (ScopeCategory.GeneralVariables.Contains(s.Type))
            {
                if (_structnames.Contains(s.Property[PROPERTYKEY.TYPE]))
                    s.Property[PROPERTYKEY.ISSTRUCT] = true.ToString();
                else
                    s.Property[PROPERTYKEY.ISSTRUCT] = false.ToString();
            }   
            foreach(Scope child in s.Child)
            {
                _CompleteStructInfo(child, _structnames);
            }
        }
 

        //public void On
        public string GetUsingNamespace(Scope s)
        {
            while (true)
            {
                if (s == null)
                    break;
                if (s.IsGeneralClass())
                {
                    return s.Property[PROPERTYKEY.USINGNAMESPACE];
                }
                s = s.Parent;
            }
            return "";
        }
        /// <summary>
        /// retrace a scope
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        //Scope LookBack(Scope sc)
        //{ 
        
        //}
        //track a symbol by searching the scope hierarchy info.
        public string FindFullUrl(string str)
        {
            str = GetPureName(str);
            string strtofind,strtoconcat;
            GetNameToFind(str, out strtofind, out strtoconcat);
            Scope tmp = CurrentScope;
            Scope found = null;
            while (tmp!=null&&found==null)
            {
                if (GetPureName(tmp.Name) == strtofind)
                    found = tmp;
                foreach (Scope child in tmp.Child)
                {
                    if (!child.IsGeneralRelationship()
                        &&!ScopeCategory.GeneralVariables.Contains(child.Type))
                    {
                        if (GetPureName(child.Name) == strtofind)
                        {
                            found = child;
                        }
                    }
                }
                tmp = tmp.Parent;
            }
            tmp = found;
            if (tmp == null)
            {
                string usingnamespace = GetUsingNamespace(CurrentScope);
                string[] splitnamespaces = usingnamespace.Split(',');
                List<Scope> foundchild = new List<Scope>();
                List<Scope> afound = Scope.FindChild(RootScope,
                s => s.IsGeneralClass() && s.Name == strtofind);
                foundchild.AddRange(afound);
                if (foundchild.Count > 0)
                {
                    tmp = foundchild[0];
                }
            }
            if (tmp != null)
                return tmp.Url + strtoconcat;
            else
            return "";
        }
    }
    //not used yet
    delegate void CompleteFunc(Scope s);

    //Test Stub
#if(TEST_REPOSITORY)
    class TestRe
    {
        static void Main(string[] args)
        {
            TypeRepository tr = TypeRepository.Instance;
            Scope s = Scope.Make(SType.NAMESPACE, "SPACE1", 0, 0);
            tr.EnterScope(tr.AddScope(s));
            {
                Scope ss = Scope.Make(SType.CLASS, "CLASS1", 0, 0);
                tr.EnterScope(tr.AddScope(ss));

                Scope sss1 = Scope.Make(SType.FUNCTION, "FUNCTION1", 0, 0);
                tr.AddScope(sss1);

                Scope sss2 = Scope.Make(SType.FUNCTION, "FUNCTION2", 0, 0);
                tr.AddScope(sss2);

                tr.LeaveScope();

                Scope sss = Scope.Make(SType.CLASS, "CLASS2", 0, 0);
                tr.EnterScope(tr.AddScope(sss));
                tr.LeaveScope();
            }
            tr.LeaveScope();
            OutputManager.DisplayTypes(tr.CurrentScope);

            System.Console.WriteLine("");
            System.Console.WriteLine("");

            string mm="Dictionary<string,string>[]";
            System.Console.WriteLine(mm);
            System.Console.WriteLine(tr.GetPureName(mm));
            System.Console.ReadLine();
        }        
    }
#endif
}
