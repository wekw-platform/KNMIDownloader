using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knmidownloader.DataModels
{
    internal class ConditionData
    {
        public int MonthStart { get; set; }

        public int MonthEnd { get; set; }

        public int HourStart { get; set; }

        public int HourEnd { get; set; }
    }
}
