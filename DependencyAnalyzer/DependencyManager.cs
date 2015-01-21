/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 *   Build command:
 *   devenv ./DependencyAnalyzer.sln /rebuild debug
 *   
 *   Maintenance History:
 *   Ver 1.0  Nov. 14  2014 created by Shikai Jin 
 */
/*
 * 
 * Public Interface
 *
 * public void Refresh()    //refresh hash table.
 * 
 * public XElement GetPackageDependency(string project)//get all package deps inside a project
 * public XElement GetTypeDependency(string project) //get all type deps inside a project
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalyzer;
using System.IO;
using System.Xml.Linq;
namespace DependencyAnalyzer
{
    /// <summary>
    /// used to get package and type deps
    /// </summary>
    class DependencyManager
    {
        private static DependencyManager _instance = null;
        static public DependencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DependencyManager();
                }
                return _instance;
            }
        }
        /// <summary>
        /// since all files are hashed this function used to get real file path
        /// </summary>
        public void Refresh()
        {
            //Clear();
            _fill_hash2pack();
        }
        /// <summary>
        /// get package name from file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetPackName(string path)
        {
            return Path.GetFileName(path);
        }
        //public Dictionary<string, HashSet<string>> PackageDep { get { return _pack2packs; } }
        Dictionary<string, string> _hash2pack = new Dictionary<string, string>();
        Object _locker = new object();
        /// <summary>
        /// hash to pack dict
        /// </summary>
        void _fill_hash2pack()
        {
            List<Scope> packs = Scope.FindChildWithType(TypeRepository.Instance.RootScope,
                (t) => { return t == SType.PACKAGE; });
            lock (_locker)
            {
                foreach (Scope pack in packs)
                {
                    _hash2pack[pack.Name] = pack.Property[PROPERTYKEY.FILENAME];
                }
            }
        }
        /// <summary>
        /// get all used packages inside each type
        /// </summary>
        /// <param name="usedtypes"></param>
        /// <returns></returns>
        HashSet<string> GetUsedPackage(List<Scope> usedtypes)
        {
            HashSet<string> ret = new HashSet<string>();
            foreach (Scope type in usedtypes)
            {
                string fullpackname = TypeRepository.Instance.GetPackageName(type.TypeUrl);
                ret.Add(GetPackName(fullpackname));
            }
            return ret;
        }
        /// <summary>
        /// get package name from hash
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string Hash2ShortPackagename(string str)
        {
            if (_hash2pack.ContainsKey(str))
            {
                return GetPackName(_hash2pack[str]);
            }
            return "";
        }
        /// <summary>
        /// get pckage deps as xml
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public XElement GetPackageDependency(string project)
        {
            XElement ret = new XElement("root");
            List<Scope> prolist = GetProjectList(project);
            if (prolist.Count > 0)
            {
                foreach (Scope pack in prolist[0].Child)
                {
                    List<Scope> usedtypes = GetUsedType(pack);

                    XElement adep = new XElement("dependency");
                    string packagename=Hash2ShortPackagename(pack.Name);
                    adep.Add(new XElement("name", packagename));
                    var packset=GetUsedPackage(usedtypes);
                    foreach (string apack in packset)
                    {
                        if (apack.Length > 0 && apack != packagename)    
                        adep.Add(new XElement("using", apack));
                    }

                    ret.Add(adep);

                }
            }

            return ret;
        }
        /// <summary>
        /// get scops used by sc
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        List<Scope> GetUsedType(Scope sc)
        {
            return Scope.FindChild(sc,
                (s) =>
                {
                    return (s != sc) && 
                        (ScopeCategory.GeneralVariables.Contains(s.Type) || 
                        s.Type == SType.STATEMENT_INHERITANCE ||
                        s.IsGeneralClass()) &&
                        //(!_boringtypes.Contains(s.TypeName)) &&
                        s.TypeUrl.Length > 0;
                });
        }
        /// <summary>
        /// get types deps for the scope
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        XElement GetTypeDepForOneScope(Scope sc)
        {
            List<Scope> usedtypes = GetUsedType(sc);
            if (usedtypes.Count == 0)
            {
                return null;
            }
            HashSet<string> hash = new HashSet<string>();
            foreach (var scope in usedtypes)
            {
                hash.Add(scope.TypeName);
            }
            XElement adep = new XElement("dependency");
            adep.Add(new XElement("name", sc.Name));

            foreach (string deps in hash)
            {
                if (deps != sc.Name)
                    adep.Add(new XElement("using", deps));
            }
            return adep;
        }
        //List<string> _boringtypes = null;
        ///// <summary>
        ///// get unconcerned types
        ///// </summary>
        //void GetBoringTypes()
        //{
        //    if (_boringtypes == null)
        //    {
        //        _boringtypes = new List<string>();
        //        foreach (TypeCode t in Enum.GetValues(typeof(TypeCode)))
        //        {
        //            _boringtypes.Add(t.ToString());
        //            _boringtypes.Add(t.ToString().ToLower());
        //            _boringtypes.Add(t.ToString() + "[]");
        //            _boringtypes.Add(t.ToString().ToLower() + "[]");
        //        }
        //        _boringtypes.Add("int");
        //    }
        //}
        /// <summary>
        /// get projects 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<Scope> GetProjectList(string name)
        {
            return Scope.FindChild(TypeRepository.Instance.RootScope,
                (sc) => { return sc.Type == SType.PROJECT && sc.Name == name; });
        }
        /// <summary>
        /// get type deps
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public XElement GetTypeDependency(string project)
        {
            //GetBoringTypes();
            List<Scope> prolist = GetProjectList(project);
            XElement ret = new XElement("root");
            if (prolist.Count > 0)
            {
                List<Scope> types = Scope.FindChild(prolist[0], 
                    (sc) => { return sc.IsGeneralClass(); });
                foreach (Scope s in types)
                {
                    XElement ele = GetTypeDepForOneScope(s);
                    if (ele != null)
                        ret.Add(ele);             
                }
            }
            return ret;
        }

    }
}



#if(EEE)
    class Test
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(
                DependencyAnalyzer.DependencyManager.Instance.GetPackageDependency("AAA").ToString());
            System.Console.WriteLine(
                DependencyAnalyzer.DependencyManager.Instance.GetTypeDependency("BBB").ToString());
        }
    }
#endif