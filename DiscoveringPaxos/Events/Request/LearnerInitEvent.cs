using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Machines;

namespace DiscoveringPaxos.Events.Request
{
    public class LearnerInitEvent :Event
    {
        public LearnerInitEvent(string name, List<MachineId> acceptors)
        {
            this.Name = name;
            this.Acceptors = acceptors;
        }

        public string Name { get; private set; }
        public List<MachineId> Acceptors { get; private set; }
    }
}
