
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Timers;
using EasyNetQ;
using MindMurmur.Domain.HeartMonitor;
using log4net;

namespace Console.HR.Publisher
{
    public sealed class HRMEmulator : HeartRateMonitor
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override string Name { get { return "Emulator"; } }
        Timer timer;
        Random random = new Random();
        private int maxSlope = 1;
        IBus bus = RabbitHutch.CreateBus("host=localhost");

        private int slope = 0;
        private int bpm;
        private double heartBeats = 0;

        HRMPacket lastPacket;

        private int MIN_BPM = 40;
        private int MAX_BPM = 190;
        
        public delegate void EmulatorMinBPMChangedEventHandler(object sender, int minBPM);
        public event EmulatorMinBPMChangedEventHandler EmulatorMinBPMChanged;
        public delegate void EmulatorMaxBPMChangedEventHandler(object sender, int maxBPM);
        public event EmulatorMaxBPMChangedEventHandler EmulatorMaxBPMChanged;

        public int MinBPM
        {
            get
            {
                return MIN_BPM;
            }
            set
            {
                int bck = MIN_BPM;

                if (value > MaxBPM)
                    MaxBPM = value;

                MIN_BPM = value;
                if (bck != value)
                    EmulatorMinBPMChanged?.Invoke(this, value);
            }
        }
        
        public int MaxBPM
        {
            get
            {
                return MAX_BPM;
            }
            set
            {
                int bck = MAX_BPM;

                if (value < MinBPM)
                    MinBPM = value;

                MAX_BPM = value;
                if (bck != value)
                    EmulatorMaxBPMChanged?.Invoke(this, value);
            }
        }
        public HRMEmulator()
        {
            Running = false;
            TotalPackets = 0;
            HeartBeats = 0;
            bpm = random.Next(55, 81);
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ThumpThump();
        }

        private void ThumpThump()
        {
            lastPacket = new HRMPacket(bpm);

            if (MinHeartRate == null)
                MinHeartRate = (byte)bpm;
            else
                if (bpm < MinHeartRate)
                MinHeartRate = (byte)bpm;

            if (MaxHeartRate == null)
                MaxHeartRate = (byte)bpm;
            else
               if (bpm > MaxHeartRate)
                MaxHeartRate = (byte)bpm;

            if (bpm >= 180)
                maxSlope = 3;
            else if (bpm >= 120)
                maxSlope = 2;
            else
                maxSlope = 1;

            int slopeVar = random.Next(-1, 2);
            slope += slopeVar;
            if (slope > maxSlope)
                slope = maxSlope;
            else if (slope < -maxSlope)
                slope = -maxSlope;
            bpm += slope;

            if (bpm > MAX_BPM)
            {
                bpm = MAX_BPM;
                slope = 0;
            }
            else if (bpm < MIN_BPM)
            {
                bpm = MIN_BPM;
                slope = 0;
            }

            TotalPackets++;
            heartBeats += bpm / 60D;
            HeartBeats = (int)heartBeats;
            ProcessPacket(lastPacket);

#if DEBUG
            logger.Debug("Firing PacketProcessed event, packet = " + lastPacket);
#endif
            base.FirePacketProcessed(new PacketProcessedEventArgs(lastPacket));
        }

        private Subject<HRMPacket> packetCreated = new Subject<HRMPacket>();

        public IObservable<HRMPacket> PacketCreated
        {
            get { return this.packetCreated; }
        }

        public void ProcessPacket(HRMPacket packet)
        {
            try
            {
                lastPacket = packet;
                bus.Publish(packet);
                logger.Debug(string.Format("published message {0}", packet.ToString()));
                System.Console.WriteLine(string.Format("published message {0}", packet.ToString()));

                var status = new HRMStatus(this, packet);
                bus.Publish(status);
                logger.Debug(string.Format("published message {0}", status.ToString()));
                System.Console.WriteLine(string.Format("published message {0}", status.ToString()));
                
                this.packetCreated.OnNext(packet);
            }
            catch (Exception exception)
            {
                this.packetCreated.OnError(exception);
            }
        }

        public override IHRMPacket LastPacket
        {
            get
            {
                return lastPacket;
            }
            protected set
            {
                lastPacket = (HRMPacket) value;
            }
        }

        public override int TotalPackets { get; protected set; }
        public override int CorruptedPackets { get; protected set; }
        public override int HeartBeats { get; protected set; }
        public override byte? MinHeartRate { get; protected set;}
        public override byte? MaxHeartRate { get; protected set; }

        public override int HeartRateSmoothingFactor { get; set; }

        public override double SmoothedHeartRate
        {
            get 
            {
                return 60;
            }
            protected set 
            {
                return;
            }
        }

        public override bool Running { get; protected set; }
 
        public override void Start()
        {
            timer.Start();
            Running = true;
        }

        public override void Stop()
        {
            timer.Stop();
            Running = false;
        }

        public override void Reset()
        {
            MinHeartRate = null;
            MaxHeartRate = null;
        }

        public override void Dispose()
        {
            timer.Dispose();
        }

        public void StartTimer()
        {
            System.Reactive.Linq.Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(
                    x =>
                    {
                        ThumpThump();
                        logger.Debug("Thump...");
            });
        }
        
    }
}
