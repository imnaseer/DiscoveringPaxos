using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;
using DiscoveringPaxos.Spec;

namespace DiscoveringPaxos.Machines
{
    public class Learner : Machine
    {
        private string name;
        private List<MachineId> acceptors;

        private Dictionary<MachineId, string> acceptorToValueMap = 
            new Dictionary<MachineId, string>();

        public void InitOnEntry()
        {
            var initEvent = (LearnerInitEvent)ReceivedEvent;

            this.name = initEvent.Name;
            this.acceptors = initEvent.Acceptors;
        }

        public void ValueAcceptedEventHandler()
        {
            var acceptedEvent = (ValueAcceptedEvent)ReceivedEvent;

            var acceptor = acceptedEvent.Acceptor;
            var acceptedValue = acceptedEvent.Value;

            acceptorToValueMap[acceptor] = acceptedValue;

            if (acceptorToValueMap.Count == acceptors.Count)
            {
                bool? allSame = null;
                string lastValue = null;
                foreach (var value in acceptorToValueMap.Values)
                {
                    if (allSame == null)
                    {
                        lastValue = value;
                        allSame = true;
                    }
                    else if (lastValue != value)
                    {
                        allSame = false;
                        break;
                    }
                }

                Monitor<SafetyMonitor>(allSame == true || allSame == null ?
                    (Event)new ValueLearnedEvent() :
                    (Event)new ConflictingValuesLearnedEvent());
            }
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(ValueAcceptedEvent), nameof(ValueAcceptedEventHandler))]
        internal class Init : MachineState
        {
        }
    }
}
