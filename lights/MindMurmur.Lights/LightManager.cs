using EasyNetQ;
using MindMurmur.Domain.Messages;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using MindMurmur.Domain;
using MindMurmur.Domain.Light;
using MindMurmur.Lights.Control;

namespace MindMurmur.Lights
{
    public class LightManager
    {
        #region Private Properties

        private Subject<int> bpmSubject = new Subject<int>();
        private Subject<int> dimmerSubject = new Subject<int>();
        private Subject<Color> colorSubject = new Subject<Color>();
        private Subject<MeditationState> meditationSubject = new Subject<MeditationState>();
        private Timer heartRatePulseTimer;
        private float pulsetime;
        private DateTime lastDmxTickDateTime = DateTime.UtcNow;
        private DateTime lastStateChange = DateTime.UtcNow;
        private DateTime lastChandalierSwap = DateTime.UtcNow;
        private Bitmap fromTransitionBitmap = new Bitmap(51, 2);
        private Bitmap toTransitionBitmap = new Bitmap(51, 2);
        private Bitmap fromTransitionBitmapSecondaryColor = new Bitmap(51, 2);
        private Bitmap toTransitionBitmapSecondaryColor = new Bitmap(51, 2);
        private Bitmap fromTransitionBitmapTertiaryColor = new Bitmap(51, 2);
        private Bitmap toTransitionBitmapTertiaryColor = new Bitmap(51, 2);
        private Bitmap meditativeStateBitmapColor = new Bitmap(101, 2);
        private int currentHeartRate = 20;
        private short startingChandelierIndex = 1;
        private short chandelierColorIndex = 1;
        private double secondsFromLastTransition
        {
            get { return (DateTime.UtcNow - lastStateChange).TotalSeconds; }
        }
        private bool isTransitioning
        {
            get { return secondsFromLastTransition < 15.0; }
        }

        #endregion

        #region Public Properties

        public MeditationState PreviousMediationState { get; set; }
        public MeditationState CurrentMediationState { get; set; }
        private int CurrentHeartRate
        {
            get { return (secondsFromLastTransition > 15.0) ? currentHeartRate / 2 : currentHeartRate; }
            set { currentHeartRate = value; }
        }
        public Color PreviousColor { get; set; }
        public Color PreviousSecondaryColor { get; set; }
        public Color PreviousTertiaryColor { get; set; }
        public Color CurrentColor { get; set; }
        public Color CurrentSecondaryColor { get; set; }
        public Color CurrentTertiaryColor { get; set; }
        public DMX LightDMX { get; set; }

        #endregion

        #region ctor

        public LightManager(DMX dmx)
        {
            PreviousMediationState = MeditationState.IDLE;
            CurrentMediationState = MeditationState.IDLE;
            PreviousColor = Color.Blue;
            CurrentColor = Color.Aqua;
            CurrentSecondaryColor = Color.LightSlateGray;
            CurrentTertiaryColor = Color.HotPink;
            CurrentHeartRate = 50;
            LightDMX = dmx;
            BuildGradientTransition();
            // 25 FPS timer to update LEDs 
            heartRatePulseTimer = new Timer(PulseTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Color Handlers

        /// <summary>
        /// Builds two gradient graphic objects that are use to get a pixel color for transitioning between one color to another
        /// </summary>
        private void BuildGradientTransition()
        {
            lock (meditativeStateBitmapColor)
            {
                using (var graphics = Graphics.FromImage(fromTransitionBitmap))
                {
                    ColorBlend blend = new ColorBlend(3);
                    blend.Colors = new Color[3] { CurrentColor, CurrentSecondaryColor, CurrentTertiaryColor };
                    blend.Positions = new float[3] { 0f, 0.5f, 1f };
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(meditativeStateBitmapColor.Width, meditativeStateBitmapColor.Height), CurrentColor, CurrentSecondaryColor) {InterpolationColors = blend})
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (fromTransitionBitmap)
            {
                using (var graphics = Graphics.FromImage(fromTransitionBitmap))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0),
                        new Point(fromTransitionBitmap.Width, fromTransitionBitmap.Height), PreviousColor, Color.White))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (toTransitionBitmap)
            {
                using (var graphics = Graphics.FromImage(toTransitionBitmap))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(toTransitionBitmap.Width, toTransitionBitmap.Height),Color.White, CurrentColor))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (fromTransitionBitmapSecondaryColor)
            {
                using (var graphics = Graphics.FromImage(fromTransitionBitmapSecondaryColor))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(fromTransitionBitmapSecondaryColor.Width, fromTransitionBitmapSecondaryColor.Height),PreviousColor,Color.White))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (toTransitionBitmapSecondaryColor)
            {
                using (var graphics = Graphics.FromImage(toTransitionBitmapSecondaryColor))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(toTransitionBitmapSecondaryColor.Width, toTransitionBitmapSecondaryColor.Height),Color.White, CurrentColor)){
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (fromTransitionBitmapTertiaryColor)
            {
                using (var graphics = Graphics.FromImage(fromTransitionBitmapTertiaryColor))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(fromTransitionBitmapTertiaryColor.Width, fromTransitionBitmapTertiaryColor.Height), PreviousColor, Color.White))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }
            lock (toTransitionBitmapTertiaryColor)
            {
                using (var graphics = Graphics.FromImage(toTransitionBitmapTertiaryColor))
                {
                    using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(toTransitionBitmapTertiaryColor.Width, toTransitionBitmapTertiaryColor.Height), Color.White, CurrentColor))
                    {
                        graphics.FillRectangle(gradientBrush, gradientBrush.Rectangle);
                    }
                }
            }

        }

        /// <summary>
        /// Returns the current color from the ascending or descending transition color scheme
        /// </summary>
        /// <returns></returns>
        private Color GetCurrentColor()
        {
            var seconds = secondsFromLastTransition;
            Color rtn = CurrentColor;
            if (seconds > 5 && seconds <= 10) rtn = Color.White;
            if (seconds <= 5)
            {
                lock (fromTransitionBitmap)
                {
                    rtn = fromTransitionBitmap.GetPixel(Convert.ToInt32(seconds) * 10, 1);
                }
            }
            if (!(seconds > 10) || !(seconds < 15)) return rtn;
            lock (toTransitionBitmap)
            {
                rtn = toTransitionBitmap.GetPixel(Convert.ToInt32(seconds - 10.0) * 10, 1);
            }
            return rtn;
        }

        private Color GetChandelierRangeColor(float locationPercentage)
        {
            Color rtn = CurrentColor;
            lock (meditativeStateBitmapColor)
            {
                rtn = meditativeStateBitmapColor.GetPixel(Convert.ToInt32(locationPercentage*100), 1);
            }
            return rtn;
        }

        /// <summary>
        /// Returns the current secondary color from the ascending or descending transition color scheme
        /// </summary>
        /// <returns></returns>
        private Color GetCurrentSecondaryColor()
        {
            var seconds = secondsFromLastTransition;
            Color rtn = CurrentSecondaryColor;
            if (seconds > 5 && seconds <= 10) rtn = Color.White;
            if (seconds <= 5)
            {
                lock (fromTransitionBitmapSecondaryColor)
                {
                    rtn = fromTransitionBitmapSecondaryColor.GetPixel(Convert.ToInt32(seconds) * 10, 1);
                }
            }
            if (!(seconds > 10) || !(seconds < 15)) return rtn;
            lock (toTransitionBitmapSecondaryColor)
            {
                rtn = toTransitionBitmapSecondaryColor.GetPixel(Convert.ToInt32(seconds - 10.0) * 10, 1);
            }
            return rtn;
        }

        /// <summary>
        /// Returns the current tertiary color from the ascending or descending transition color scheme
        /// </summary>
        /// <returns></returns>
        private Color GetCurrentTertiaryColor()
        {
            var seconds = secondsFromLastTransition;
            Color rtn = CurrentSecondaryColor;
            if (seconds > 5 && seconds <= 10) rtn = Color.White;
            if (seconds <= 5)
            {
                lock (fromTransitionBitmapTertiaryColor)
                {
                    rtn = fromTransitionBitmapTertiaryColor.GetPixel(Convert.ToInt32(seconds) * 10, 1);
                }
            }
            if (!(seconds > 10) || !(seconds < 15)) return rtn;
            lock (toTransitionBitmapTertiaryColor)
            {
                rtn = toTransitionBitmapTertiaryColor.GetPixel(Convert.ToInt32(seconds - 10.0) * 10, 1);
            }
            return rtn;
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        private Dictionary<short, Color> GetChandelierColors(List<Color> colors)
        {
            var startIndex = startingChandelierIndex;
            var rtn = new Dictionary<short, Color>();
            var colorIndex = 0;

            //this keeps track of when the chandelier has cycled through each of the colors for the current state
            //chandelierColorIndex is 1,2,3 for primary, secondary, tertiary colors
            if (lastChandalierSwap < DateTime.UtcNow.AddSeconds(-1 * Config.ChandelierLevelSwapSeconds))
            {
                chandelierColorIndex = chandelierColorIndex == 3 ? (short) 1 : Convert.ToInt16(chandelierColorIndex + 1);
                lastChandalierSwap = DateTime.UtcNow;
            }

            //there are two controls here, either we are transitioning between meditative states or we are not
            if (isTransitioning)
            {
                for (short i = startIndex; i <= Config.ChandelierLightStrips.Count; i++)
                {
                    rtn.Add(i, colors[colorIndex]);
                    colorIndex += 1;
                    if (colorIndex > colors.Count - 1) colorIndex = 0; //if we have used all the colors up, start over
                }
                foreach (var k in Config.ChandelierLightStrips.Keys)
                {
                    if (!rtn.ContainsKey(k))
                        rtn.Add(k, colors[colorIndex]);
                    colorIndex += 1;
                    if (colorIndex > colors.Count - 1) colorIndex = 0; //if we have used all the colors up, start over
                }
            }
            else
            {
                //we are NOT transitioning

                switch (Config.ChandelierOperationBehavior)
                {
                    case ChandelierBehavior.ALTERNATE_ALL_COLORS_OVER_TIMESPAN:
               
                        foreach (var k in Config.ChandelierLightStrips.Keys)
                        {
                            if (!rtn.ContainsKey(k))
                                rtn.Add(k, colors[colorIndex]);
                            colorIndex += 1;
                            if (colorIndex > colors.Count - 1) colorIndex = 0; //if we have used all the colors up, start over
                        }
                        break;
                }

            }
            return rtn;
        }
        #endregion

        #region Timers

        /// <summary>
        /// timer event that initiates the lights according to the current color and pulse
        /// </summary>
        /// <param name="state"></param>
        private void PulseTimer_Tick(object state)
        {
            var deltatime = DateTime.UtcNow - lastDmxTickDateTime;
            lastDmxTickDateTime = DateTime.UtcNow;
            //move pulse timer from heartbeat rate
            pulsetime += (float)deltatime.TotalSeconds * (CurrentHeartRate / 60f);
            // restrict value between 0 and 1
            pulsetime -= (float)Math.Floor(pulsetime);
            var fadedColor = GetCurrentColor().FadedColor(pulsetime);
            var fadedSecondaryColor = GetCurrentSecondaryColor().FadedColor(pulsetime);
            var fadedTertiaryColor = GetCurrentTertiaryColor().FadedColor(pulsetime);
            // 0 = top | 0.5 = low | 1 = top
            //int alpha = (int)(255 * (1 - Math.Sin(pulsetime * Math.PI)));
            //Color fadedColor = Color.FromArgb(alpha, color.R, color.G, color.B);

            //byte[] dmx = this.LightDMX.GetDMXFromColors(new List<Color> { fadedColor });
            //LightDMX.SendDMXFrames(dmx);
            LightDMX.SetEdgeLightStrips(fadedColor);

            LightDMX.SetChandelierLightStrips(GetChandelierColors( new List<Color>(){ fadedColor, fadedSecondaryColor, fadedTertiaryColor }));

            Console.WriteLine($"LED COLOR: {fadedColor.ToString()}");
        }

        public void StartChandelierCycle(int amountOfTimeSeconds, int frequencySeconds)
        {
            Observable
                .Timer(TimeSpan.FromSeconds(amountOfTimeSeconds), TimeSpan.FromSeconds(frequencySeconds))
                .Subscribe(
                    x =>
                    {
                        startingChandelierIndex =
                            Convert.ToInt16((startingChandelierIndex + 1 > Config.ChandelierLightStrips.Count)
                                ? 1 : startingChandelierIndex + 1);
                        Console.WriteLine($"startingChandelierIndex: {startingChandelierIndex}");
                    }
                    );
        }

        #endregion 

        /// <summary>
        /// Starts the primary light processes
        /// </summary>
        /// <returns></returns>
        async public Task Start()
        {
            // Start DMX
            try
            {
                if (LightDMX != null)
                    await LightDMX.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: DMX controller not found. continuing...\r\n" + ex);
            }

            if (CurrentHeartRate < 20) CurrentHeartRate = 20;

            //sets up bus connection and subscribes
            HitchToTheBus();

            // set pulse timer @ 25 FPS to update LEDs
            heartRatePulseTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(40));
        }

        /// <summary>
        /// Establishes all bus subscriptions to RabbitMQ
        /// </summary>
        public void HitchToTheBus()
        {
            var bus = RabbitHutch.CreateBus("host=localhost");

            var heartRateSubscribeResult = bus.Subscribe<HeartRateCommand>("heartRateCommand", (cmd) =>
            {
                if (CurrentHeartRate != cmd.HeartRate)
                {
                    Console.WriteLine($"Heart.Rx {cmd.CommandId} [{cmd.HeartRate}]");
                    CurrentHeartRate = cmd.HeartRate / 2;//cutting the heart rate in half
                    bpmSubject.OnNext(cmd.HeartRate);
                }
            });

            var dimmerControlSubscribeResult = bus.Subscribe<DimmerControlCommand>("dimmerControlCommand", (cmd) =>
            {
                if (CurrentHeartRate != cmd.DimmerValue)
                {
                    Console.WriteLine($"Dimmer.Rx {cmd.CommandId} [{cmd.DimmerValue}]");
                    Config.DimmerValue = cmd.DimmerValue;
                    foreach (LightStrip strip in Config.VerticesLightStrips)
                    {
                        strip.Dimmer = Config.DimmerValue;
                    }
                    foreach (var k in Config.ChandelierLightStrips.Keys)
                    {
                        Config.ChandelierLightStrips[k].Dimmer = Config.DimmerValue;
                    }
                    dimmerSubject.OnNext(cmd.DimmerValue);
                }
            });

            var colorControlSubscribeResult = bus.Subscribe<ColorControlCommand>("colorCommand", (cmd) => {
                Color cmdColor = Color.FromArgb(cmd.ColorRed, cmd.ColorGreen, cmd.ColorBlue);
                if (CurrentColor != cmdColor)
                {
                    Console.WriteLine($"Color.Rx {cmd.CommandId} [{cmd.ColorRed},{cmd.ColorGreen},{cmd.ColorBlue}]");
                    CurrentColor = cmdColor;
                    colorSubject.OnNext(cmdColor);
                }
            });

            var meditationStateSubscribeResult = bus.Subscribe<MeditationStateCommand>("meditationStateCommand", (cmd) => {
                Console.WriteLine($"MeditationState.Rx {cmd.CommandId} [{cmd.State}]");
                var thisState = (MeditationState)cmd.State;
                lastStateChange = DateTime.UtcNow;
                //Set previous values
                PreviousMediationState = thisState;
                PreviousColor = CurrentColor;
                PreviousSecondaryColor = CurrentSecondaryColor;
                PreviousTertiaryColor = CurrentTertiaryColor;
                //set new colors
                CurrentColor = Config.MeditationColors[thisState].Item1; //sets the primary color from the meditation state
                CurrentSecondaryColor = Config.MeditationColors[thisState].Item2; //sets the secondary color from the meditation state
                CurrentTertiaryColor = Config.MeditationColors[thisState].Item3; //sets the tertiary color from the meditation state
                CurrentMediationState = thisState;//record the current state

                meditationSubject.OnNext(thisState);
                // meditationSubject.Subscribe(state => { });
                StartChandelierCycle(15, 3);

                BuildGradientTransition();//build the bitmap so we can get the color for transitioning
            });

            var eegDataSubcribeResult = bus.Subscribe<EEGDataCommand>("eegDataCommand", (cmd) => {
                Console.WriteLine($"EEGData.Rx {cmd.CommandId} [{string.Join(", ", cmd.Values)}]");
            });

            Console.WriteLine("[ ] Hitched to the bus");
        }

        public async Task RunTestsTask()
        {
            if (LightDMX != null)
                await LightDMX.Connect();

            await LightDMX.TestSequence().ConfigureAwait(true);
            await LightDMX.Test().ConfigureAwait(true);
        }
    }
}
