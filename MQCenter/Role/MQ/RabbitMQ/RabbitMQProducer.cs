using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;
using RabbitMQ.Client.Events;
using Vector.Role.Interface;

namespace Vector.Role.MQ.RabbitMQ
{
    class RabbitMQProducer : IProducer
    {
        ConnectionFactory factory;
        IConnection connection;
        IModel channel;
        delegate void ReceiveCallback(string msg);
        event ReceiveCallback callback;
        QConfig qConfig;
        bool running = true;

        public RabbitMQProducer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            factory = new ConnectionFactory();
            factory.HostName = qConfig.IP;
            factory.Port = qConfig.Port != 0 ? qConfig.Port : AmqpTcpEndpoint.UseDefaultPort;
            factory.Protocol = Protocols.DefaultProtocol;
            factory.UserName = string.IsNullOrWhiteSpace(qConfig.UserName) ? RabiitMQConstant.DEFAULTUSERNAME : qConfig.UserName;
            factory.Password = string.IsNullOrWhiteSpace(qConfig.Password) ? RabiitMQConstant.DEFAULTPASSWORD : qConfig.Password;
            qConfig.PersistentModeReply = QConfig.PersistentSet.Persistent;
            if (qConfig.Callback != null)
                callback = new ReceiveCallback(qConfig.Callback);
        }

        public void SendTextMsg(string textMsg)
        {
            IList<string> textMsgs = new List<string>();
            textMsgs.Add(textMsg);
            SendTextMsg(textMsgs);
        }

        public void SendTextMsg(IList<string> textMsgs)
        {
            //创建一个 AMQP 连接
            //using (IConnection connection = factory.CreateConnection())
            connection = factory.CreateConnection();
            {
                //using (IModel channel = connection.CreateModel())
                channel = connection.CreateModel();
                {
                    //在MQ上定义一个队列
                    declareQueue(channel, qConfig);

                    string replyQueue = BeginReplyConsume(channel, qConfig, consumer_Received);
                    IBasicProperties basicProperties = BuildBasicProperties(channel, qConfig.PersistentMode, replyQueue);

                    //序列化消息对象，RabbitMQ并不支持复杂对象的序列化，所以对于自定义的类型需要自己序列化
                    //指定发送的路由，通过默认的exchange直接发送到指定的队列中。
                    try
                    {
                        string _exchange, _routingKey;
                        getExchangeAndRoutingKey(qConfig, out _exchange, out _routingKey);
                        publish(channel, qConfig, textMsgs, basicProperties, _exchange, _routingKey);
                        confirm(channel, qConfig);
                    }
                    catch (Exception ex)
                    {
                        running = false;
                        if (ex.HResult != -2147024858)
                            throw ex;
                    }
                    finally
                    {
                        if (!qConfig.RequiredReply)
                            Dispose();
                    }
                }
            }
        }

        private static void confirm(IModel channel, QConfig qConfig)
        {
            //if (qConfig.ConfirmPublishTimeout != 0)
            switch (qConfig.ConfirmPublishTimeout)
            {
                //一直等待
                case -1:
                    {
                        channel.ConfirmSelect();
                        channel.WaitForConfirmsOrDie();
                    }
                    break;
                //不等待
                case 0:
                    //channel.WaitForConfirmsOrDie(new System.TimeSpan(qConfig.ConfirmPublishTimeout));
                    break;
                //等待
                default:
                    if (qConfig.ConfirmPublishTimeout > 0)
                    {
                        channel.ConfirmSelect();
                        channel.WaitForConfirmsOrDie(new System.TimeSpan(qConfig.ConfirmPublishTimeout));
                    }
                    break;
            }
        }

        private static void publish(IModel channel, QConfig qConfig, IList<string> textMsgs, IBasicProperties basicProperties, string _exchange, string _routingKey)
        {
            long amount = qConfig.Amount;
            if (amount > 1)
            {
                Parallel.For(0, amount, (i, loopState) =>
                {
                    //无序方式
                    //Parallel.ForEach(textMsgs, textMsg =>
                    //有序方式
                    textMsgs.ToList().ForEach(textMsg =>
                    {
                        channel.BasicPublish(_exchange, _routingKey, basicProperties, Encoding.UTF8.GetBytes(textMsg));
                    });
                });
            }
            else
            {
                //有序方式
                textMsgs.ToList().ForEach(textMsg =>
                {
                    channel.BasicPublish(_exchange, _routingKey, basicProperties, Encoding.UTF8.GetBytes(textMsg));
                });
            }
        }

        private static void getExchangeAndRoutingKey(QConfig qConfig, out string _exchange, out string _routingKey)
        {
            if (!string.IsNullOrWhiteSpace(qConfig.Exchange) && !string.IsNullOrWhiteSpace(qConfig.RoutingKey))
            {
                _exchange = qConfig.Exchange;
                _routingKey = qConfig.RoutingKey;
            }
            else
            {
                _exchange = string.Empty;
                _routingKey = qConfig.QueueName;
            }
        }

        private static void declareQueue(IModel channel, QConfig qConfig)
        {
            bool persistB = qConfig.PersistentMode == QConfig.PersistentSet.Persistent ? true : false;
            if (qConfig.CreateNewQueue)
                channel.QueueDeclare(qConfig.QueueName, persistB, false, false, null);
        }

        private static string BeginReplyConsume(IModel channel, QConfig qConfig, EventHandler<BasicDeliverEventArgs> received)
        {
            string replyQueue;
            if (qConfig.RequiredReply)
            {
                bool persistReplyB = qConfig.PersistentModeReply == QConfig.PersistentSet.Persistent ? true : false;
                replyQueue = channel.QueueDeclare(qConfig.QueueNameReply, persistReplyB, false, false, null).QueueName;
                ReplyConsume(channel, replyQueue, received);
            }
            else
            {
                replyQueue = null;
            }
            return replyQueue;
        }

        private static void ReplyConsume(IModel channel, string replyQueue, EventHandler<BasicDeliverEventArgs> received)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += received;
            channel.BasicConsume(replyQueue, true, consumer);
        }

        void consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            if (callback != null && qConfig.RequiredReply)
            {
                string text = Encoding.UTF8.GetString(e.Body);
                string correlationId = e.BasicProperties.CorrelationId;
                Dispose();
                callback(string.Format("CorrelationId:{0},Text:{1}", correlationId, text));
            }
            else if (callback != null && !qConfig.RequiredReply)
            {
                Dispose();
                callback(string.Format("Success!"));
            }
            else
            {
                Dispose();
            }
        }

        private static IBasicProperties BuildBasicProperties(IModel channel, QConfig.PersistentSet persistentMode, string replyQueue = null)
        {
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            //1为默认非持久化，2为持久化persistent
            switch (persistentMode)
            {
                case QConfig.PersistentSet.NonPersistent:
                    basicProperties.DeliveryMode = 1;
                    break;
                case QConfig.PersistentSet.Persistent:
                    basicProperties.DeliveryMode = 2;
                    break;
            }
            basicProperties.CorrelationId = Guid.NewGuid().ToString();
            if (replyQueue != null)
                basicProperties.ReplyTo = replyQueue;
            return basicProperties;
        }

        public void Dispose()
        {
            //channel.QueueDelete(queueUrl.QueueName);
            running = false;
            if (channel != null && channel.IsOpen)
            {
                //channel.Abort();
                channel.Close();
                channel.Dispose();
                channel = null;
            }
            if (connection != null && connection.IsOpen)
            {
                //connection.Abort();
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }



    }
}
