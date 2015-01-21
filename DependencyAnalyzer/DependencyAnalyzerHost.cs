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
 * public async static Task UpdateAsync()//async update to all other servers
 * public static void Update()   //update to all other servers
 * static public void AnalyzeLocalRepository() //analyze local code repository
 * public void Run() //run the process
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalyzer;
using System.Xml.Linq;
using Util;
using System.Threading;
namespace DependencyAnalyzer
{
    /// <summary>
    /// carry out all processes
    /// </summary>
    public class DependencyAnalyzerHost
    {
        /// <summary>
        /// update repository asycnly
        /// </summary>
        public async static void UpdateAsync()
        {
            var otherserver = ConfigManager.Instance.OtherServers;
            foreach (var ser in otherserver)
            {
                string url = ser.Value;
                await Message.MakeGetRepo(url).SendAsyc();
            }
            DependencyManager.Instance.Refresh();
        }
        /// <summary>
        /// update repository
        /// </summary>
        public static void Update()
        {
            var otherserver = ConfigManager.Instance.OtherServers;
            foreach (var ser in otherserver)
            {
                string url = ser.Value;
                //if (MessageService.Knock(url))
                //{
                    Message.MakeGetRepo(url).Send();
                    //Message.MakeGetPro(url).Send();
                //}
            }
            DependencyManager.Instance.Refresh();
        }
        /// <summary>
        /// initialize broad cast to all servers
        /// </summary>
        public static void Initialize_BroadCast()
        {
            var otherserver = ConfigManager.Instance.OtherServers;
            foreach (var ser in otherserver)
            {
                string url = ser.Value;
                Message.MakeInit(url).Send();
            }
        }
        /// <summary>
        /// async version
        /// </summary>
        /// <returns></returns>
        public static async Task Initialize_BroadCastAsync()
        {
            var otherserver = ConfigManager.Instance.OtherServers;
            foreach (var ser in otherserver)
            {
                string url = ser.Value;
                await Message.MakeInit(url).SendAsyc();
            }       
        }
        /// <summary>
        /// setup all message handlers
        /// </summary>
        void SetupHandlers()
        {
            MessageProcessor.Instance.AddHandler(new Get_Repository());
            MessageProcessor.Instance.AddHandler(new Post_Repository());
            MessageProcessor.Instance.AddHandler(new Query_Handler());
            MessageProcessor.Instance.AddHandler(new Initialize());
        }
        static Object _locker = new Object();
        /// <summary>
        /// analyze local code repos
        /// </summary>
        static public void AnalyzeLocalRepository()
        {
            lock (_locker)
            {
                TypeRepository.Instance.Reset();
                ProjectParser pp = new ProjectParser();
                List<string> paths = ConfigManager.Instance.RepositoryPaths;
                try
                {
                    foreach (string p in paths)
                    {
                        pp.Parse(p, "*.cs", ConfigManager.Instance.IsSubdirectory);
                    }
                    TypeRepository.Instance.Complete();
                    DependencyManager.Instance.Refresh();
                }
                catch (Exception e)
                {
                    DebugLog.Instance.Write(e);
                }
            }
        }
        /// <summary>
        /// start local service
        /// </summary>
        void StartupService()
        {
            MessageService.StartLocal();
        }
        /// <summary>
        /// start broad cast 
        /// </summary>
        async void StartupBroadCast()
        {
            //await UpdateAsync();
            await Initialize_BroadCastAsync();
        }
        /// <summary>
        /// parse commandline
        /// </summary>
        /// <param name="args"></param>
        void ParseCMD(string[] args)
        {
            if (args.Length > 0)
            {
                List<string> paths = new List<string>();
                foreach (string s in args)
                {
                    if (s != "_NoSub_")
                    {
                        paths.Add(s);
                    }
                    else
                    {
                        ConfigManager.Instance.IsSubdirectory = false;
                    }
                }
                if (paths.Count > 0)
                {
                    ConfigManager.Instance.RepositoryPaths.Clear();
                    ConfigManager.Instance.RepositoryPaths.AddRange(paths);
                }
            } 
        }
        /// <summary>
        /// run the workflow
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            if(args!=null)
                ParseCMD(args);
            System.Console.WriteLine("  Set up Message Handlers");
            SetupHandlers();
            System.Console.WriteLine("  Analyze repo");
            AnalyzeLocalRepository();

            System.Console.WriteLine("  Start Servcie");
            StartupService();
            System.Console.WriteLine("  Server @ {0}", ConfigManager.Instance.LocalUrl);

            System.Console.WriteLine("  Broadcast to other servers...");
            StartupBroadCast();

            DirectoryMonitor.Instance.Run();

        }
    }
}


//test stub
#if(START_MAIN)
    class Test
    {
        static void Main(string[] args)
        {
            DependencyAnalyzer.DependencyAnalyzerHost dah = new DependencyAnalyzer.DependencyAnalyzerHost();
            dah.Run(args);
            //var p=DependencyManager.Instance.PackageDep;
            //TimeSpan ts = new TimeSpan(0, 0, 70);
            //System.Console.WriteLine(ts.ToString());
            System.Console.ReadKey();
        }
    }
#endif