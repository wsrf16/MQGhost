using Vector.Role.Interface;
using Vector.Role.MQ.ActiveMQ;
using Vector.Role.MQ.MSMQ;
using Vector.Role.MQ.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector.Role
{
    public enum MQTypeEnum
    {
        MSMQ = 1,
        ActiveMQ = 2,
        RabbitMQ = 3
    }

    public enum RoleType
    {
        Producer = 1,
        Consumer = 2,
    }

    public enum MSMQFormatEnum
    {
        Active = 1,
        Binary = 2,
        Xml = 3
    }

    public class MQFactory
    {
        public static IProducer GenerateProducer(QConfig queueUrl)
        {
            IProducer producer;
            switch (queueUrl.MQType)
            {
                case MQTypeEnum.MSMQ:
                    producer = new MSMQProducer(queueUrl);
                    break;
                case MQTypeEnum.ActiveMQ:
                    producer = new ActiveMQProducer(queueUrl);
                    break;
                case MQTypeEnum.RabbitMQ:
                    producer = new RabbitMQProducer(queueUrl);
                    break;
                default:
                    producer = null;
                    break;
            }
            return producer;
        }

        public static IConsumer GenerateConsumer(QConfig queueUrl)
        {
            IConsumer consumer;
            switch (queueUrl.MQType)
            {
                case MQTypeEnum.MSMQ:
                    consumer = new MSMQConsumer(queueUrl);
                    break;
                case MQTypeEnum.ActiveMQ:
                    consumer = new ActiveMQConsumer(queueUrl);
                    break;
                case MQTypeEnum.RabbitMQ:
                    consumer = new RabbitMQConsumer(queueUrl);
                    break;
                default:
                    consumer = null;
                    break;
            }
            return consumer;
        }
    }
}
