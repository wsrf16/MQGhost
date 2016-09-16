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
using System.Messaging;
using System.Text.RegularExpressions;
using Vector.Role.Interface;

namespace Vector.Role.MQ.MSMQ
{
    class MSMQProducer : IProducer
    {
        QConfig qConfig;
        string queuePath;
        MessageQueue queue;
        Message message;
        bool running = true;

        public MSMQProducer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            string _ip = string.IsNullOrWhiteSpace(qConfig.IP) ? MSMQConstant.DEFAULTIP : qConfig.IP;
            string _port = qConfig.Port == 0 ? string.Empty : (":" + qConfig.Port);
            queuePath = (_ip + _port + qConfig.QueueName).Trim();
            queuePath = MSMQHelper.MakeMQPath(queuePath);
            //QueuePath = ".\\private$\\globalprocess_6";
        }


        public void SendTextMsg(string textMsg)
        {
            if (qConfig.IsTransaction)
                SendByTransaction(textMsg);
            else
                Send(textMsg);
        }

        private void Send(string textMsg)
        {
            //using (MessageQueue queue = new MessageQueue())
            queue = new MessageQueue();
            {
                //using (Message message = new Message())
                message = new Message();
                {
                    message.Label = qConfig.Label;
                    message.Body = textMsg;
                    message.Formatter = MSMQHelper.MSMQFormatter(qConfig.MSMQFormat);

                    queue.Path = queuePath;
                    long amount = qConfig.Amount;
                    Parallel.For(0, amount, (i, loopState) =>
                    {
                        queue.Send(message);
                    });
                }
            }
        }

        private void SendByTransaction(string textMsg)
        {
            using (MessageQueueTransaction trans = new MessageQueueTransaction())
            {
                trans.Begin();
                //using (MessageQueue queue = new MessageQueue())
                queue = new MessageQueue();
                {
                    //using (Message message = new Message())
                    message = new Message();
                    {
                        message.Label = qConfig.Label;
                        message.Body = textMsg;
                        message.Formatter = MSMQHelper.MSMQFormatter(qConfig.MSMQFormat);

                        queue.Path = queuePath;
                        long amount = qConfig.Amount;
                        Parallel.For(0, amount, (i, loopState) =>
                        {
                            queue.Send(message, trans);
                            trans.Commit();
                        });
                    }
                }
            }
        }

        public void Dispose()
        {
            //channel.QueueDelete(queueUrl.QueueName);
            running = false;
            if (message != null)
            {
                //channel.Abort();
                message.Dispose();
                message = null;
            }
            if (queue != null)
            {
                //connection.Abort();
                queue.Close();
                queue.Dispose();
                queue = null;
            }
        }



    }
}
