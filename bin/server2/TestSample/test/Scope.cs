/*
 * Manual Page:
 * -----------------------------------------------------------------------------------------------------
 * class Scope:
 * introduction:
 * scopes are organized in a tree structre held by TypeRepository
 * 
 * static functions:
 * Scope.Make(type,name,beginline,endline)                       //make a general scope 
 * Scope.MakeMember(accessibility,types,name,beginline,endline)  //member variable of a class
   Scope.MakeProperty(accessibility,types,name,beginline,endline) //property with getter setter
 * Scope.Function(accessibility,types,name,beginline,endline)     //make a scope representing function
 * Scope.FindParentClass(scope)                    //find which class this scope is inside
 * Scope.FindParentClassWithType(scope,con)       //find which specific type this scope is inside
 * Scope.FindChildWithType(Scope,con)             //find children of a spefific type
 * Scope.MakeRoot()                               //Make a root for the scope tree
 * Scope.IsKeyword(string)                        //to check if this string is a keyword in c#
 * Scope.HasWhichKeyword(stringlist)              //find which keywords the stringlist contains
 * Scope.Keyword2Enum(string)                     //Get the enum value for a keyword
 * Scope.MakeKeyword(name,beginline,endline)      //make for,switch,etc. scope
 * Scope.IsContain(scope1,scope2)                 //check if scope1 contains scope2
 * 
 * non-static function:
 * 
 * Scope sc=new Scope();
 * sc.HasSameSignature(s)    //check if sc has the same name and type with s
 * sc.AddChild(s)            //add s to the children list
 * sc.IsGeneralClass();      //return true if sc is a class,interface,enum or struct ,otherwise false
 * sc.IsGeneralRelationship();  //return true if sc is a relationship
 * sc.IsGeneralNamespace();     //return true if sc is a namespace
 * sc.IsGeneralFunction();      //return true if sc is a function
 * 
 * Public properties or member variablbs
 * ------------------------------------------------------------------------------
 * Name           Type          Meaning
 * ------------------------------------------------------------------------------
 * BeginLine  :   uint          beginline of this scope
 * EndLine    :   uint          beginline of this scope
 * Type       :   enum SType    type of this scope, defined in enum SType
 * Name       :   string        name of this scope     
 * Parent     :   Scope         parent of this scope
 * Child      :   List<Scope>   children of this scope
 * ------------------------------------------------------------------------------
 * 
 * -----------------------------------------------------------------------------------------------------
 * 
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 3  2014 
 * created by Shikai Jin 
 * 
 * Ver 1.0  Oct. 7  2014 
 * URL related code
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AAA.BBB;


namespace CodeAnalyzer
{
    //All element types concerned
    public enum SType
    {
        ROOT,
        FUNCTION,
        NAMESPACE,
        CLASS,
        STRUCT,
        ENUM,
        INTERFACE,
        DELEGATE,
        FOR,
        FOREACH,
        LOOPBREAK,
        IF,
        WHILE,
        ANONYMOUS,
        SWTICH,
        TRY,
        PROPERTY,
        CATCH,
        STATEMENT_INHERITANCE,
        STATEMENT_COMPOSTION,
        STATEMENT_USING,
        STATEMENT_AGGREGATION,
        MEMBER,
        OTHER,
        NOT
    }

    //This is not C# property. it is additional info besides type and name
    public class PROPERTYKEY
    {

        static public string ACCESSIBILITY { get { return "ACCESSIBILITY"; } }
        static public string TYPE { get { return "TYPE"; } }
        static public string ISSTATIC { get { return "ISSTATIC"; } }
        static public string ARGSTABLE { get { return "ARGSTABLE"; } }
   //     static public string ARGSEP { get { return ","; } }

        static public string COMPLEXITY { get { return "COMPLEXITY"; } }
        static public string SIZE { get { return "SIZE"; } }
        static public string TYPEURL { get { return "TYPEURL"; } }
        static public string URL { get { return "URL"; } }
    }
/*
 *It is not exactly as its name suggests
 *it not only can represent all scopes, it also can represent all replationships, all concerned statement. 
 *Every scope can have children and parent
 */
    public class Scope
    {
        public Scope()
        {
            BeginLine = 0;
            EndLine = 0;
            Type = SType.NOT;
            Name = "";
            Parent = null;
        }
        public uint BeginLine { get; set; }
        public uint EndLine { get; set; }
        public SType Type { get; set; }
        public string Name { get; set; }
        //function sizes, complexity, argument types will be stored here.
        public Dictionary<string, string> _property=new Dictionary<string,string>();
        public Dictionary<string, string> Property
        {
            get { return _property; }
        }
        //with same type and name
        public bool HasSameSignature(Scope s)
        {
            if(s.Type!=Type)
                return false;
            if (s.Name != this.Name)
                return false;
            return true;

        }
        public bool HasUrl()
        {
            return Property.ContainsKey(PROPERTYKEY.TYPEURL);
        }
        public void SetUrl(string url)
        {
            Property[PROPERTYKEY.TYPEURL] = url;
        }
        //clone to another new instance
        public Scope Clone()
        { 
            Scope s=new Scope();
            s.Type=this.Type;
            s.Name=this.Name;
            s.Parent=this.Parent;
            s.BeginLine=this.BeginLine;
            s.EndLine=this.EndLine;
            foreach (var pair in this.Property)
            {
                s.Property.Add(pair.Key,pair.Value);
            }
            return s;
        }
        //Get info of member variable
        static public string[] GetMember(Scope s)
        {
            if (s.Type != SType.MEMBER)
            {
                return null ;
            }
            string fulltext = s.Name;
            string[] tags = fulltext.Split(' ');
            if (tags.Length < 3)
                return null;
            return tags;
        }
        //make a C# propert element
        static public Scope MakeProperty(string accessibility,string type, 
            string name,uint b, uint e,bool isstatic=false)
        {
            Scope s = MakeMember(accessibility,type,name,b,e,isstatic);
            s.Type = SType.PROPERTY;
            return s;
        }
        //make a class member element
        static public Scope MakeMember(string accessibility,string type, 
            string name,uint b, uint e,bool isstatic=false)
        {
            Scope s = new Scope();
            s.Type = SType.MEMBER;
            string staticstr=isstatic?"static":"";
            s.Name = name;
            s.BeginLine = b;
            s.EndLine = e;
            s.Property[PROPERTYKEY.URL] = TypeRepository.Instance.GetUrl(name);
            s.Property[PROPERTYKEY.ACCESSIBILITY] = accessibility;
            s.Property[PROPERTYKEY.TYPE] = type;
            s.Property[PROPERTYKEY.ISSTATIC] = isstatic.ToString();
            return s;
        }
        //make a member function element
        static public Scope MakeFunction(string accessibility, string type, List<string> argstable,
            string name, uint b, uint e, bool isstatic = false)
        {
            Scope s = new Scope();
            s.Type = SType.FUNCTION;
            s.Property[PROPERTYKEY.ACCESSIBILITY] = accessibility;
            s.Property[PROPERTYKEY.TYPE] = type;
            s.Property[PROPERTYKEY.ISSTATIC] = isstatic.ToString();
            s.Property[PROPERTYKEY.COMPLEXITY]="0";
            s.Property[PROPERTYKEY.ARGSTABLE] = String.Join(",", argstable);
            s.Property[PROPERTYKEY.SIZE] = "0";
            s.Name = name;
            s.BeginLine = b;
            s.EndLine = e;

            return s;
        }
        //get the class containing sc
        static public Scope FindParentClass(Scope sc)
        {
            Scope tmp = sc;
            while (true)
            {
                if (tmp == null)
                    return null;
                if (tmp.Type == SType.CLASS || tmp.Type == SType.INTERFACE || tmp.Type == SType.STRUCT)
                    return tmp;
                tmp = tmp.Parent;

            }
        }
        //get all parents, grand parents, etc. with specific types that satisfy a certain condition
        static public Scope FindParentWithType(Scope sc,Predicate<SType> con)
        {
            Scope tmp = sc;
            while (true)
            {
                if (tmp == null)
                    return null;
                if (con(tmp.Type))
                    return tmp;
                tmp = tmp.Parent;
            }
        }
        //get all children satisfying the specific condition
        static public List<Scope> FindChildWithType(Scope sc,Predicate<SType> con, bool isrecursive = true)
        {
            List<Scope> ret = new List<Scope>();
            if (con(sc.Type))
                ret.Add(sc);

            if (sc.Child.Count == 0)
            {
                return ret;
            }
            else
            {
                foreach (Scope child in sc.Child)
                {
                    List<Scope> newadded = FindChildWithType(child, con, isrecursive);
                    if (newadded.Count != 0)
                        ret.AddRange(newadded);
                }
            }

            return ret;
        }
        //make general element
        static public Scope Make(SType t,string name,uint b,uint e)
        {
            Scope s = new Scope();
            s.Type = t;
            s.Name = name;
            s.BeginLine = b;
            s.EndLine = e;
            return s;
        }
        //make a root element for the tree
        static public Scope MakeRoot()
        {
            Scope s = new Scope();
            s.Type = SType.ROOT;
            s.Name = "";
            s.BeginLine = 0;
            s.EndLine = 0;
            return s;
        }
        //some of keywords that begin a scope
        static string[] Keywords = { "for", "switch", "try", "catch", "if","foreach" };
        static Dictionary<SType, int> Type2Count = new Dictionary<SType, int>();
        //judge if it's a key word.
        public static bool IsKeyword(string str)
        {
            return Keywords.Contains(str);
        }
        public static string HasWhichKeyword(IEnumerable<string> l)
        {
            foreach (string str in l)
            {
                if (IsKeyword(str))
                {
                    return str;
                }
            }
            return "";
        }
        //Convert a string keyword to a enum member accordingly.
        public static SType Keyword2Enum(string type)
        {
            switch (type)
            { 
                case "for":
                    return SType.FOR;
                case "if":
                    return SType.IF;
                case "try":
                    return SType.TRY;
                case "switch":
                    return SType.SWTICH;
                case"catch":
                    return SType.CATCH;
                case "foreach":
                    return SType.FOREACH;
                case "break":
                    return SType.LOOPBREAK;
                default:
                    return SType.OTHER;
            }
        }
        // make scope containing a  cerntain key word, such as for scope, switch scope, etc.
        static public Scope MakeKerword(string type, uint b, uint e)
        {
            Scope s = new Scope();
            s.Type = Keyword2Enum(type);
            if (!Type2Count.ContainsKey(s.Type))
            {
                Type2Count[s.Type] = 0;
            }
            s.Name = Type2Count[s.Type].ToString();
            s.BeginLine = b;
            s.EndLine = e;
            //Type2Count.
            Type2Count[s.Type]++;
            return s;
        }
        //add a child scope
        public void AddChild(Scope c)
        {
            Child.Add(c);
            c.Parent = this;
        }
        //determine if tmp contains child
        static public bool IsContain(Scope tmp, Scope child)
        {
            if (tmp == child)
                return true;
            else
            { 
                foreach(Scope c in tmp.Child)
                {
                    if (true == IsContain(c, child))
                        return true;
                }
                return false;
            }
            
        }
        //setter and getter of parent
        public Scope Parent { get; set; }
        //getter of children
        public List<Scope> Child
        {
            get
            {
                return _Child;
            }
        }
        //public static Func<SType, bool> IsGeneralRelationship =
        //    c => new List<SType>() { SType.STATEMENT_COMPOSTION, SType.STATEMENT_INHERITANCE, 
        //                                SType.STATEMENT_USING,SType.STATEMENT_AGGREGATION }.Contains(c);
        //public static Func<SType, bool> IsGeneralClass =
        //    c => new List<SType>() { SType.STRUCT, SType.CLASS, SType.INTERFACE, SType.ENUM }.Contains(c);
        //public static Func<SType, bool> IsGeneralFunction =
        //    c => new List<SType>() { SType.FUNCTION }.Contains(c);
        //public static Func<SType, bool> IsGeneralNamespace =
        //    c => c == SType.NAMESPACE;
        
        //is a relationship?
        public bool IsGeneralRelationship()
        {
            List<SType> gc = new List<SType>() { SType.STATEMENT_COMPOSTION, SType.STATEMENT_INHERITANCE, 
                                        SType.STATEMENT_USING,SType.STATEMENT_AGGREGATION };
            return gc.Contains(this.Type);
        }
        //is a class?
        public bool IsGeneralClass()
        {
            List<SType> gc = new List<SType>() { SType.STRUCT, SType.CLASS, SType.INTERFACE, SType.ENUM };
            return gc.Contains(this.Type);
        }
        //is a function?
        public bool IsGeneralFunction()
        {
            List<SType> gc = new List<SType>() { SType.FUNCTION };
            return gc.Contains(this.Type);
        }
        //is a namespace?
        public bool IsGeneralNamespace()
        {
            return this.Type == SType.NAMESPACE;
        }
        private List<Scope> _Child=new List<Scope>();
    }
    //----< test stub >--------------------------------------------------
#if(TEST_SCOPES)
    class TestScope
    {
        static void Main(string[] args)
        {
            Scope s = Scope.Make(SType.NAMESPACE, "SPACE1", 0, 0);
            {
                Scope ss = Scope.Make(SType.CLASS, "CLASS1", 0, 0);
                s.AddChild(ss);

                Scope sss1 = Scope.Make(SType.FUNCTION, "FUNCTION1", 0, 0);
                ss.AddChild(sss1);

                Scope sss2 = Scope.Make(SType.FUNCTION, "FUNCTION2", 0, 0);
                ss.AddChild(sss2);


                Scope sss = Scope.Make(SType.CLASS, "CLASS2", 0, 0);
                s.AddChild(sss);
            }
            OutputManager.DisplayTypes(s);
            System.Console.ReadLine();
        }
    }
#endif
}
