using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Messaging;
using Vector.Role.Interface;

namespace Vector.Role.MQ.MSMQ
{
    class MSMQConsumer : IConsumer
    {
        delegate void ReceiveCallback(string msg);
        event ReceiveCallback callback;
        QConfig qConfig;
        string queuePath;
        MessageQueue queue;

        public MSMQConsumer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            string _ip = string.IsNullOrWhiteSpace(qConfig.IP) ? MSMQConstant.DEFAULTIP : qConfig.IP;
            string _port = qConfig.Port == 0 ? string.Empty : (":" + qConfig.Port);
            queuePath = (_ip + _port + qConfig.QueueName).Trim();
            if (qConfig.Callback != null)
                callback = new ReceiveCallback(qConfig.Callback);
            //QueuePath = ".\\private$\\globalprocess_6";
        }


        public void Tap()
        {
            using (MessageQueue queue = new MessageQueue(queuePath))
            {
                Message message = queue.Peek();
                message.Formatter = MSMQHelper.MSMQFormatter(qConfig.MSMQFormat);
            }

        }

        public void Listen()
        {
            queue = new MessageQueue(queuePath);
            queue.ReceiveCompleted += queue_ReceiveCompleted;
            queue.BeginReceive();
        }
        void queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            Message message = ((ReceiveCompletedEventArgs)e).Message;
            consumer_Listener(message);
        }

        public void Dispose()
        {
            if (queue != null)
            {
                queue.Close();
                queue.Dispose();
            }
        }

        void consumer_Listener(Message message)
        {
            message.Formatter = MSMQHelper.MSMQFormatter(qConfig.MSMQFormat);
            if (callback != null)
                callback(message.Body.ToString());
            queue.BeginReceive();
            //Console.WriteLine("Receive: " + msg.Text);
        }

        public void Pop()
        {
            //
        }

    }


}
