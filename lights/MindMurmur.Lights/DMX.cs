using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MindMurmur.Domain.Light;
using MindMurmur.Lights.Control;

namespace MindMurmur.Lights
{
    public class DMX : IDisposable
    {
        private DMXController DMXController;
        private short maxChannel = 512;

        public bool IsConnected = false;
       // public ConcurrentQueue<DmxData> DataQueue = new ConcurrentQueue<DmxData>();

        public DMX(short channelCount)
        {
        }

        public void Connect()
        {
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Connecting to DMX Device...");
            ushort vendorId = 0x10CF;
            ushort productId = 0x8062;
            ushort usagePage = 0x00FF;
            ushort usageId = 0x0001;

            try
            {
                //// K8062
                IsConnected = false;
                if (DMXController != null) DMXController.Dispose();
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Loading DMX Controller...");
                DMXController = new DMXController();
                DMXController.StartDevice();
                DMXController.SetChannelCount(256);
                Thread.Sleep(2000);
                DMXController.SetChannelCount(256);
                Thread.Sleep(2000);
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[<] DMX: SetChannelCount: " + maxChannel);
                IsConnected = true;
                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] Connected");

                //start listening for queued messages
                StartQueueListening();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[!] " + ex.Message);
                IsConnected = false;
                throw ex;
            }
        }

        public byte[] GetDMXFromColors(IEnumerable<Color> LEDColors)
        {
            if (LEDColors == null || !LEDColors.Any()) return null;
            if (LEDColors.Count() > maxChannel / 3)
                LEDColors = LEDColors.Take(maxChannel / 3);

            byte[] channels = LEDColors.SelectMany((color) =>
            {
                var A = color.A / 255M;
                var RGB = new ColorMine.ColorSpaces.Rgb() { R = (int)(color.R * A), G = (int)(color.G * A), B = (int)(color.B * A) };
                var CMY = RGB.To<ColorMine.ColorSpaces.Cmy>();
                // LAB
                return new byte[] {
                    (byte)(15*CMY.C),
                    (byte)(15*CMY.M),
                    (byte)(15*CMY.Y)
                };
            }).ToArray();

            return channels;
        }

        //public void SetColor(Color color)
        //{
        //    byte[] dmxdata = GetDMXFromColors(new Color[] { color, color, color, color, color, color, color });
        //    Console.WriteLine("[" + color.ToString() + "]: (" + dmxdata[0] + "," + dmxdata[1] + "," + dmxdata[2] + ") ");
        //    SendDMXFrames(dmxdata);
        //}

        public void SetEdgeLightStrips(Color color)
        {
            foreach (LightStrip strip in Config.VerticesLightStrips)
            {
                strip.SetColor(color);
                var channels = strip.GetDMXChannelColors();
                foreach (var key in channels.Keys)
                {
                    //DataQueue.Enqueue(new DmxData(key, channels[key]));
                    DMXController.SetChannel(key, channels[key]);
                }
            }
        }

        public void SetChandelierLightStrips(Dictionary<short,Color> colors)
        {
            foreach (var k in colors.Keys)
            {
                Config.ChandelierLightStrips[k].SetColor(colors[k]);
                var channels = Config.ChandelierLightStrips[k].GetDMXChannelColors();
                foreach (var key in channels.Keys)
                {
                    //DataQueue.Enqueue(new DmxData(key, channels[key]));
                    DMXController.SetChannel(key, channels[key]);
                }
            }
        }

        public void SetLightStripColor(LightStrip strip, Color color)
        {
            strip.SetColor(color);
            var channels = strip.GetDMXChannelColors();
            foreach (var key in channels.Keys)
            {
                //DataQueue.Enqueue(new DmxData(key, channels[key]));
                DMXController.SetChannel(key, channels[key]);
            }
        }

        public void Dispose()
        {
            if (this.DMXController != null && IsConnected)
                this.DMXController.Dispose();
            VellemanDMX.StopDevice();
        }
        
        #region Testing

        public async Task TestSequence()
        {
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("[ ] Testing lights...");

            foreach (var color in new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Cyan, Color.Purple, Color.Pink, Color.White, Color.LightGray, Color.Gray, Color.DarkGray, Color.Black })
            {
                SetEdgeLightStrips(color);
                await Task.Delay(250);
            }
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[-] Done testing");
        }
        
        public async Task Test()
        {
            byte max = 255;
            byte high = 200;
            byte med = 100;
            byte low = 50;
            byte verylow = 15;

            while (true)
            {
                for (short i = 20; i < 24; i++)
                {
                    Debug.WriteLine($"({i}:{max},{high},{med},{low},{verylow})");
                    DMXController.SetChannel(i, max);
                    Thread.Sleep(200);
                    DMXController.SetChannel(i, high);
                    Thread.Sleep(200);
                    DMXController.SetChannel(i, med);
                    Thread.Sleep(200);
                    DMXController.SetChannel(i, low);
                    Thread.Sleep(200);
                    DMXController.SetChannel(i, verylow);
                    Thread.Sleep(2000);
                }
            }
        }

        #endregion

        private void StartQueueListening()
        {
            //Task.Run(async () =>
            // {
            //     while (true)
            //     {
            //         DmxData data;
            //         if (DataQueue.TryDequeue(out data))
            //         {
            //             Debug.WriteLine(data);
            //             DMXController.SetChannel(data.Channel, data.Value);
            //         }
            //         else
            //         {
            //            // don't run again for at least 200 milliseconds
            //            await Task.Delay(200);
            //         }
            //     }
            // });
        }
    }
}

