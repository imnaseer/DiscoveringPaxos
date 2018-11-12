using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class ValueAcceptedEvent : Event
    {
        public ValueAcceptedEvent(MachineId acceptor, Proposal proposal, string value)
        {
            this.Proposal = proposal;
            this.Acceptor = acceptor;
            this.Value = value;
        }

        public MachineId Acceptor { get; private set; }

        public Proposal Proposal { get; private set; }

        public string Value { get; private set; }
    }
}
