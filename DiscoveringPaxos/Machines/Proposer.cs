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

        private int proposalCounter = 0;

        private string value;
        private Proposal currentProposal = null;
        private HashSet<MachineId> receivedProposalResponses = null;

        private Dictionary<MachineId, Proposal> acceptorToPreviouslyAcceptedProposal = new Dictionary<MachineId, Proposal>();
        private Dictionary<MachineId, string> acceptorToPreviouslyAcceptedValue = new Dictionary<MachineId, string>();

        private bool finalValueChosen = false;

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

            SendProposalRequests(request.Value);

            Goto<WaitingForAcks>();
        }

        public void ProposalResponseHandler()
        {
            var response = (ProposalResponse)ReceivedEvent;

            if (!response.Acknowledged)
            {
                return;
            }

            var fromAcceptor = response.From;
            var previouslyAcceptedProposal = response.PreviouslyAcceptedProposal;
            var previouslyAcceptedValue = response.PreviouslyAcceptedValue;

            if (previouslyAcceptedProposal != null)
            {
                acceptorToPreviouslyAcceptedProposal[fromAcceptor] = previouslyAcceptedProposal;
                acceptorToPreviouslyAcceptedValue[fromAcceptor] = previouslyAcceptedValue;
            }

            receivedProposalResponses.Add(response.From);

            if (!finalValueChosen && 
                Utils.GreaterThan50PercentObservations(receivedProposalResponses.Count, acceptors.Count))
            {
                Proposal chosenPreviouslyAcceptedProposal = null;
                string chosenValue = null;
                foreach (var acceptor in acceptorToPreviouslyAcceptedProposal.Keys)
                {
                    var proposal = acceptorToPreviouslyAcceptedProposal[acceptor];
                    if (chosenPreviouslyAcceptedProposal == null ||
                        proposal.GreaterThan(chosenPreviouslyAcceptedProposal))
                    {
                        chosenPreviouslyAcceptedProposal = proposal;
                        chosenValue = acceptorToPreviouslyAcceptedValue[acceptor];
                    }
                }

                if (chosenPreviouslyAcceptedProposal != null)
                {
                    Assert(chosenValue != null);
                    this.value = chosenValue;
                }

                finalValueChosen = true;

                foreach (var acceptor in receivedProposalResponses)
                {
                    Send(acceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
                }
            }
            else if (finalValueChosen)
            {
                Send(fromAcceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
            }
        }

        private void SendProposalRequests(string value)
        {
            this.value = value;
            this.proposalCounter++;
            this.receivedProposalResponses = new HashSet<MachineId>();
            this.currentProposal = new Proposal(this.name, proposalCounter);
            foreach (var acceptor in acceptors)
            {
                Send(acceptor, new ProposalRequest(this.Id, currentProposal));
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
