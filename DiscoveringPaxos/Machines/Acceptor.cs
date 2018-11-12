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

        private Proposal lastAckedProposal;

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

            if ((lastAckedProposal == null ||
                 proposal.GreaterThan(lastAckedProposal)))
            {
                lastAckedProposal = proposal;
                Send(proposer, new ProposalResponse(
                    this.Id, 
                    proposal, 
                    true /* acknowledged */,
                    this.lastAcceptedProposal,
                    this.acceptedValue));
            }
        }

        public void AcceptRequestHandler()
        {
            var acceptRequest = (AcceptRequest)ReceivedEvent;
            var proposal = acceptRequest.Proposal;
            var value = acceptRequest.Value;

            if (lastAckedProposal == null ||
                !lastAckedProposal.Equals(proposal))
            {
                return;
            }

            //if (this.lastAcceptedProposal != null && 
            //    !this.lastAcceptedProposal.Equals(proposal))
            //{
            //    Assert(this.acceptedValue == value);
            //}

            this.lastAcceptedProposal = proposal;
            this.acceptedValue = value;

            foreach (var learner in learners)
            {
                Send(learner, new ValueAcceptedEvent(this.Id, proposal, value));
            }
        }

        public void HaltAcceptorEventHandler()
        {
            Raise(new Halt());
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(ProposalRequest), nameof(ProposalRequestHandler))]
        [OnEventDoAction(typeof(AcceptRequest), nameof(AcceptRequestHandler))]
        [OnEventDoAction(typeof(HaltAcceptorEvent), nameof(HaltAcceptorEventHandler))]
        internal class Init : MachineState
        {
        }
    }
}
