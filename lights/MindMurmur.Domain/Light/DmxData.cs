using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.Light
{
    public class DmxData
    {
        public short Channel { get; set; }
        public byte Value { get; set; }

        public DmxData(short channel, byte value)
        {
            Channel = channel;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Channel: {0} Data: {1}", Channel, Value);
        }
    }
}
