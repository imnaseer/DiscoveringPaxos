using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class ClientProposeValueRequest : Event
    {
        public ClientProposeValueRequest(MachineId from, string value)
        {
            this.From = from;
            this.Value = value;
        }

        public MachineId From { get; private set; }
        
        public string Value { get; private set; }
    }
}
