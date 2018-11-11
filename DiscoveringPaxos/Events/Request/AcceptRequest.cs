using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class AcceptRequest : Event
    {
        public AcceptRequest(MachineId from, Proposal proposal, string value)
        {
            this.From = from;
            this.Proposal = proposal;
            this.Value = value;
        }

        public MachineId From { get; private set; }

        public string Value { get; private set; }

        public Proposal Proposal { get; private set; }
    }
}
