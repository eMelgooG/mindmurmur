using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.Messages
{
    public class ColorControlCommand
    {
        public short ColorBlue { get; set; }
        public short ColorGreen { get; set; }
        public short ColorRed { get; set; }
        public string CommandId { get; set; }
    }
}
