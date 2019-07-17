using EasyNetQ;
using EasyNetQ.Rx;
using log4net;
using MindMurmur.Domain.HeartMonitor;
using MindMurmur.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console.HR.Subscriber
{

    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static IBus bus = RabbitHutch.CreateBus("host=localhost");

        static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                bus.Subscribe<ColorControlCommand>("Commands.ColorControl", HandleColorControlCommand);
                bus.Subscribe<MeditationStateCommand>("Commands.MeditationState", MeditationStateCommand);
                bus.Subscribe<HeartRateCommand>("Commands.HeartRate", HandleHeartRateCommand);

                System.Console.WriteLine("Listening for messages. Hit <return> to quit.");
                System.Console.ReadLine();
            }
        }

        private static void MeditationStateCommand(MeditationStateCommand obj)
        {
            throw new NotImplementedException();
        }
        private static void HandleColorControlCommand(ColorControlCommand obj)
        {
            throw new NotImplementedException();
        }

        private static void HandleHeartRateCommand(HeartRateCommand obj)
        {
            throw new NotImplementedException();
        }

        private static void HandleStatusMessage(HRMStatus obj)
        {
            throw new NotImplementedException();
        }

        private static void HandlePacketMessage(HRMPacket obj)
        {
            throw new NotImplementedException();
        }
    }
}
