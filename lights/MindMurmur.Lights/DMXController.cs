using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using MindMurmur.Domain.Light;

namespace MindMurmur.Lights
{
    public class DMXController : IDisposable
    {

        #region Constants

        const string VIRTUALFILENAME = "ShareK8062Data";
        const int DATASIZE = 2500;
        /// <summary>
        /// extra offset for production environment
        /// </summary>
        const int CHANNEL_OFFSET = 7;
        /// <summary>
        /// built-in value from DMX specificationz
        /// </summary>
        const int CHANNEL_INTERNAL_OFFSET = 3;

        #endregion

        #region Fields

        readonly MemoryMappedFile memoryMappedFile;
        readonly MemoryMappedViewAccessor accessor;
        Process process;

        #endregion

        #region Properties

        public bool IsConnected
        {
            get
            {
                return GetShareData(0) == 123;
            }
        }

        #endregion

        #region Constructor and Dispose

        public DMXController()
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen(VIRTUALFILENAME, DATASIZE, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, null, System.IO.HandleInheritability.Inheritable);
            accessor = memoryMappedFile.CreateViewAccessor(0, DATASIZE);
        }

        public void Dispose()
        {
            accessor.Dispose();
            memoryMappedFile.Dispose();

            // If process is still alive, kill it
            if (process != null && !process.HasExited)
                process.Kill();
        }

        public void SetData(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length > 512)
                throw new ArgumentOutOfRangeException("Can't send more data then 512 values");

            for (short i = 0; i < data.Length; i++)
                SetChannel(Convert.ToInt16(i + 1), data[i]);
        }

        #endregion

        #region Public Methods

        public void StartDevice()
        {
            SetShareData(1, 222);
            process = Process.Start("K8062e.exe");
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[-] K8062e.exe started successfully. pid = " + process.Handle.ToString());
        }

        public void StopDevice()
        {
            SetShareData(0, 333);
        }

        public void SetChannel(short channel, byte value)
        {
            if (channel <= 0 || channel > 512)
                throw new ArgumentOutOfRangeException("Parameter 'channel' must be greater then 0 and less then or equal to 512");
            SetShareData(channel + CHANNEL_OFFSET + CHANNEL_INTERNAL_OFFSET, (int)value);
        }

        public void SetChannelCount(short channels)
        {
            SetShareData(2, (int)channels);
        }

        #endregion

        #region Private methods to read and write the data

        byte[] ReadAllData()
        {
            byte[] buffer = new byte[DATASIZE];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = accessor.ReadByte(i);
            return buffer;
        }

        void SetShareData(int address, int data)
        {
            address *= sizeof(int);
            accessor.Write(address, data);
        }

        int GetShareData(int address)
        {
            address *= sizeof(int);
            return accessor.ReadInt32(address);
        }

        #endregion

    }
}
