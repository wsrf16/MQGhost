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
    class RabbitMQConsumer : IConsumer
    {
        ConnectionFactory factory;
        IConnection connection;
        IModel channel;
        static IConnection connection_pop;
        static IModel channel_pop;
        delegate void ReceiveCallback(string msg);
        event ReceiveCallback callback;
        QConfig qConfig;
        bool running = true;

        public RabbitMQConsumer(QConfig qConfig)
        {
            this.qConfig = qConfig;
            factory = new ConnectionFactory();
            factory.HostName = qConfig.IP;
            factory.Port = qConfig.Port != 0 ? qConfig.Port : AmqpTcpEndpoint.UseDefaultPort;
            factory.Protocol = Protocols.DefaultProtocol;
            factory.UserName = string.IsNullOrWhiteSpace(qConfig.UserName) ? RabiitMQConstant.DEFAULTUSERNAME : qConfig.UserName;
            factory.Password = string.IsNullOrWhiteSpace(qConfig.Password) ? RabiitMQConstant.DEFAULTPASSWORD : qConfig.Password;
            if (qConfig.Callback != null)
                callback = new ReceiveCallback(qConfig.Callback);
        }

        public void Tap()
        {
            using (IConnection connection_Tap = factory.CreateConnection())
            {
                bool b = connection_Tap.IsOpen;
            }
        }

        public void Pop()
        {
            //创建一个 AMQP 连接
            connection_pop = factory.CreateConnection();
            {
                channel_pop = connection_pop.CreateModel();
                {
                    //在MQ上定义一个队列
                    bool persistB = qConfig.PersistentMode == QConfig.PersistentSet.Persistent ? true : false;
                    if (qConfig.CreateNewQueue)
                        channel.QueueDeclare(qConfig.QueueName, persistB, false, false, null);

                    //在队列上定义一个消费者
                    QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel_pop);
                    channel_pop.QueueBind(qConfig.QueueName, "exchange", "key");
                    channel_pop.BasicConsume(qConfig.QueueName, false, consumer);       //此处是一次不把所有消息都取出
                    new System.Threading.Tasks.Task(() =>
                    {
                        try
                        {
                            //阻塞函数，获取队列中的消息
                            //BasicDeliverEventArgs args = consumer.Queue.Dequeue();
                            BasicDeliverEventArgs args = null;
                            bool b = consumer.Queue.Dequeue(500, out args);
                            if (b)
                                consumer_Listener_pop(args);
                        }
                        catch (Exception ex)
                        {
                            running = false;
                            if (ex.HResult != -2147024858)
                                throw ex;
                        }
                            finally
                            {
                                Dispose();
                            }
                    }).Start();
                }
            }
        }

        public void Listen()
        {
            //创建一个 AMQP 连接
            connection = factory.CreateConnection();
            {
                channel = connection.CreateModel();
                {
                    //在MQ上定义一个队列
                    bool persistB = qConfig.PersistentMode == QConfig.PersistentSet.Persistent ? true : false;
                    //channel.QueueDeclare(queueUrl.QueueName, persistB, false, false, null);

                    //在队列上定义一个消费者
                    //QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);
                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                    consumer.Received += consumer_Listener;
                    channel.BasicConsume(qConfig.QueueName, false, consumer);
                }
            }
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

            if (channel_pop != null && channel_pop.IsOpen)
            {
                //channel_pop.Abort();
                channel_pop.Close();
                channel_pop.Dispose();
                channel_pop = null;
            }
            //RabbitMQ的连接使用的是单例，上面已经释放，所以不需要再次释放
            if (connection_pop != null && connection_pop.IsOpen)
            {
                //connection_pop.Abort();
                connection_pop.Close();
                connection_pop.Dispose();
                connection_pop = null;
            }
        }


        void consumer_Listener_pop(BasicDeliverEventArgs args)
        {
            consumer_Listener(args, channel_pop);
        }

        void consumer_Listener(object sender, BasicDeliverEventArgs e)
        {
            consumer_Listener(e, this.channel);
        }

        void consumer_Listener(BasicDeliverEventArgs args, IModel channelBack)
        {
            channelBack.BasicAck(args.DeliveryTag, false);          //此处可进行手动应答
            IBasicProperties basicProperties = channelBack.CreateBasicProperties();
            basicProperties.CorrelationId = args.BasicProperties.CorrelationId;

            string msg = Encoding.UTF8.GetString(args.Body);
            if (callback != null)
                callback(msg);
            if (qConfig.RequiredReply)
                channelBack.BasicPublish("", args.BasicProperties.ReplyTo, basicProperties, Encoding.UTF8.GetBytes(msg));
        }
    }


}
