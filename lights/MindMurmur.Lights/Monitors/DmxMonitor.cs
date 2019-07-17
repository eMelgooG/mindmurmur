using System;
using MindMurmur.Domain.Light;

namespace MindMurmur.Lights.Monitors
{

    public class DmxMonitor : IObserver<DmxData>
    {
        DMX dmx = null;
        private IDisposable unsubscriber;
        private bool first = true;
        private DmxData last;
        public DmxMonitor(DMX dmx)
        {
            this.dmx = dmx;
        }

        public virtual void Subscribe(IObservable<DmxData> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional DMXData packets will not be processed.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(DmxData value)
        {
            if (first)
            {
                last = value;
                first = false;
            }
            else
            {
                last = value;
            }
        }
    }
}
