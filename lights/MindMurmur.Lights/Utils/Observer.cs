using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Lights.Utils
{
    public static partial class ObservableEx
    {
        public static IObservable<T> FromValues<T>(params T[] values)
        {
            return values.ToObservable();
        }
    }
}
