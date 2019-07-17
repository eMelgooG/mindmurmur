using System;

namespace MindMurmur.Lights.Monitors
{
    public class HeartRateMonitor : IObserver<int>
    {
        DMX dmx = null;
        private IDisposable unsubscriber;
        private bool first = true;
        private int last;
        public HeartRateMonitor(DMX dmx)
        {
            this.dmx = dmx;
        }

        public virtual void Subscribe(IObservable<int> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional heart rates will not be processed.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(int value)
        {
            if (first)
            {
                Console.WriteLine("First heart beat registered @ {0}", value);
                last = value;
                first = false;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Current bpm is {0}, a change from {1}", value, last);
                last = value;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
