using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MindMurmur.Domain;
using MindMurmur.Domain.Light;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MindMurmur.Lights.Control
{
    public static class Config
    {
        /// <summary>
        /// Channels currently configured for LED lights
        /// </summary>
        public static List<LightStrip> VerticesLightStrips = new List<LightStrip>();
        public static Dictionary<short,LightStrip> ChandelierLightStrips = new Dictionary<short, LightStrip>();
        public static Int16 DimmerValue = -1; //TODO: make this a config value
        public static Int32 ChandelierLevelSwapSeconds = 10;
        public static ChandelierBehavior ChandelierOperationBehavior = ChandelierBehavior.ALTERNATE_ALL_COLORS_OVER_TIMESPAN;

        static Config() {

        }

        public static void Init() {

            List<LightStrip> lightStrips = JsonConvert.DeserializeObject<List<LightStrip>>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigData", "lightStripConfig.json")));
            Dictionary<short, LightStrip> chandelierLightStrips = JsonConvert.DeserializeObject<Dictionary<short, LightStrip>>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigData", "chandelierLightStripConfig.json")));

            foreach (LightStrip strip in lightStrips)
            {
                strip.LEDList.RemoveAt(0);
            }
            foreach (var k in chandelierLightStrips.Keys)
            {
                chandelierLightStrips[k].LEDList.RemoveAt(0);
            }

            VerticesLightStrips = lightStrips;
            ChandelierLightStrips = chandelierLightStrips;
        }

        public static short MaxChannels
        {
            get
            {
                short maxChannel = 0;
                foreach (LightStrip strip in VerticesLightStrips)
                {
                    if (strip.MaxChannel > maxChannel)
                        maxChannel = strip.MaxChannel;
                }
                foreach (var k in ChandelierLightStrips.Keys)
                {
                    if (ChandelierLightStrips[k].MaxChannel > maxChannel)
                        maxChannel = ChandelierLightStrips[k].MaxChannel;
                }
                return maxChannel;
            }
        }

        public static Dictionary<MeditationState, Tuple<Color, Color, Color>> MeditationColors =
            new Dictionary<MeditationState, Tuple<Color, Color, Color>>()
            {
                {
                    MeditationState.OFF,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 0, 0),
                        Color.FromArgb(255, 0, 0, 0),
                        Color.FromArgb(255, 0, 0, 0))
                },
                {
                    MeditationState.IDLE,
                        new Tuple<Color, Color, Color>(
                            Color.FromArgb(255, 204, 0, 255),
                            Color.FromArgb(255, 0, 255, 24),
                            Color.FromArgb(255, 0, 89, 255))
                },
                {
                    MeditationState.LEVEL_4,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 192, 255),
                        Color.FromArgb(255, 255, 188, 0),
                        Color.FromArgb(255, 22, 232, 107))
                },
                {
                    MeditationState.LEVEL_3,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(255, 181, 0, 255),
                        Color.FromArgb(255, 232, 204,2))
                },
                {
                    MeditationState.LEVEL_2,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 255, 210, 0),
                        Color.FromArgb(255, 0, 135, 255),
                        Color.FromArgb(255, 255, 52, 211))
                },
                {
                    MeditationState.LEVEL_1,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 255, 0, 0),
                        Color.FromArgb(255, 150, 0, 255),
                        Color.FromArgb(255, 78, 184, 255))
                },
                {
                    MeditationState.LEVEL_5,
                    new Tuple<Color, Color, Color>(
                        Color.FromArgb(255, 204, 0, 255),
                        Color.FromArgb(255, 0, 255, 24),
                        Color.FromArgb(255, 0, 89, 255))
                }
            };


    }
}
