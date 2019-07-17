using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MindMurmur.Domain.HeartMonitor
{
    public interface IHRMPacket
    {
        int HeartRate { get; }
    }
}
