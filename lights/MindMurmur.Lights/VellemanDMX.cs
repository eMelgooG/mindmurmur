using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MindMurmur.Lights
{
    public class VellemanDMX : IDmx
    {
        [DllImport("K8062D.dll")]
        public extern static int StartDevice();

        [DllImport("K8062D.dll")]
        public extern static void SetData(int Channel, int Data);

        [DllImport("K8062D.dll")]
        public extern static void StopDevice();

        [DllImport("K8062D.dll")]
        public extern static void SetChannelCount(int Count);
        
        public void setDmxValue(int channel, byte value)
        {
            VellemanDMX.SetData(channel, value);
        }
        public void startWriteThread()
        {
        }
    }

    public interface IDmx
    {
        void setDmxValue(int channel, byte value);
        void startWriteThread();
    }
 }
