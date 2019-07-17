using MindMurmur.Domain.HeartMonitor;
using System;

namespace Console.HR.Publisher
{
   public class PacketControl : IDisposable
    {
        private IDisposable packetCreatedSubscription;

        public PacketControl(HRMEmulator emulator)
        {
            packetCreatedSubscription = emulator.PacketCreated.Subscribe(this.OnPacketCreated);
        }

        public void Dispose()
        {
            this.packetCreatedSubscription.Dispose();
        }

        private void OnPacketCreated(HRMPacket packet)
        {
            HRMPacket spottedPacket = packet;
        }
    }
}
