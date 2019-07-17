using System;
using System.Collections.Generic;

namespace MindMurmur.Lights
{
    public class HeartRateObservable : IObservable<int>
    {
        List<IObserver<int>> observers;
        DMX dmx = null;

        public HeartRateObservable(DMX dmx)
        {
            this.dmx = dmx;
            observers = new List<IObserver<int>>();
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);

            return new Unsubscriber(observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<int>> _observers;
            private IObserver<int> _observer;

            public Unsubscriber(List<IObserver<int>> observers, IObserver<int> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (!(_observer == null)) _observers.Remove(_observer);
            }
        }
    }
}
