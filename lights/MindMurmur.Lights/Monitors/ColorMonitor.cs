using System;
using System.Drawing;

namespace MindMurmur.Lights.Monitors
{

    public class ColorMonitor : IObserver<Color>
    {
        DMX dmx = null;
        private IDisposable unsubscriber;
        private bool first = true;
        private Color last;
        public ColorMonitor(DMX dmx)
        {
            this.dmx = dmx;
        }

        public virtual void Subscribe(IObservable<Color> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional colors will not be processed.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(Color value)
        {
            Console.WriteLine("Current color is {0}", value);
            if (first)
            {
                last = value;
                first = false;
            }
            else
            {
                Console.WriteLine("Current color is {0}, a change from {1}", value, last);
                last = value;
            }
        }
    }
}
