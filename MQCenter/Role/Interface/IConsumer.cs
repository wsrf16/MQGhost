using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vector.Role.Interface
{
    public interface IConsumer
    {
        void Tap();
        void Pop();
        void Listen();
        void Dispose();
    }
}
