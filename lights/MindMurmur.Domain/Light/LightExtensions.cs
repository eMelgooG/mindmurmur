using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.Light
{
    public static class LightExtensions
    {
        public static byte ApplyDimmer(this byte color, byte alpha, int dimmer)
        {
            return Convert.ToByte((color.AlphaDim(alpha) + Convert.ToInt16(dimmer) < 0) ?
                    0 : (color.AlphaDim(alpha) + Convert.ToInt16(dimmer) > 255) ?
                            255 :
                            color.AlphaDim(alpha) + dimmer);
        }

        public static byte AlphaDim(this byte color, byte alpha)
        {
            return Convert.ToByte((color - (255 - alpha)) < 0 ? 0 : color - (255 - alpha));
        }
        /// <summary>
        /// returns the faded color based using sin wave
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pulsetime"></param>
        /// <returns></returns>
        public static Color FadedColor(this Color color, float pulsetime)
        {
            return Color.FromArgb((int)(255 * (1 - Math.Sin(pulsetime * Math.PI))), color.R, color.G, color.B);
        }
    }
}
