using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Lights
{
    public static class Formulas
    {
        public static float Clamp(float inrange_value, float inrange_min, float inrange_max, float outrange_min, float outrange_max, bool overflow = false)
        {
            float inpct = (inrange_value - inrange_min) / (inrange_max - inrange_min);
            return Clamp(inpct, outrange_min, outrange_max, overflow);
        }

        public static float Clamp(float percent, float outrange_min, float outrange_max, bool overflow = false)
        {
            float delta = outrange_max - outrange_min;
            if (!overflow)
                percent = (percent > 1 ? 1 : percent < 0 ? 0 : percent);
            return (percent * delta) + outrange_min;
        }

        public static float Easing_Cubic(float percent, float minValue, float MaxValue)
        {
            percent *= 2;
            if (percent < 1) return (MaxValue - minValue) / 2f * percent * percent * percent + minValue;
            percent -= 2;
            return (MaxValue - minValue) / 2f * (percent * percent * percent + 2f) + minValue;
        }
        public static float Easing_Square(float percent, float minValue, float MaxValue)
        {
            percent *= 2;
            if (percent < 1) return (MaxValue - minValue) / 2f * percent * percent + minValue;
            percent -= 2;
            return (MaxValue - minValue) / 2f * (percent * percent + 2f) + minValue;
        }
    }
}
