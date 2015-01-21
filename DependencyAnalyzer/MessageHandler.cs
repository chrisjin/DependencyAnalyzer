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
 * Get_Repository.Handle //implementaion of messagehandler that tend to handle each kind of messages
 * Post_Repository.Handle//handle post message
 * Query_Handler.Handle //handle query message
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalyzer;
using Util;
namespace DependencyAnalyzer
{

    class Initialize : IMessageHandler
    {
        /// <summary>
        /// handle the init message
        /// </summary>
        /// <param name="m"></param>
        /// <param name="callback"></param>
        public void Handle(Message m, IMessageServiceCallback callback)
        {
            if (m.Action == MessageType.INITIALIZE)
            {
                foreach (Scope s in TypeRepository.Instance.RootScope.Child)
                {
                    if (s.Type == SType.PROJECT)
                    {
                        Message ret = Message.Make("", MessageType.POST_REPOSITORY,
                            Scope.Scope2XML(s,
                            //(ss) => { return !ScopeCategory.TrivialTypes.Contains(s.Type); }
                            (ss) => { return true; }
                            ).ToString());
                        ret.SendBack(callback);
                    }
                }
                Message getrepo = Message.MakeGetRepo("");
                getrepo.SendBack(callback);
            }
        }
    }
    class Get_Repository : IMessageHandler
    {
        /// <summary>
        /// handle the get repo message
        /// </summary>
        /// <param name="m"></param>
        /// <param name="callback"></param>
        public void Handle(Message m, IMessageServiceCallback callback)
        {
            if (m.Action == MessageType.GET_REPOSITORY)
            {
                System.Console.WriteLine("Msg: {0}", m.Action);
                foreach (Scope s in TypeRepository.Instance.RootScope.Child)
                {
                    if (s.Type == SType.PROJECT)
                    {
                        Message ret = Message.Make("", MessageType.POST_REPOSITORY,
                            Scope.Scope2XML(s,
                            //(ss) => { return !ScopeCategory.TrivialTypes.Contains(s.Type); }
                            (ss) => { return true; }
                            ).ToString());
                        ret.SendBack(callback);
                    }
                }
            }
        }
    }
    //class Get_Project : IMessageHandler
    //{
    //    public void Handle(Message m, IMessageServiceCallback callback)
    //    {
    //        if (m.Action == MessageType.GET_PROJECT)
    //        {
    //            System.Console.WriteLine("Msg: {0}", m.Action);
    //            Message ret = Message.Make("", MessageType.POST_PROJECT, ProjectRepository.Instance.MakeXML().ToString());
    //            string head = ret.MakeHeader();
    //            string content = ret.MakeContent();
    //            try
    //            {
    //                if (callback != null)
    //                    callback.Return(head, content);
    //            }
    //            catch (Exception e)
    //            {
    //                DebugLog.Instance.Write(e);
    //            }
    //        }
    //    }
    //}
    class Post_Repository : IMessageHandler
    {
        /// <summary>
        /// show info when merging a project
        /// </summary>
        /// <param name="sc"></param>
        void ShowBriefInfo(Scope sc)
        {
            System.Console.WriteLine("  Merging Project:{0}", sc.Name);
        }
        /// <summary>
        /// on receiving a project
        /// </summary>
        /// <param name="m"></param>
        /// <param name="callback"></param>
        public void Handle(Message m, IMessageServiceCallback callback)
        {
            if (m.Action == MessageType.POST_REPOSITORY)
            {
                System.Console.WriteLine("Msg: {0}", m.Action);
                string ele=m.Content;
                Scope remotescope = Scope.XML2Scope(ele);
                ShowBriefInfo(remotescope);
                TypeRepository.Instance.MergeProject(remotescope);
                TypeRepository.Instance.Complete();
                DependencyManager.Instance.Refresh();
               // System.Console.WriteLine(Scope.Scope2XML(TypeRepository.Instance.RootScope).ToString());
            }
        }
    }
    //class Post_Project : IMessageHandler
    //{
    //    public void Handle(Message m, IMessageServiceCallback callback)
    //    {
    //        if (m.Action == MessageType.POST_PROJECT)
    //        {
    //            System.Console.WriteLine("Msg: {0}", m.Action);
    //            ProjectRepository.Instance.MergeXML(m.Content);
    //        }
    //    }
    //}
    class Query_Handler : IMessageHandler
    {
        /// <summary>
        /// handle all kinds of queries such as query for project, query for types deps ,etc
        /// </summary>
        /// <param name="m"></param>
        /// <param name="callback"></param>
        public void Handle(Message m, IMessageServiceCallback callback)
        {
            if (m.Action == MessageType.QUERY)
            {
                System.Console.WriteLine("Msg: {0}", m.Action);
                //DependencyAnalyzerHost.UpdateAsync();
                Query q = Query.Parse(m.Content);
                switch (q.Type)
                { 
                    case Query.QType.LIST:
                        sendprojectlist(q, callback);
                        break;
                    case Query.QType.TYPE_DEPENDENCY:
                        sendtypedependency(q, callback);
                        break;
                    case Query.QType.PACKAGE_DEPENDENCY:
                        sendpackagedependency(q, callback);
                        break;
                }

            }
        }
        /// <summary>
        /// run callack function
        /// </summary>
        /// <param name="con"></param>
        /// <param name="callback"></param>
        void sendback(string con, IMessageServiceCallback callback)
        {
            Message m = Message.Make("", MessageType.REPLY, con);
            callback.Return(m.MakeHeader(), m.MakeContent());
        }
        /// <summary>
        /// send back the project list
        /// </summary>
        /// <param name="q"></param>
        /// <param name="callback"></param>
        void sendprojectlist(Query q,IMessageServiceCallback callback)
        {
            DirectoryMonitor.Instance.HandleChanged();
            q.Target = ProjectRepository.Instance.MakeXML().ToString();
            sendback(q.ToXML().ToString(), callback);
        }
        /// <summary>
        /// send back the pck deps
        /// </summary>
        /// <param name="q"></param>
        /// <param name="callback"></param>
        void sendpackagedependency(Query q, IMessageServiceCallback callback)
        {
            q.Target = DependencyManager.Instance.GetPackageDependency(q.Target).ToString();
            sendback(q.ToXML().ToString(), callback);
        }
        /// <summary>
        /// send back type deps
        /// </summary>
        /// <param name="q"></param>
        /// <param name="callback"></param>
        void sendtypedependency(Query q, IMessageServiceCallback callback)
        {
            q.Target = DependencyManager.Instance.GetTypeDependency(q.Target).ToString();
            sendback(q.ToXML().ToString(), callback);
        }
    }
}


//test stub
#if(GGG)
    class Test
    {
        static void Main(string[] args)
        {
            //should be tested with other packages.
        }
    }
#endif