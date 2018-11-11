using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class ProposalResponse : Event
    {
        public ProposalResponse(MachineId from, Proposal proposal, bool acknowledged)
        {
            this.From = from;
            this.Proposal = proposal;
            this.Acknowledged = acknowledged;
        }

        public MachineId From { get; private set; }

        public Proposal Proposal { get; private set; }

        public bool Acknowledged { get; private set; }
    }
}
