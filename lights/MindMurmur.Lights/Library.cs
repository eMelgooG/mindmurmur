using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MindMurmur.Lights
{
    public class Library : IDisposable
    {
        internal DMX DMX { get; set; }

        private  Timer MainTimer;
        

        public static async Task<Library> Create()
        {
            var lib = new Library();
            try
            {
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Starting DMX");
                lib.DMX = new DMX();
                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] DMX Started");
            }
            catch (Exception LibraryException)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[!] Error during Library .ctor");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine(LibraryException.ToString());
            }
            return lib;
        }

        async public Task Start()
        {
            MainTimer = new Timer(MainTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);

            // Start DMX
            if (DMX != null)
                await DMX.Connect();

            MainTimer.Change(TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(40));
        }

        public void Stop()
        {
            MainTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        async private void MainTimer_Tick(object State)
        {
            MainTimer.Change(Timeout.Infinite, Timeout.Infinite);
            /// LIGHTS
            /// Run the visualization
            /// And send the colors to DMX. (unless stopped)
            try
            {
                //await MidnightStarScene.SetLightsFromVisualization();
                if (this.DMX.IsConnected)
                {
                    byte[] dmx = this.DMX.GetDMXFromColors(MidnightStarScene.Poles.Select(pole => pole.LEDS.First().Color));
                    DMX.SendDMXFrames(dmx);
                }
            }
            catch (Exception TimerException)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[!] Error during MainTimer");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine(TimerException.ToString());
            }
            MainTimer.Change(40, 40);
        }
        
        public void Dispose()
        {
            if (DMX != null)
                DMX.Dispose();
            this.MainTimer.Dispose();
        }
    }
}
