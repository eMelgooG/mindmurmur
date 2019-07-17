using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMurmur.Domain.Messages
{
    public class HeartRateCommand
    {
        public string CommandId { get; set; }
        public short HeartRate { get; set; }
    }
}
