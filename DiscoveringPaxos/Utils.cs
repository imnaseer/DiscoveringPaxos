using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoveringPaxos
{
    public class Utils
    {
        public static bool GreaterThan50PercentObservations(double observed, double total)
        {
            return (observed / total) > 0.5;
        }
    }
}
