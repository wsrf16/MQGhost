using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vector.Role.MQ.MSMQ
{
    class MSMQHelper
    {
        public static IMessageFormatter MSMQFormatter(MSMQFormatEnum format)
        {
            IMessageFormatter formatter;
            switch (format)
            {
                case MSMQFormatEnum.Active:
                    formatter = new ActiveXMessageFormatter();
                    break;
                case MSMQFormatEnum.Binary:
                    formatter = new BinaryMessageFormatter();
                    break;
                case MSMQFormatEnum.Xml:
                    formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                    break;
                default:
                    formatter = new ActiveXMessageFormatter();
                    break;
            }
            return formatter;
        }

        public static string MakeMQPath(string queuePath)
        {
            string mqPath;
            const string ipReg = "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}";
            const string portReg = "\\d{1,5}";

            if (Regex.IsMatch(queuePath, "^FormatName:", RegexOptions.IgnoreCase))
                mqPath = queuePath;
            else
            {
                if (Regex.IsMatch(queuePath, string.Format("^{0}(:{1})?", ipReg, portReg)))
                    mqPath = string.Format(@"FormatName:direct=tcp:{0}", queuePath);
                else if (Regex.IsMatch(queuePath, "^\\."))
                    mqPath = string.Format(@"{0}", queuePath);
                else
                    mqPath = string.Format(@"FormatName:direct=os:{0}", queuePath);
            }
            return mqPath;
        }
    }
}
