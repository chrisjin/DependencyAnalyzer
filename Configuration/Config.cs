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
 * public static string GetProject(string path)
 * 
 *  public Dictionary<string,string> OtherServers  //get all the other servers other than the localhost
 *  public string LocalName                       //name of local machine
 *  public string LocalUrl                        //url of local
 *  public List<string> RepositoryPaths           //paths that should be handled
 *  public int TimeOut                             //network connection time out
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

using System.Configuration;
namespace Util
{
    /// <summary>
    /// get project defined in _project_ file in each folder
    /// </summary>
    public class Config
    {
        static string project_metafilename = "_Project_";
        /// <summary>
        /// get project name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetProject(string path)
        { 
            string filepath=path+"\\"+project_metafilename;
            if (File.Exists(filepath))
            {
                try
                {
                    XDocument doc = XDocument.Load(filepath);
                    XElement ele = doc.Element("project").Element("name");
                    string ret = ele.Value;
                    return ret;
                }
                catch (Exception e)
                {
                    DebugLog.Instance.Write(e);
                }
                finally
                {
                    
                }
            }
            return "";
        }
    }
    public class ConfigManager
    {
        private static ConfigManager _instance = null;
        static public ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }
        public Dictionary<string,string> OtherServers{get{return _otherservers;}}    
        public string LocalName{ get { return _localname; } }
        public string LocalUrl { get { return _localurl; } }
        public List<string> RepositoryPaths { get { return _repos; } }
        public int TimeOut { get { return _timeout; } }
        Dictionary<string, string> _otherservers = new Dictionary<string, string>();
        bool _issubdir = true;
        public bool IsSubdirectory
        {
            set { _issubdir = value; }
            get { return _issubdir; }
        }
        List<string> _repos=new List<string>();
        string _localname="";
        string _localurl="";
        int _timeout = 0;
        /// <summary>
        /// get server list
        /// </summary>
        /// <param name="doc"></param>
        void FetchAllServer(XDocument doc)
        {
            
            XElement localele = doc.Root.Element("local");
            if (localele != null)
                _localname = localele.Value;
            else
                _localname = "";
            foreach (var ser in doc.Root.Element("serverlist").Elements("server"))
            {
                string n = ser.Element("name").Value;
                string u = ser.Element("url").Value;
                if (n == _localname)
                    _localurl = u;
                else
                {
                    _otherservers[n] = u;
                }
            }     
        }
        /// <summary>
        /// get repository list
        /// </summary>
        /// <param name="doc"></param>
        void FetchAllRepository(XDocument doc)
        {
            XElement repo = doc.Root.Element("repository");
            if (repo != null)
            {
                foreach (var rp in repo.Elements("dir"))
                {
                    _repos.Add(rp.Value);
                }
            }
        }
        /// <summary>
        /// get time out info
        /// </summary>
        /// <param name="doc"></param>
        void FetchTimeOut(XDocument doc)
        {
            XElement timeout = doc.Root.Element("timeout");
            if (timeout != null)
            {
                try
                {
                    _timeout = int.Parse(timeout.Value);
                }
                catch (Exception e)
                {
                    DebugLog.Instance.Write(e);
                }
            }
        }
        /// <summary>
        /// constructor
        /// </summary>
        ConfigManager()
        {
            try 
            {
                XDocument doc = XDocument.Load("Config.xml");
                FetchAllServer(doc);
                FetchAllRepository(doc);
                FetchTimeOut(doc);
            }
            catch(Exception e)
            {
                DebugLog.Instance.Write(e);
            }
        }
    }
}



#if(BBB)
    class Test
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("LocalName: {0}, Url: {1}",
                Util.ConfigManager.Instance.LocalName, Util.ConfigManager.Instance.LocalUrl);
            System.Console.WriteLine("Server Urllist:\n{0}",
                String.Join(",", Util.ConfigManager.Instance.OtherServers.Values));
            System.Console.WriteLine("Repository Paths:\n{0}",
                String.Join(",", Util.ConfigManager.Instance.RepositoryPaths));
            System.Console.WriteLine("Time out: {0}", Util.ConfigManager.Instance.TimeOut);
        }
    }
#endif