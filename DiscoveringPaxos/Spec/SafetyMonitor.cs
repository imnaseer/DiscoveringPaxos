using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;

namespace DiscoveringPaxos.Spec
{
    public class SafetyMonitor : Monitor
    {
        [Start]
        [Hot]
        [OnEventGotoState(typeof(ValueLearnedEvent), typeof(ValueLearned))]
        internal class Init : MonitorState
        {
        }

        [OnEventGotoState(typeof(ValueLearnedEvent), typeof(ValueLearned))]
        internal class ValueLearned : MonitorState
        {
        }
    }
}
