using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector.Role.Interface
{
    public interface IProducer
    {
        void SendTextMsg(string msgText);
        void Dispose();
    }
}
