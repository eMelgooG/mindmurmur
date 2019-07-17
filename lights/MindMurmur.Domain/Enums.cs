using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain
{
    public enum MeditationState
    {
        OFF = -1,
        IDLE = 0,
        LEVEL_1 = 1,
        LEVEL_2 = 2,
        LEVEL_3 = 3,
        LEVEL_4 = 4,
        LEVEL_5 = 5
    }

    public enum ChandelierBehavior
    {
        ALTERNATE_ALL_COLORS_OVER_TIMESPAN = 0,
        SEPARATE_COLORS_ACROSS_LEVELS = 1
    }
}
