using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Vector.Role.Interface;

namespace Vector.Role.MQ.ActiveMQ
{
    class ActiveMQConsumer : IConsumer
    {
        IConnectionFactory factory;
        IConnection connection;
        ISession session;
        delegate void ReceiveCallback(string msg);
        event ReceiveCallback callback;
        QConfig qConfig;

        public ActiveMQConsumer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            factory = new ConnectionFactory("tcp://" + qConfig.IP + ":" + qConfig.Port + "/");
            if (qConfig.Callback != null)
                callback = new ReceiveCallback(qConfig.Callback);
        }

        public void Tap()
        {
            using (IConnection connection_Tap = factory.CreateConnection(qConfig.UserName, qConfig.Password))
            {
                try
                {
                    connection_Tap.ClientId = Guid.NewGuid().ToString();
                    connection_Tap.Start();
                }
                finally
                {
                    connection_Tap.Close();
                    connection_Tap.Dispose();
                }
            }
        }

        public void Listen()
        {
            //Create the connection
            connection = factory.CreateConnection();
            {
                connection.ClientId = Guid.NewGuid().ToString();
                connection.Start();

                //Create the Session
                session = connection.CreateSession();
                {
                    //Create the Consumer
                    //IMessageConsumer consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic("testing"), "testing listener", null, false);
                    IMessageConsumer consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(qConfig.QueueName));
                    consumer.Listener += new MessageListener(consumer_Listener);
                    //var aadfs = consumer.Receive(new TimeSpan( 900));
                }
            }
        }

        public void Dispose()
        {
            if (session != null)
            {
                session.Close();
                session.Dispose();
            }
            if (connection != null)
            {
                connection.Stop();
                connection.Close();
                connection.Dispose();
            }
        }

        void consumer_Listener(IMessage message)
        {
            ITextMessage msg = (ITextMessage)message;
            if (callback != null)
                callback(msg.Text);
            //Console.WriteLine("Receive: " + msg.Text);
        }

        public void Pop()
        {
            connection = factory.CreateConnection();
            {
                connection.ClientId = Guid.NewGuid().ToString();
                connection.Start();

                //Create the Session
                session = connection.CreateSession();
                {
                    //Create the Consumer
                    //IMessageConsumer consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic("testing"), "testing listener", null, false);
                    IMessageConsumer consumer = session.CreateConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(qConfig.QueueName));
                    ITextMessage msg = (ITextMessage)consumer.Receive(new TimeSpan(3000));
                    callback(msg.Text);
                }
            }
        }
    }


}
