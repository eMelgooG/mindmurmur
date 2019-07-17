using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MindMurmur.Domain.HeartMonitor
{
    public class HRMStatus
    {
        public int? HeartRate { get; private set; }
        public int? MinHeartRate { get; private set; }
        public int? MaxHeartRate { get; private set; }
        public int TotalPackets { get; private set; }

        public IHRMPacket HRMPacket { get; private set; }

        public HRMStatus(HeartRateMonitor hrm, IHRMPacket hrmPacket)
        {
            HeartRate = hrm.LastPacket.HeartRate;
            MinHeartRate = hrm.MinHeartRate;
            MaxHeartRate = hrm.MaxHeartRate;
            TotalPackets = hrm.TotalPackets;
            HRMPacket = hrmPacket;
        }

        public override string ToString()
        {
            return string.Format("HeartRate={0}, MinHeartRate={1}, MinHeartRate={2}, MinHeartRate={3} ", HeartRate, MinHeartRate, MaxHeartRate, TotalPackets);
        }
    }
}
