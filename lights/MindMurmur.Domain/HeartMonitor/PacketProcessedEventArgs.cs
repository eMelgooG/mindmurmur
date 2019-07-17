using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MindMurmur.Domain.HeartMonitor
{
    public class PacketProcessedEventArgs : EventArgs
    {
        public IHRMPacket HRMPacket;

        public PacketProcessedEventArgs(IHRMPacket hrmPacket)
        {
            this.HRMPacket = hrmPacket;
        }
    }
}
