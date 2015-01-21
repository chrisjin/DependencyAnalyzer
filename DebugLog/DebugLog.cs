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
 * public void Write(string str, params object[] arg)//write a string to a log file
 * public void WriteLine(string str, params object[] arg)//write a new line to the log file
 * public void Write(Exception e)  //write the exception to the log file
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
namespace Util
{
    public class DebugLog
    {
        private static DebugLog _instance = null;
        static public DebugLog Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DebugLog();
                }
                return _instance;
            }
        }
        Object _locker=new Object();
        /// <summary>
        /// constructor
        /// </summary>
        DebugLog()
        {
            using (FileStream fs = new FileStream("log.txt", FileMode.Create))
            { 
            }
        }
        /// <summary>
        /// write string in the file
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        public void Write(string str, params object[] arg)
        {
            lock (_locker)
            {
                using (FileStream fs = new FileStream("log.txt", FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(str, arg);
                    }
                }
            }
        }
        /// <summary>
        /// write a new line to the file
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        public void WriteLine(string str, params object[] arg)
        {
            lock (_locker)
            {
                using (FileStream fs = new FileStream("log.txt", FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(str, arg);
                    }
                }
            }
        }
        /// <summary>
        /// write exception to the file with stack info
        /// </summary>
        /// <param name="e"></param>
        public void Write(Exception e)
        {
            WriteLine("From: {0}", e.Source);
            WriteLine("{0}", e.Message);
            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                if (sf.GetFileName() != null)
                    WriteLine("  File: {0}, Line Number: {1}, Column Number {2}",
                        sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());

            }
            WriteLine("");
            WriteLine("");
        }
    }
}


//test stub
#if(CCC)
    class Test
    {
        static void Main(string[] args)
        {
            Util.DebugLog.Instance.Write(new Exception());
            Util.DebugLog.Instance.WriteLine("This is an exception!");
        }
    }
#endif