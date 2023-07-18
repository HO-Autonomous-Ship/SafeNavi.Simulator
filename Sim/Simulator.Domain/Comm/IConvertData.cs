using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public interface IConvertData
    {
        void Serialize(ref byte[] bytes);

        void Deserialize(byte[] bytes, ref int offset);
    }
}
