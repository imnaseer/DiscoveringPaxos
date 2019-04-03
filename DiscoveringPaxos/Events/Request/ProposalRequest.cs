using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class ProposalRequest : Event
    {
        public ProposalRequest(MachineId from, Proposal proposal)
        {
            this.From = from;
            this.Proposal = proposal;
        }

        public MachineId From { get; private set; }

        public Proposal Proposal { get; private set; }
    
        public override string GetEventRepresentation()
        {
            return "propose(" + this.Proposal.ToString() + ")";
        }
    }
}
