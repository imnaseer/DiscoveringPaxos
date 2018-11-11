using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos;
using DiscoveringPaxos.Events.Request;
using DiscoveringPaxos.Spec;

namespace DiscoveringPaxos.Machines
{
    public class Acceptor : Machine
    {
        private string name;
        private List<MachineId> proposers;
        private List<MachineId> learners;

        private Proposal lastAcceptedProposal;
        private string acceptedValue;

        public void InitOnEntry()
        {
            var initEvent = (AcceptorInitEvent)ReceivedEvent;

            this.name = initEvent.Name;
            this.proposers = initEvent.Proposers;
            this.learners = initEvent.Learners;
        }

        public void ProposalRequestHandler()
        {
            var proposalRequest = (ProposalRequest)ReceivedEvent;

            var proposer = proposalRequest.From;
            var proposal = proposalRequest.Proposal;

            if (this.acceptedValue == null && 
                (lastAcceptedProposal == null ||
                 proposal.GreaterThan(lastAcceptedProposal)))
            {
                lastAcceptedProposal = proposal;
                Send(proposer, new ProposalResponse(this.Id, proposal, acknowledged: true));
            }
        }

        public void AcceptRequestHandler()
        {
            var acceptRequest = (AcceptRequest)ReceivedEvent;
            var proposal = acceptRequest.Proposal;
            var value = acceptRequest.Value;

            if (lastAcceptedProposal == null ||
                !lastAcceptedProposal.Equals(proposal))
            {
                return;
            }

            this.acceptedValue = value;

            foreach (var learner in learners)
            {
                Send(learner, new ValueAcceptedEvent(this.Id, value));
            }
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(ProposalRequest), nameof(ProposalRequestHandler))]
        [OnEventDoAction(typeof(AcceptRequest), nameof(AcceptRequestHandler))]
        internal class Init : MachineState
        {
        }
    }
}
