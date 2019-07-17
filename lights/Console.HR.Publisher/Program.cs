using EasyNetQ;
using log4net;
using MindMurmur.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Console.HR.Publisher
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var test = new RabbitTests();
            test.RunTestsForever();


            //HRMEmulator hrm = new HRMEmulator();
            //hrm.Start();
        }
    }

    public class RabbitTests {

        IBus bus = RabbitHutch.CreateBus("host=localhost");

        public void RunTests()
        {
            var random = new Random();

            for (int i = 0; i < 5; i++)
            {
                var colorCmd = new ColorControlCommand() { CommandId = Guid.NewGuid().ToString(), ColorBlue = Convert.ToInt16(random.Next(1, 210)), ColorGreen = Convert.ToInt16(random.Next(1, 210)), ColorRed = Convert.ToInt16(random.Next(1, 210)) };
                var heartCmd = new HeartRateCommand() {CommandId = Guid.NewGuid().ToString(), HeartRate= Convert.ToInt16(random.Next(60, 170)) };

                bus.Publish(colorCmd);
                bus.Publish(heartCmd);
            }

        }


        public void RunTestsForever()
        {
            var random = new Random();

            while(true)
            {
                var colorCmd = new ColorControlCommand() { CommandId = Guid.NewGuid().ToString(), ColorBlue = Convert.ToInt16(random.Next(1, 210)), ColorGreen = Convert.ToInt16(random.Next(1, 210)), ColorRed = Convert.ToInt16(random.Next(1, 210)) };
                var heartCmd = new HeartRateCommand() { CommandId = Guid.NewGuid().ToString(), HeartRate = Convert.ToInt16(random.Next(60, 170)) };

                bus.Publish(colorCmd);
                bus.Publish(heartCmd);

                Thread.Sleep(50);
            }

        }

    }

}
