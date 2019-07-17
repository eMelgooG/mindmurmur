using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MindMurmur.Domain.HeartMonitor
{
    public class HRMPacket : IHRMPacket
    {
        private int heartRate;

        public HRMPacket(int heartRate)
        {
            this.heartRate = heartRate;
        }

        public int HeartRate
        {
            get { return heartRate; }
        }

        public override string ToString()
        {
            return base.ToString() + "[ HeartRate = " + HeartRate + " ]";
        }

    }
}
