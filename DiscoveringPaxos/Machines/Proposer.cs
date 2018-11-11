using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;

namespace DiscoveringPaxos.Machines
{
    public class Proposer : Machine
    {
        private string name;
        private List<MachineId> acceptors;
        private string value;

        private int proposalCounter = 0;
        private Proposal currentProposal = null;
        private HashSet<MachineId> receivedProposalResponses = null;

        public void InitOnEntry()
        {
            var initEvent = (ProposerInitEvent)ReceivedEvent;

            this.name = initEvent.Name;
            this.acceptors = initEvent.Acceptors;

            Goto<Ready>();
        }

        public void ProposeValueRequestHandler()
        {
            var request = (ClientProposeValueRequest)ReceivedEvent;

            this.value = request.Value;
            this.proposalCounter++;
            this.receivedProposalResponses = new HashSet<MachineId>();
            this.currentProposal = new Proposal(this.name, proposalCounter);
            foreach (var acceptor in acceptors)
            {
                Send(acceptor, new ProposalRequest(this.Id, currentProposal));
            }

            Goto<WaitingForAcks>();
        }

        public void ProposalResponseHandler()
        {
            var response = (ProposalResponse)ReceivedEvent;

            if (!response.Acknowledged)
            {
                return;
            }

            receivedProposalResponses.Add(response.From);

            if (receivedProposalResponses.Count == acceptors.Count)
            {
                foreach (var acceptor in acceptors)
                {
                    Send(acceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
                }
            }
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        internal class Init : MachineState
        {
        }

        [OnEventDoAction(typeof(ClientProposeValueRequest), nameof(ProposeValueRequestHandler))]
        internal class Ready : MachineState
        {
        }

        [OnEventDoAction(typeof(ProposalResponse), nameof(ProposalResponseHandler))]
        internal class WaitingForAcks : MachineState
        {
        }
    }
}
