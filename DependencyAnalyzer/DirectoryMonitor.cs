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
 * public void HandleChanged()//handle all changes that happens to a observed path
 * public void Run()  //run the watching
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;
using Util;
using System.Timers;
namespace DependencyAnalyzer
{
    /// <summary>
    /// to monitor the changes of a directory
    /// </summary>
    class DirectoryMonitor
    {
        private static DirectoryMonitor _instance = null;
        static public DirectoryMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DirectoryMonitor();
                }
                return _instance;
            }
        }
        /// <summary>
        /// add watcher to a certain folder
        /// </summary>
        /// <param name="path"></param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        void AddWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName ;

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            
            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            // Begin watching.
            watcher.EnableRaisingEvents = true;

            _watcherlist.Add(watcher);
        }
        List<FileSystemWatcher> _watcherlist = new List<FileSystemWatcher>();

        
        bool __dirty=false;
        bool _dirty
        {
            get { lock (_locker) { return __dirty; } }
            set { lock (_locker) { __dirty = value; } }
        }
        Object _locker = new Object();
        /// <summary>
        /// if changes happen this func keep the repo up to date
        /// </summary>
        public void HandleChanged()
        {
            lock (_locker)
            {
                if (_dirty)
                {

                    DependencyAnalyzerHost.AnalyzeLocalRepository();
                    _dirty = false;
                    foreach (var ser in ConfigManager.Instance.OtherServers)
                    {
                        ProjectRepository.Instance.SendProject(ser.Value);
                    }
                }
            }
        }
        private static Timer aTimer;
        /// <summary>
        /// 
        /// </summary>
        DirectoryMonitor()
        {
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DirectoryMonitor.Instance.HandleChanged();
        }
        /// <summary>
        /// run the watch
        /// </summary>
        public void Run()
        {
            try
            {
                foreach (string p in ConfigManager.Instance.RepositoryPaths)
                {
                    AddWatcher(p);
                }
            }
            catch (Exception e)
            {
                DebugLog.Instance.Write(e);
            }
        }
        /// <summary>
        /// handle changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("  File: " + e.FullPath + " " + e.ChangeType);
            DirectoryMonitor.Instance._dirty = true;
            
        }
        /// <summary>
        /// handle name changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("  File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            DirectoryMonitor.Instance._dirty = true;
        }
    }
}

//test stub
#if(FFF)
    class Test
    {
        static void Main(string[] args)
        {
            DependencyAnalyzer.DirectoryMonitor.Instance.Run();
            DependencyAnalyzer.DirectoryMonitor.Instance.HandleChanged();
        }
    }
#endif