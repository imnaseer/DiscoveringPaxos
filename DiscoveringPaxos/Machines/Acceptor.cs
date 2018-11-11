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

        private string chosenValue;

        public void InitOnEntry()
        {
            var initEvent = (AcceptorInitEvent)ReceivedEvent;

            this.chosenValue = null;

            this.name = initEvent.Name;
            this.proposers = initEvent.Proposers;
            this.learners = initEvent.Learners;
        }

        public void ProposalRequestHandler()
        {
            var proposalRequest = (ProposalRequest)ReceivedEvent;

            var proposal = proposalRequest.Proposal;
            this.chosenValue = proposal.Value;

            foreach (var learner in learners)
            {
                Send(learner, new ValueAcceptedEvent(this.Id, this.chosenValue));
            }
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(ProposalRequest), nameof(ProposalRequestHandler))]
        internal class Init : MachineState
        {
        }
    }
}
