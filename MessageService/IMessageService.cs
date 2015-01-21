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
 *void Send(string header, string content); //contract to send message
 *void Knock();    //contract to determine if a server is accessible
 *
 * void Return(string header, string content); //service callback
 * 
 * void Handle(Message m, IMessageServiceCallback callback);//message handler base functions
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
namespace DependencyAnalyzer
{
    /// <summary>
    /// message contract
    /// </summary>
    [ServiceContract(Namespace = "DependencyAnalyzer", SessionMode = SessionMode.Required,
                    CallbackContract = typeof(IMessageServiceCallback))]
    public interface IMessageService
    {
        /// <summary>
        /// send mesage
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        [OperationContract(IsOneWay = true)]
        void Send(string header, string content);
        /// <summary>
        /// decide whether a server is on
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Knock();
    }
    public interface IMessageServiceCallback
    {
        /// <summary>
        /// callback 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        [OperationContract(IsOneWay = true)]
        void Return(string header, string content);
    }
    public interface IMessageHandler
    {
        /// <summary>
        /// interface representing the message handler
        /// </summary>
        /// <param name="m"></param>
        /// <param name="callback"></param>
        void Handle(Message m, IMessageServiceCallback callback);
    }
}


//test stub
#if(JJJ)
    class Test
    {
        static void Main(string[] args)
        {
            //Tested with other packages
        }
    }
#endif