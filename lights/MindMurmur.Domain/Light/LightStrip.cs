using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;

namespace MindMurmur.Domain.Light
{
    [DataContract]
    public class LightStrip
    {
        [DataMember]
        /// <summary>
        /// starting index of this fixture
        /// </summary>
        public short Index { get; set; }

        [DataMember]
        /// <summary>
        /// amount of addresses on a single fixure before it hits the next fixture
        /// </summary>
        public short Offset { get; set; }

        [DataMember]
        /// <summary>
        /// Dimmer override
        /// </summary>
        public short Dimmer { get; set; }

        [DataMember]
        /// <summary>
        /// Controls whether the lightstrip honors alpha or not.
        /// If it does not, the alpha will be used to reduce the RGB values
        /// by 255 - Current Alpha on Color.A 
        /// </summary>
        public bool HonorAlpha { get; set; }

        [DataMember]
        public List<LED> LEDList { get; set; }
        

        public LightStrip()
        {
            Index = 1;
            Offset = 3;//for the three colors
            LEDList = new List<LED>(){new LED(1)};
        }

        public LightStrip(int fixtureIndex, int numberOfLightSegments, short dimmer, bool honorAlpha, short lightOffset =0)
        {
            HonorAlpha = honorAlpha;
            Dimmer = dimmer;
            fixtureIndex = Convert.ToInt16(fixtureIndex+7);//TODO: figure out why the strip is indexed for red at 8 instead of 1
            Index = (short)fixtureIndex;
            Offset = (short)(fixtureIndex + lightOffset);
            LEDList = new List<LED>();
            var x = fixtureIndex;
            var maxIndex = fixtureIndex + (numberOfLightSegments * (4 + lightOffset));
            while (x<maxIndex)
            {
                LEDList.Add(new LED((short)x)); //add LED to the list
                x += 4+lightOffset;
            } //end while
        }

        /// <summary>
        /// Sets color for all LED lights
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            foreach (LED led in LEDList)
                led.SetColor(color, Dimmer);
        }

        /// <summary>
        /// Returns the individual channels and their respective colors that should be sent to fixture.
        /// This applies the alpha and dimmer corrections.
        /// </summary>
        /// <returns></returns>
        public Dictionary<short,byte> ChannelColors()
        {
            var rtn = new Dictionary<short, byte>();
            foreach (LED led in LEDList)
            {
                rtn.Add(led.AlphaChannel, led.Alpha);
                var red = HonorAlpha ? led.Red : led.Red.ApplyDimmer(led.Alpha, Dimmer);
                var green = HonorAlpha ? led.Green : led.Green.ApplyDimmer(led.Alpha, Dimmer);
                var blue = HonorAlpha ? led.Blue : led.Blue.ApplyDimmer(led.Alpha, Dimmer);
                Debug.WriteLine(string.Format("ChannelColors() [{0}, {1}, {2}, {3}]", led.Index, red, green, blue));
                rtn.Add(led.RedChannel, red);
                rtn.Add(led.GreenChannel,green);
                rtn.Add(led.BlueChannel,blue );
            }
            return rtn;
        }

        /// <summary>
        /// Applies the color to all light strips
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Dictionary<short, byte> ChannelColors(Color color)
        {
            SetColor(color);
            return ChannelColors();
        }

        public short MaxChannel {
            get
            {
                short maxChannel = 0;
                foreach (LED led in LEDList)
                {
                    if ((led.Index+Offset)>maxChannel)
                        maxChannel = (short)(led.Index + Offset);
                }
                return maxChannel;
            }
        }

    }
}
