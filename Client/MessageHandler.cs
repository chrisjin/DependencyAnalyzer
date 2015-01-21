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
 * public 
 * Hanlde, handle message there are three kinds of messages to get project list, query dependencies
 * 
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyAnalyzer;
using System.Windows.Controls;
using System.Xml.Linq;
using Util;
using System.Windows.Threading;
namespace Client
{

    class REC_ANALYSIS:IMessageHandler
    {
         /// <summary>
         /// Handle message
         /// </summary>
         /// <param name="m"></param>
         /// <param name="callback"></param>
        public void Handle(Message m, IMessageServiceCallback callback)
        {
            if (m.Action == MessageType.REPLY)
            {
                Query q = Query.Parse(m.Content);
                switch (q.Type)
                { 
                    case Query.QType.LIST:
                        handleprojectlist(q.Target);
                        break;
                    case Query.QType.PACKAGE_DEPENDENCY:
                        handlepackagedep(q.Target);
                        break;
                    case Query.QType.TYPE_DEPENDENCY:
                        handletypedep(q.Target);
                        break;
                }
            }
        }
        /// <summary>
        /// handle type dependency
        /// </summary>
        /// <param name="list"></param>
        void handletypedep(string list)
        {
            //System.Console.WriteLine(list);
            var aa=DictionaryParser.XML2Dict(list);
            Action ac = () =>
            {
                MainWindow.Instance.DrawDependency(aa,false);
                MainWindow.Instance.XMLTextBox.Text = list;
            };
            MainWindow.Instance.
                    Dispatcher.Invoke(ac, System.Windows.Threading.DispatcherPriority.Background);
        }
        /// <summary>
        /// handle pakcage dependency
        /// </summary>
        /// <param name="list"></param>
        void handlepackagedep(string list)
        {
            //System.Console.WriteLine(list);
            var aa = DictionaryParser.XML2Dict(list);
            Action ac = () =>
            {
                MainWindow.Instance.DrawDependency(aa,true);
                MainWindow.Instance.XMLTextBox.Text = list;
            };
            MainWindow.Instance.
                    Dispatcher.Invoke(ac, System.Windows.Threading.DispatcherPriority.Background);
            
        }
        /// <summary>
        /// handle project list
        /// </summary>
        /// <param name="list"></param>
        void handleprojectlist(string list)
        {
            try
            {
                XDocument doc = XDocument.Parse(list);
                List<string> pl = new List<string>();
                foreach (var ap in doc.Root.Elements("project"))
                {
                    string name = ap.Element("name").Value;
                    pl.Add(name);
                }
                Action ac = () =>
                {
                    MainWindow.Instance.ProjectList.Items.Clear();
                    for (int i = 0; i < pl.Count; i++)
                        MainWindow.Instance.ProjectList.Items.Add(pl[i]);
                };
                MainWindow.Instance.
                    Dispatcher.Invoke(ac,System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception e)
            {
                DebugLog.Instance.Write(e);
            }
        }
    }
}


#if(AAA)
    class Test
    {
        static void Main(string[] args)
        {
            Client.REC_ANALYSIS ra = new Client.REC_ANALYSIS();
            Message message = Message.MakeGetRepo("http://localhost:8080/server1");
            ra.Handle(message, new MessageServiceCallback());
        }
    }
#endif