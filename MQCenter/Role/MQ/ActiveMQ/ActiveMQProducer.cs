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
    class ActiveMQProducer : IProducer
    {
        IConnectionFactory factory;
        QConfig qConfig;
        IConnection connection;
        ISession session;
        bool running = true;

        public ActiveMQProducer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            string _ip = string.IsNullOrWhiteSpace(qConfig.IP) ? ActiveMQConstant.DEFAULTIP : qConfig.IP;
            int _port = qConfig.Port == 0 ? ActiveMQConstant.DEFAULTPORT : qConfig.Port;
            factory = new ConnectionFactory(string.Format("tcp://{0}:{1}/", _ip, _port));
        }

        public void SendTextMsg(string textMsg)
        {
            //using (IConnection connection = factory.CreateConnection())
            connection = factory.CreateConnection();
            {
                //Create the Session
                session = connection.CreateSession();
                {
                    //Create the Producer for the topic/queue
                    //IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(queueUrl.QueueName));  //broadcast
                    IMessageProducer prod = session.CreateProducer(new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue(qConfig.QueueName));

                    //Send Messages
                    ITextMessage msg = prod.CreateTextMessage();
                    msg.Text = textMsg;
                    long amount = qConfig.Amount;
                    Parallel.For(0, amount, (i, loopState) =>
                    {
                        prod.Send(msg, convert2AMQMsgDeliveryMode(qConfig.PersistentMode), MsgPriority.Normal, TimeSpan.MinValue);
                    });
                }
            }
        }

        private static MsgDeliveryMode convert2AMQMsgDeliveryMode(QConfig.PersistentSet persistentMode)
        {
            MsgDeliveryMode mode;
            switch (persistentMode)
            {
                case QConfig.PersistentSet.NonPersistent:
                    mode = MsgDeliveryMode.NonPersistent;
                    break;
                case QConfig.PersistentSet.Persistent:
                    mode = MsgDeliveryMode.Persistent;
                    break;
                default:
                    mode = MsgDeliveryMode.NonPersistent;
                    break;
            }
            return mode;
        }

        public void Dispose()
        {
            //channel.QueueDelete(queueUrl.QueueName);
            running = false;
            if (session != null)
            {
                //channel.Abort();
                session.Close();
                session.Dispose();
                session = null;
            }
            if (connection != null && connection.IsStarted)
            {
                //connection.Abort();
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }
    }
}
