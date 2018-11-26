using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Machines;

namespace DiscoveringPaxos.Events.Request
{
    public class ProposerInitEvent : Event
    {
        public ProposerInitEvent(string name, Dictionary<string, MachineId> acceptors, MachineId networkMachine)
        {
            this.Name = name;
            this.Acceptors = acceptors;
            this.NetworkMachine = networkMachine;
        }

        public string Name { get; private set; }

        public Dictionary<string, MachineId> Acceptors { get; private set; }

        public MachineId NetworkMachine { get; private set; }
    }
}
