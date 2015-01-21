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
 * public class MessageService : IMessageService
 * public void Send(string header, string content) //implementation of communication contract to send message
 * public void Knock()                            //determine if the server is accessible
 * 
 * public class MessageServiceCallback : IMessageServiceCallback
 * public void Return(string header, string content)// callback message
 * 
 * public class MessageType
 * 
 * public static ServiceHost GetHost(string url)  //get service host 
 * public static void StartLocal()                //start service
 * public static IMessageService GetRemote(string url, IMessageServiceCallback callback) //get endpoint
 * public static bool Knock(string url)          //determine whther the server is available
 * 
 * public class Query
 * public XElement ToXML()                      //convert the query to xml
 * public static Query Parse(string s)          //parse a query text
 * public static Query Make(QType t, string taeget) //form a query
 * 
 * public class Message
 * public string MakeHeader()                   //generate text header from inner data
 * public string MakeContent()                  //generate text content from innner data
 * public static Message Parse(string header, string content) //parse text into header and content
 * 
 * Make(string sender, string rec, string action, string content) //make a generic message
 * public static Message MakeQuery(string rec, string content)   //make a query message
 * public static Message MakeGetRepo(string rec)                 //make get repo message
 * public void Send()                          //send to the url designated in the inner data fields
 * public void SendAsync()                    //async send
 * 
 * public class MessageProcessor
 * public void AddHandler(IMessageHandler h)   //add message handlers
 * public void Handle(Message m,IMessageServiceCallback mc) //handle a specific message
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml;
using Util;
namespace DependencyAnalyzer
{
    /// <summary>
    /// impl of the contract
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MessageService : IMessageService
    {
        IMessageServiceCallback _callback = null;
        /// <summary>
        /// init the call back
        /// </summary>
        public MessageService()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IMessageServiceCallback>();
        }
        /// <summary>
        /// send message
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        public void Send(string header, string content)
        {
            Message m = Message.Parse(header, content);
            MessageProcessor.Instance.Handle(m, _callback);
        }
        /// <summary>
        /// ping a server
        /// </summary>
        public void Knock()
        {
            return;
        }
        /// <summary>
        /// create a binding that is able to handle long texts
        /// </summary>
        /// <returns></returns>
        static WSDualHttpBinding MostPowerfulBinding()
        {
            WSDualHttpBinding binding = new WSDualHttpBinding();
            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.Security.Mode = WSDualHttpSecurityMode.None;
            binding.UseDefaultWebProxy = false;                         /////F****CK!!!!
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            binding.ReaderQuotas.MaxBytesPerRead = 10485760;
            binding.ReaderQuotas.MaxDepth = 2000000;
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            binding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            return binding;
        
        }
        /// <summary>
        /// get server host
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ServiceHost GetHost(string url)
        {
            WSDualHttpBinding binding = MostPowerfulBinding();
            Uri address = new Uri(url);
            Type service = typeof(MessageService);
            ServiceHost host = new ServiceHost(service, address);

            host.AddServiceEndpoint(typeof(IMessageService), binding, address);
            return host;
        }
        /// <summary>
        /// start the local service
        /// </summary>
        public static void StartLocal()
        {
            ServiceHost h = GetHost(ConfigManager.Instance.LocalUrl);
            h.Open();
        }
        static Dictionary<string, DuplexChannelFactory<IMessageService>> _url2channel = 
            new Dictionary<string, DuplexChannelFactory<IMessageService>>();


        static Object _locker = new Object();
        static Dictionary<string, DuplexChannelFactory<IMessageService>> Url2Channel
        {
            get { lock (_locker) { return _url2channel; } }
        }
        /// <summary>
        /// get remote endpoint
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IMessageService GetRemote(string url, IMessageServiceCallback callback)
        {
            WSDualHttpBinding binding = MostPowerfulBinding();
            //binding.Security.Mode = WSDualHttpSecurityMode.None;
            binding.SendTimeout = new TimeSpan(0, 0, ConfigManager.Instance.TimeOut);
            EndpointAddress address = new EndpointAddress(url);
            InstanceContext instanceContext = new InstanceContext(callback);
            DuplexChannelFactory<IMessageService> factory = new DuplexChannelFactory<IMessageService>(instanceContext, binding, address);
            
            return factory.CreateChannel();
        }
        /// <summary>
        /// get remote endpoint with default callback
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IMessageService GetRemote(string url)
        {
            return GetRemote(url, new MessageServiceCallback());
        }
        /// <summary>
        /// ping a server
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Knock(string url)
        {
            bool ret = true;
            int count=0;
            while (true)
            {
                try
                {
                    count++;
                    IMessageService ms = MessageService.GetRemote(url,new MessageServiceCallback());
                    ms.Knock();
                    break;
                }
                catch (Exception e)
                {
                    if (count > 2)
                    {
                        System.Console.WriteLine("  {0} is not accessible...", url);
                        DebugLog.Instance.Write(e);
                        ret = false;
                        break;

                    }
                    System.Console.WriteLine("  Knocking...");
                }
            }
            return ret;
        }
    }
    /// <summary>
    /// impl of callback
    /// </summary>
    public class MessageServiceCallback : IMessageServiceCallback
    {
        /// <summary>
        /// callback func. give the control to the message processor
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        public void Return(string header, string content)
        {
            Message m = Message.Parse(header, content);
            MessageProcessor.Instance.Handle(m, null);       
        }
    }
    /// <summary>
    /// all message types
    /// </summary>
    public class MessageType
    {
        public static string GET_REPOSITORY { get { return "GET_REPOSITORY"; } }
        public static string POST_REPOSITORY { get { return "POST_REPOSITORY"; } }
        public static string GET_PROJECT { get { return "GET_PROJECT"; } }
        public static string POST_PROJECT { get { return "POST_PROJECT"; } }
        public static string QUERY { get { return "QUERY"; } }
        public static string REPLY { get { return "REPLY"; } }
        public static string INITIALIZE { get { return "INITIALIZE"; } }

    }
    /// <summary>
    /// the content of a query message
    /// </summary>
    public class Query
    {
        public enum QType
        { 
            LIST,
            RECURSIVE_LIST,
            PACKAGE_DEPENDENCY,
            TYPE_DEPENDENCY
        }
        public QType Type { set; get; }
        public string Target { set; get; }
        /// <summary>
        /// convert to xml
        /// </summary>
        /// <returns></returns>
        public XElement ToXML()
        {
            //if(Target!="")

            XElement e = new XElement(Type.ToString(), Target);
            return e;
        }
        /// <summary>
        /// parse xml to get Query object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Query Parse(string s)
        {
            XDocument doc = XDocument.Parse(s);
            
            QType t=(QType)Enum.Parse(typeof(QType),doc.Root.Name.ToString());
            string targ = doc.Root.Value;

            return Make(t, targ);
        }
        /// <summary>
        /// make a query
        /// </summary>
        /// <param name="t"></param>
        /// <param name="taeget"></param>
        /// <returns></returns>
        public static Query Make(QType t, string taeget)
        {
            Query q = new Query();
            q.Target = taeget;
            q.Type = t;
            return q;
        }

    }
    public class Message
    {
        string _sender="";
        string _receiver="";
        string _action="";
        string _content="";

        public string Sender { get { return _sender; } }
        public string Receiver { get { return _receiver; } }
        public string Action { get { return _action; } }
        public string Content { get { return _content; } }
        /// <summary>
        /// make header from the data fields
        /// </summary>
        /// <returns></returns>
        public string MakeHeader()
        {
            XDocument headerdoc = new XDocument();
            headerdoc.Add(new XElement("header"));
            headerdoc.Root.Add(new XElement("sender", _sender));
            headerdoc.Root.Add(new XElement("receiver", _receiver));
            headerdoc.Root.Add(new XElement("action", _action));
            return headerdoc.ToString();
        }
        /// <summary>
        /// make contents from data fields
        /// </summary>
        /// <returns></returns>
        public string MakeContent()
        {
            XDocument contentdoc = new XDocument();
            contentdoc.Add(new XElement("content", _content));
            return contentdoc.ToString();
        }
        /// <summary>
        /// parsing the text to get message object
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Message Parse(string header, string content)
        {
            Message m = new Message();
            try
            {
                XDocument headerdoc = XDocument.Parse(header);
                m._sender = headerdoc.Root.Element("sender").Value;
                m._receiver = headerdoc.Root.Element("receiver").Value;
                m._action = headerdoc.Root.Element("action").Value;

                XDocument contdoc = XDocument.Parse(content);
                m._content = contdoc.Root.Value;
            }
            catch (Exception e)
            {
                DebugLog.Instance.Write(e);
            }
            return m;
        }
        /// <summary>
        /// generic message 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rec"></param>
        /// <param name="action"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Message Make(string sender, string rec, string action, string content)
        {
            Message m = new Message();
            m._sender = sender;
            m._receiver = rec;
            m._action = action;
            m._content = content;
            return m;
        }
        /// <summary>
        /// message with default sender
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="action"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Message Make(string rec, string action, string content)
        {
            return Make(ConfigManager.Instance.LocalUrl,
                rec,action,content);
        }
        /// <summary>
        /// gen a query message
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Message MakeQuery(string rec, string content)
        {
            return Make(rec, MessageType.QUERY, content);
        }
        /// <summary>
        /// gen a message to get repos
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public static Message MakeGetRepo(string rec)
        {
            string time = DateTime.Now.ToUniversalTime().ToString();
            return Make(rec, MessageType.GET_REPOSITORY, time);
        }
        /// <summary>
        /// gen a message to initilize
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public static Message MakeInit(string rec)
        {
            string time = DateTime.Now.ToUniversalTime().ToString();
            return Make(rec, MessageType.INITIALIZE, time);      
        }
        //public static Message MakeGetPro(string rec)
        //{
        //    string time = DateTime.Now.ToUniversalTime().ToString();
        //    return Make(rec, MessageType.GET_PROJECT, time);     
        //}
        /// <summary>
        /// send to the url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool _SendTo(string url)
        {
            if (url.Length == 0)
                return false;
            int count = 0;
            while (true)
            {
                try
                {
                    count++;
                    IMessageService ms = MessageService.GetRemote(url,new MessageServiceCallback());
                    string header = MakeHeader();
                    string content = MakeContent();
                    System.Console.Write("  Sending: {0}", this.Action);
                    ms.Send(header, content);
                    System.Console.WriteLine("  Succeed!! {0}", url);
                    return true;
                }
                catch (Exception e)
                {
                    if (count > 2)
                    {
                        DebugLog.Instance.Write(e);
                        DebugLog.Instance.WriteLine("Connection Failed!");
                        System.Console.WriteLine("  Connection Failed! {0}", url);
                        break;
                    }
                    System.Console.WriteLine("  Retry... {0}",url);
                }
            }
            return false;
        }
        /// <summary>
        /// async sending 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendAsyc()
        {
            bool ret=await Task<bool>.Run(() => {return _SendTo(_receiver); });
            return ret;
        }
        /// <summary>
        /// sending to the sever refered in the message
        /// </summary>
        /// <returns></returns>
        public bool Send()
        {
            return _SendTo(_receiver);
        }
        /// <summary>
        /// send message though the callback func
        /// </summary>
        /// <param name="callback"></param>
        public void SendBack(IMessageServiceCallback callback)
        {
            string head = MakeHeader();
            string content =MakeContent();
            try
            {
                if (callback != null)
                {
                    System.Console.Write("  Sending Back: {0}", this.Action);
                    callback.Return(head, content);
                    System.Console.WriteLine("  Succeed!!");
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("  Failed!!");
                DebugLog.Instance.Write(e);
            }
        }
    }
    /// <summary>
    /// container for all message handlers
    /// </summary>
    public class MessageProcessor
    {
        private static MessageProcessor _instance = null;
        static public MessageProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MessageProcessor();
                }
                return _instance;
            }
        }
        delegate void Actions(Message m,IMessageServiceCallback mc);
        event Actions ACTIONS=null;
        /// <summary>
        /// add a message handler
        /// </summary>
        /// <param name="h"></param>
        public void AddHandler(IMessageHandler h)
        {
            ACTIONS += h.Handle;
        }
        /// <summary>
        /// handle the incoming messages
        /// </summary>
        /// <param name="m"></param>
        /// <param name="mc"></param>
        public void Handle(Message m,IMessageServiceCallback mc)
        {
            //Task.Run(() => { ACTIONS(m, mc); });
            ACTIONS(m, mc);
        }

    }
}


//test stub
#if(KKK)
    class Test
    {
        static void Main(string[] args)
        {
            DependencyAnalyzer.MessageService.StartLocal();
            DependencyAnalyzer.MessageService.GetHost("http://localhost:8080/server1").Open();
            var remote=DependencyAnalyzer.MessageService.GetRemote("http://localhost:8080/server1");
        
            DependencyAnalyzer.Message m=DependencyAnalyzer.Message.MakeGetRepo("http://localhost:8080/server1");
            m.Send();
            m.SendAsyc();

            //others are tested with other modules
        }
    }
#endif