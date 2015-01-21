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
    class Navigate
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
        public event ONFILEFOUND OnFileFound=null;
        /*
         * if isrecursive is true it will recursively iterate every
         * file in every subordinate folder.
         *  
         */
        public void Go(string path, string pattern,bool isrecursive=false)
        {
            string fullpath = Path.GetFullPath(path);
            string[] files=Directory.GetFiles(path, pattern);
            foreach (string file in files)
            {
                //call related functions
                if (OnFileFound != null)
                    OnFileFound(Path.GetFullPath(file));
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
        }
    }

    /*
     * 
     *TestStub  
     */
    class TestNavi
    {
        public Navigate navi = new Navigate();
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


