
using System;

namespace MindMurmur.Domain.HeartMonitor
{
    public abstract class HeartRateMonitor : IDisposable
    {
        public abstract string Name { get; }

        public delegate void PacketProcessedHandler(object sender, PacketProcessedEventArgs e);
        public event PacketProcessedHandler PacketProcessed;

        protected virtual void FirePacketProcessed(PacketProcessedEventArgs e)
        {
            PacketProcessedHandler handler = PacketProcessed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Data
        public abstract IHRMPacket LastPacket { get; protected set; }
        public abstract int TotalPackets { get; protected set; }
        public abstract int CorruptedPackets { get; protected set; }

        public abstract int HeartBeats { get; protected set; }

        public abstract byte? MinHeartRate { get; protected set; }
        public abstract byte? MaxHeartRate { get; protected set; }

        public abstract int HeartRateSmoothingFactor { get; set; }
        public abstract double SmoothedHeartRate { get; protected set; }
        
        // Commands
        public abstract bool Running { get; protected set; }
        public abstract void Start();
        public abstract void Stop();
        public abstract void Reset();

        public abstract void Dispose();
    }
}
