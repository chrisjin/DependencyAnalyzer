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
 * public XElement MakeXML()//convert the project to xml text
 * 
 * public override void OnFileEnter(string name) //callback function that will run before parsing one file
 * public override void OnFileLeave(string name)//callback function that will run after parsing one file
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalyzer;
using Util;
using System.Xml.Linq;
using System.Security.Cryptography;
namespace DependencyAnalyzer
{
    class ProjectRepository
    {
        private static ProjectRepository _instance = null;
        static public ProjectRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProjectRepository();
                }
                return _instance;
            }
        }
        /// <summary>
        /// convert to xml
        /// </summary>
        /// <returns></returns>
        public XElement MakeXML()
        {
            XElement proele = new XElement("root");
            List<Scope> prolist = Scope.FindChild(TypeRepository.Instance.RootScope,
                (s) => { return s.Type == SType.PROJECT; });
            foreach (var pro in prolist)
            {
                XElement apro = new XElement("project");
                apro.Add(new XElement("name", pro.Name));
                proele.Add(apro);
            }
            return proele;
        }
        /// <summary>
        /// send project message
        /// </summary>
        /// <param name="s"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        Message ProjectMessage(Scope s, string url)
        {
            Message ret = Message.Make(url, MessageType.POST_REPOSITORY,
                Scope.Scope2XML(s,
                            //(ss) => { return !ScopeCategory.TrivialTypes.Contains(s.Type); }
                (ss) => { return true; }
                ).ToString());
            return ret;
        }
        /// <summary>
        /// send whole project
        /// </summary>
        /// <param name="url"></param>
        public void SendProject(string url)
        {
            foreach (Scope s in TypeRepository.Instance.RootScope.Child)
            {
                if (s.Type == SType.PROJECT)
                {
                    Message ret = ProjectMessage(s, url);
                    ret.Send();
                }
            }
        }
    }
    class ProjectParser:Parser
    {
        
        //Dictionary<string, string> _file2project=new Dictionary<string,string>();
        //public Dictionary<string,string> File2Project 
        //{
        //    get { return _file2project; }
        //}
        Stack<string> _projectnamestack=new Stack<string>();
        Stack<string> _projectdefinedir = new Stack<string>();
        /// <summary>
        /// constructor
        /// </summary>
        public ProjectParser()
        {
            //Navigate.Instance.OnFileFound += OnFindFile;
            Navigate.Instance.OnDirectoryEnter += OnDirectoryEnter;
            Navigate.Instance.OnDirectoryLeave += OnDirectoryLeave;
            _projectnamestack.Push("NemoProject");
            _projectdefinedir.Push("%.!?");
        }
        /// <summary>
        /// befor parsing a file
        /// </summary>
        /// <param name="name"></param>
        public override void OnFileEnter(string name)
        {
            string projectname = _projectnamestack.Peek();
            //ProjectRepository.Instance.Add(projectname, name);


            Scope pro = TypeRepository.Instance.AddScope(Scope.MakeProject(projectname));
            TypeRepository.Instance.EnterScope(pro);
            Scope pack = TypeRepository.Instance.AddScope(Scope.MakePackage(name));
            TypeRepository.Instance.EnterScope(pack);
        }
        /// <summary>
        /// when done parsing the file
        /// </summary>
        /// <param name="name"></param>
        public override void OnFileLeave(string name)
        {
            TypeRepository.Instance.LeaveScope();
            TypeRepository.Instance.LeaveScope();
        }
        /// <summary>
        /// enter a folder
        /// </summary>
        /// <param name="path"></param>
        void OnDirectoryEnter(string path)
        {
            string projectname = Config.GetProject(path);
            if (projectname != "")
            {
                _projectnamestack.Push(projectname);
                _projectdefinedir.Push(path);
            }
        }
        /// <summary>
        /// leaving a folder
        /// </summary>
        /// <param name="path"></param>
        void OnDirectoryLeave(string path)
        {
            if (_projectnamestack.Count > 0&&_projectdefinedir.Count>0)
            {

                string definepath = _projectdefinedir.Peek();
                if (definepath == path)
                {
                    _projectnamestack.Pop();
                    _projectdefinedir.Pop();
                }
            }
        }
    }
}


//test stub
#if(HHH)
    class Test
    {
        static void Main(string[] args)
        {
           DependencyAnalyzer.ProjectParser pp=new DependencyAnalyzer.ProjectParser();
           pp.Parse("./TestSample","*cs");
        }
    }
#endif