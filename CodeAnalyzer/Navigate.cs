/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * Navigate inst = Navigate.Instance;              //Get the instance of Navigate
 * Navigate.Instance.OnFileFound += YOURFOUNCTION  //YOURFOUNCTION should be a function that accepts string as input.
 * Navigate.Go(path,pattern);                      //Navigate the specific folder and invoke related functions
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 5  2014 created by Shikai Jin 
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
namespace CodeAnalyzer
{
    /*
     * iterate every file in a certain folder recursively 
     * this class has a delegate to let other class subscribe
     * when a file found this delegate will invoke all the 
     * related functions with the name of this found file as input argument
     */
    public class Navigate
    {
        //List<string> _files=new List<string>();
        //public List<string> Files{ get { return _files; } }
        /*
         * 
         * Implementaion of Singleton
         * One instance is enough for doing its job.
         */
        private static Navigate _instance = null;
        static public Navigate Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Navigate();
                }
                return _instance;
            }
        }
        /*
         * delegate for other classes to subscribe
         *  
         */
        public delegate void ONFILEFOUND(string path);
        /*
         * a delegate instance declare
         * event indicates that only this class has the right to
         * invoke the subcriber's function via thia delegate
         *  
         */
        event ONFILEFOUND _onfilefound=null;
        event ONFILEFOUND _ondirectoryenter = null;
        event ONFILEFOUND _ondirectoryleave = null;
        public event ONFILEFOUND OnFileFound
        {
            add { _onfilefound += value; }
            remove { _onfilefound -= value; }
        }
        public event ONFILEFOUND OnDirectoryEnter
        {
            add { _ondirectoryenter += value; }
            remove { _ondirectoryenter -= value; }
        }
        public event ONFILEFOUND OnDirectoryLeave
        {
            add { _ondirectoryleave += value; }
            remove { _ondirectoryleave -= value; }
        }

        /*
         * if isrecursive is true it will recursively iterate every
         * file in every subordinate folder.
         *  
         */
        public void Go(string path, string pattern,bool isrecursive=false)
        {
            if (_ondirectoryenter != null)
                _ondirectoryenter(path);
            string fullpath = Path.GetFullPath(path);
            string[] files=Directory.GetFiles(path, pattern);
            foreach (string file in files)
            {
                //call related functions
                if (_onfilefound != null)
                    _onfilefound(Path.GetFullPath(file));
                //_files.Add(Path.GetFullPath(file));
            }
            if (isrecursive)
            {
                string[] dirs = Directory.GetDirectories(path);

                foreach (string dir in dirs)//recursive call
                {
                    Go(dir, pattern, isrecursive);
                }
            }
            if (_ondirectoryleave != null)
                _ondirectoryleave(path);
        }
    }

    /*
     * 
     *TestStub  
     */
    class TestNavi
    {
        private Navigate navi = new Navigate();
        TestNavi()
        {
            navi.OnFileFound += print;
        }
        void print(string str)
        {
            System.Console.WriteLine(str);
        }
#if(TEST_NAVIGATE)
        static void Main(string[] args)
        {
            TestNavi t = new TestNavi();
            t.navi.Go(".", "*.*");
            System.Console.ReadLine();
        }
#endif
    }



}


