using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PSharp;
using Microsoft.PSharp.Timers;
using DiscoveringPaxos.Events.Request;
using DiscoveringPaxos.Spec;

namespace DiscoveringPaxos.Machines
{
    public class Node : TimedMachine
    {
        private string name;

        // Proposer fields
        private Dictionary<string, MachineId> acceptors;
        private Dictionary<MachineId, string> acceptorNames;

        private int proposalCounter = 0;

        private string value;
        private Proposal currentProposal = null;
        private HashSet<MachineId> receivedProposalResponses = null;

        private Dictionary<MachineId, Proposal> acceptorToPreviouslyAcceptedProposal = new Dictionary<MachineId, Proposal>();
        private Dictionary<MachineId, string> acceptorToPreviouslyAcceptedValue = new Dictionary<MachineId, string>();

        private bool finalValueChosen = false;

        private TimerId proposerTimerId;

        // Acceptor fields
        private Dictionary<string, MachineId> proposers;
        private Dictionary<string, MachineId> learners;

        private Proposal lastAckedProposal;

        private Proposal lastAcceptedProposal;
        private string acceptedValue;

        // Learner fields
        private Dictionary<MachineId, Proposal> acceptorToProposalMap = new Dictionary<MachineId, Proposal>();
        private Dictionary<Proposal, string> proposalToValueMap = new Dictionary<Proposal, string>();

        private string learnedValue = null;

        public void InitOnEntry()
        {
            var nodeInitEvent = (NodeInitEvent)ReceivedEvent;

            this.name = nodeInitEvent.Name;

            ProposerInitOnEntry(nodeInitEvent);
            AcceptorInitOnEntry(nodeInitEvent);
            LearnerInitOnEntry(nodeInitEvent);
        }

        protected override string GetStateInfo()
        {
            var proposerStateInfo = GetProposerStateInfo();
            var acceptorStateInfo = GetAcceptorStateInfo();
            var learnerStateInfo = GetLearnerStateInfo();

            return
                "Proposer: " + proposerStateInfo + "<br/>" +
                "Acceptor: " + acceptorStateInfo + "<br/>" +
                "Learner: " + learnerStateInfo;
        }

        // Proposer event handlers
        public void ProposerInitOnEntry(NodeInitEvent initEvent)
        {
            this.acceptors = initEvent.Nodes;
            this.acceptorNames = new Dictionary<MachineId, string>();
            foreach (string name in acceptors.Keys)
            {
                this.acceptorNames[acceptors[name]] = name;
            }
        }

        public void ProposeValueRequestHandler()
        {
            var request = (ClientProposeValueRequest)ReceivedEvent;

            this.value = request.Value;
            this.proposalCounter++;
            this.receivedProposalResponses = new HashSet<MachineId>();
            this.currentProposal = new Proposal(this.name, proposalCounter);

            this.proposerTimerId = this.StartTimer(payload: null, period: 1 * 60 * 1000 /*1 minute*/, IsPeriodic: true);

            SendProposalRequests(request.Value);
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
                    SendNetworkRequest(acceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
                }
            }
            else if (finalValueChosen)
            {
                SendNetworkRequest(fromAcceptor, new AcceptRequest(this.Id, this.currentProposal, this.value));
            }
        }

        public void TimerElapsedEventHandler()
        {
            this.proposalCounter++;
            this.receivedProposalResponses = new HashSet<MachineId>();
            this.currentProposal = new Proposal(this.name, proposalCounter);

            SendProposalRequests(this.acceptedValue == null ? this.value : this.acceptedValue);
        }

        protected string GetProposerStateInfo()
        {
            string currentProposalStr = currentProposal == null ?
                "<None>" :
                currentProposal.ToString();

            List<string> responses = new List<string>();
            if (receivedProposalResponses != null)
            {
                foreach (MachineId mid in receivedProposalResponses)
                {
                    string responseStr =
                        this.acceptorNames[mid];

                    if (acceptorToPreviouslyAcceptedValue.ContainsKey(mid))
                    {
                        responseStr += ": " +
                            acceptorToPreviouslyAcceptedValue[mid] + "@" + acceptorToPreviouslyAcceptedValue[mid].ToString();
                    }

                    responses.Add(responseStr);
                }
            }

            string valueStr = value == null ?
                "<None>" :
                value;

            return valueStr + ", " + currentProposalStr + ", " + String.Join("; ", responses);
        }

        private void SendProposalRequests(string value)
        {
            foreach (var acceptor in acceptors.Values)
            {
                SendNetworkRequest(acceptor, new ProposalRequest(this.Id, this.currentProposal));
            }
        }

        private void SendNetworkRequest(MachineId to, Event eventObject)
        {
            Send(to, eventObject);
        }

        // Acceptor event handlers
        public void AcceptorInitOnEntry(NodeInitEvent initEvent)
        {
            this.proposers = initEvent.Nodes;
            this.learners = initEvent.Nodes;
        }

        public void ProposalRequestHandler()
        {
            var proposalRequest = (ProposalRequest)ReceivedEvent;

            var proposer = proposalRequest.From;
            var proposal = proposalRequest.Proposal;

            ProposalRequestHandler(proposer, proposal);
        }

        public void ProposalRequestHandler(MachineId proposer, Proposal proposal)
        {
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

            this.lastAcceptedProposal = proposal;
            this.acceptedValue = value;

            foreach (var learner in learners.Values)
            {
                Send(learner, new ValueAcceptedEvent(this.Id, proposal, value));
            }
        }

        public void HaltAcceptorEventHandler()
        {
            Raise(new Halt());
        }

        protected string GetAcceptorStateInfo()
        {
            string lastAckedProposalStr = lastAckedProposal == null ?
                "<None>" :
                lastAckedProposal.ToString();

            string lastAcceptedProposalStr = lastAcceptedProposal == null ?
                "<None>" :
                lastAcceptedProposal.ToString();

            string acceptedValueStr = acceptedValue == null ?
                "<None>" :
                acceptedValue;

            return
                "Acked: " + lastAckedProposalStr + ", " +
                "Accepted: " + acceptedValueStr + "@" + lastAcceptedProposalStr;
        }

        // Learner event handlers
        public void LearnerInitOnEntry(NodeInitEvent initEvent)
        {
            this.acceptors = initEvent.Nodes;

            this.acceptorNames = new Dictionary<MachineId, string>();
            foreach (string name in acceptors.Keys)
            {
                this.acceptorNames[acceptors[name]] = name;
            }
        }

        public void ValueAcceptedEventHandler()
        {
            var acceptedEvent = (ValueAcceptedEvent)ReceivedEvent;

            var acceptor = acceptedEvent.Acceptor;
            var acceptedProposal = acceptedEvent.Proposal;
            var value = acceptedEvent.Value;

            acceptorToProposalMap[acceptor] = acceptedProposal;
            proposalToValueMap[acceptedProposal] = value;

            var proposalGroups = acceptorToProposalMap.Values.GroupBy(p => p);
            var majorityProposalList = proposalGroups.OrderByDescending(g => g.Count()).First();

            if (Utils.GreaterThan50PercentObservations(majorityProposalList.Count(), acceptors.Count))
            {
                var majorityProposal = majorityProposalList.First();
                var majorityValue = this.proposalToValueMap[majorityProposal];

                if (this.learnedValue == null)
                {
                    this.learnedValue = majorityValue;
                    Monitor<SafteyAndLivenessMonitor>(new ValueLearnedEvent());

                    if (this.proposerTimerId != null)
                    {
                        // Stop the proposer timer since we have learned a value
                        this.StopTimer(this.proposerTimerId);
                    }
                }
                else
                {
                    if (this.learnedValue == majorityValue)
                    {
                        Monitor<SafteyAndLivenessMonitor>(new ValueLearnedEvent());
                    }
                    else
                    {
                        Monitor<SafteyAndLivenessMonitor>(new ConflictingValuesLearnedEvent());
                    }
                }
            }
        }

        protected string GetLearnerStateInfo()
        {
            List<string> messages = new List<string>();

            foreach (MachineId mid in acceptorToProposalMap.Keys)
            {
                Proposal proposal = acceptorToProposalMap[mid];
                string value = proposalToValueMap[proposal];

                string message =
                    this.acceptorNames[mid] + ": " +
                    value + "@" + proposal.ToString();

                messages.Add(message);
            }

            return
                "Learned: " + (learnedValue == null ? "<None>" : learnedValue) + ", " +
                "Evidence: " + String.Join("; ", messages);
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventDoAction(typeof(ProposalRequest), nameof(ProposalRequestHandler))]
        [OnEventDoAction(typeof(AcceptRequest), nameof(AcceptRequestHandler))]
        [OnEventDoAction(typeof(HaltAcceptorEvent), nameof(HaltAcceptorEventHandler))]
        [OnEventDoAction(typeof(ValueAcceptedEvent), nameof(ValueAcceptedEventHandler))]
        [OnEventDoAction(typeof(ClientProposeValueRequest), nameof(ProposeValueRequestHandler))]
        [OnEventDoAction(typeof(ProposalResponse), nameof(ProposalResponseHandler))]
        [OnEventDoAction(typeof(TimerElapsedEvent), nameof(TimerElapsedEventHandler))]
        internal class Init : MachineState
        {
        }
    }
}
