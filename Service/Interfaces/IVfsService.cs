using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Emroy.Vfs.Service.Dto;


namespace Emroy.Vfs.Service.Interfaces
{
    [ServiceContract(SessionMode = SessionMode.Required, 
        CallbackContract = typeof (IVfsServiceCallback))
        ]
 
    public interface IVfsService
    {
        /// <summary>
        /// Connect to service
        /// </summary>
        /// <param name="userName">user name to use</param>
        /// <returns>response <see cref="Response"/></returns>
        [OperationContract(IsInitiating = true)]
        Response Connect(string userName);

        /// <summary>
        /// Performs command
        /// </summary>
        /// <param name="command"></param>
        /// <returns>response <see cref="Response"/></returns>
        [OperationContract(IsInitiating = false)]
        Response PerformCommand(VfsCommand command);

        /// <summary>
        /// Disconnect user
        /// </summary>
        /// <param name="userName">user name to disconnect</param>
        /// <returns>response <see cref="Response"/></returns>
        [OperationContract(IsTerminating = true, IsInitiating = false)]
        Response Disconnect(string userName);


 
    }

    /// <summary>
    /// Response data
    /// </summary>
    [DataContract]
    public class Response
    {
        /// <summary>
        /// Message returned to client
        /// </summary>
        [DataMember]
        public string Message; 

        /// <summary>
        /// True if fail
        /// </summary>
        [DataMember]
        public bool Fail;

        [DataMember]
        public object Data;
    }

    /// <summary>
    /// Service Callback for feedback
    /// </summary>
    public interface IVfsServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Feedback(string login, string message);
    }
}
