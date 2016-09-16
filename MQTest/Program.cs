using Library.Serializer;
using Vector.Role;
using Vector.Role.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vector;

namespace MQTest
{
    class Program
    {
        static void Main(string[] args)
        {
            QConfig qConfig = new QConfig();
            qConfig.Amount = 1;
            qConfig.ConfirmPublishTimeout = 1000;
            qConfig.CreateNewQueue = false;
            qConfig.Callback = callback;
            //qConfig.HostName = "11";
            qConfig.RoutingKey = "";
            qConfig.IP = "192.168.87.13";
            qConfig.MQType = MQTypeEnum.RabbitMQ;
            qConfig.UserName = "admin";
            qConfig.Password = "ep123456";
            qConfig.PersistentMode = QConfig.PersistentSet.NonPersistent;
            qConfig.PersistentModeReply = QConfig.PersistentSet.NonPersistent;
            qConfig.Port = 5672;
            qConfig.QueueName = "SB";
            qConfig.RequiredReply = true;
            qConfig.Exchange = "exchange1";
            qConfig.RoutingKey = "key";


            IProducer producer = MQFactory.GenerateProducer(qConfig);
            //producer.SendTextMsg("1111");

            IConsumer consumer = MQFactory.GenerateConsumer(qConfig);
            //consumer.Pop();

            RMQAPI api = new RMQAPI("192.168.87.11", "admin", "ep123456");

            var totalMount = api.APIOverview();
            //var fas = api.Op("overview").object_totals;
            Console.ReadLine();
        }

        private static void callback(string s)
        {
            Console.WriteLine(s);
        }


    }
}
