using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Machines;

namespace DiscoveringPaxos.Events.Request
{
    public class NodeInitEvent : Event
    {
        public NodeInitEvent(string name, Dictionary<string, MachineId> nodes)
        {
            this.Name = name;
            this.Nodes = nodes;
        }

        public string Name { get; private set; }

        public Dictionary<string, MachineId> Nodes { get; private set; }

        public override string GetEventRepresentation()
        {
            return "node-init-event(" + this.Name + ")";
        }
    }
}
