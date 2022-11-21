using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF
{

    [ServiceContract]
    public interface IChatClient
    {
        [OperationContract(IsOneWay = true)]
        void RecieveMessage(string user,string message);
    }

    [ServiceContract(CallbackContract = typeof(IChatClient))]
    public interface IChatService
    {
        [OperationContract(IsOneWay =true)]
        void Join(string username);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);

    }


    [ServiceBehavior(ConcurrencyMode=ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        Dictionary<IChatClient, string> users = new Dictionary<IChatClient, string>();
        public void Join(string username)
        {
            IChatClient connection = OperationContext.Current.GetCallbackChannel<IChatClient> ();
            users[connection] = username;

        }

        public void SendMessage(string message)
        {
            IChatClient connection = OperationContext.Current.GetCallbackChannel<IChatClient>();

            string user;

            if(!users.TryGetValue(connection, out user))
            {
                return;
            }

            foreach(var other in users.Keys)
            {
                if(other == connection)
                {
                    continue;
                }
               
                other.RecieveMessage(user, message);
                

            }
        }
    }

     class Program
    {
        static void Main(string[] args)
        {
            ServiceHost serviceHost = new ServiceHost(typeof(ChatService));
            serviceHost.Open();
            Console.WriteLine("Service is ready");
            Console.ReadLine();
            serviceHost.Close();
        }
    }
}
