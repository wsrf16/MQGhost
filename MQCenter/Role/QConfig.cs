using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector.Role
{
    public class QConfig
    {
        public string IP
        {
            get;
            set;
        }
        public int Port
        {
            get;
            set;
        }
        public string Label
        {
            get;
            set;
        }

        public long ConfirmPublishTimeout
        {
            get;
            set;
        }

        public string QueueName
        {
            get;
            set;
        }

        public string RoutingKey
        {
            get;
            set;
        }

        public string Exchange
        {
            get;
            set;
        }

        public string QueueNameReply
        {
            get
            {
                return QueueName + Constant.ReplyPostfix;
            }
        }

        public string Password
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        //public string HostName
        //{
        //    get;
        //    set;
        //}

        public long Amount
        {
            get;
            set;
        }

        public bool RequiredReply
        {
            get;
            set;
        }

        public bool IsTransaction
        {
            get;
            set;
        }        

        public PersistentSet PersistentMode
        {
            get;
            set;
        }

        public PersistentSet PersistentModeReply
        {
            get;
            set;
        }

        public bool CreateNewQueue
        {
            get;
            set;
        }

        public Action<string> Callback
        {
            get;
            set;
        }

        public MQTypeEnum MQType
        {
            get;
            set;
        }

        public MSMQFormatEnum MSMQFormat
        {
            get;
            set;
        }

        public enum PersistentSet
        {
            NonPersistent = 0,
            Persistent = 1,
        }
    }
}
