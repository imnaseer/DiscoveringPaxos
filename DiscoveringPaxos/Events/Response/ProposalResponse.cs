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
        public ProposalResponse(
            MachineId from, 
            Proposal proposal, 
            bool acknowledged,
            Proposal previouslyAcceptedProposal,
            string previouslyAcceptedValue)
        {
            this.From = from;
            this.Proposal = proposal;
            this.Acknowledged = acknowledged;
            this.PreviouslyAcceptedProposal = previouslyAcceptedProposal;
            this.PreviouslyAcceptedValue = previouslyAcceptedValue;
        }

        public MachineId From { get; private set; }

        public Proposal Proposal { get; private set; }

        public bool Acknowledged { get; private set; }

        public Proposal PreviouslyAcceptedProposal { get; private set; }

        public string PreviouslyAcceptedValue { get; private set; }
    }
}
