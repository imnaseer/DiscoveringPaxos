using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;
using DiscoveringPaxos.Spec;

namespace DiscoveringPaxos.Machines
{
    public class Learner : Machine
    {
        private string name;
        private Dictionary<string, MachineId> acceptors;
        private Dictionary<MachineId, string> acceptorNames;

        private Dictionary<MachineId, Proposal> acceptorToProposalMap = new Dictionary<MachineId, Proposal>();
        private Dictionary<Proposal, string> proposalToValueMap = new Dictionary<Proposal, string>();

        private string learnedValue = null;

        public void InitOnEntry()
        {
            var initEvent = (LearnerInitEvent)ReceivedEvent;

            this.name = initEvent.Name;
            this.acceptors = initEvent.Acceptors;

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

        protected override string GetStateInfo()
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
        [OnEventDoAction(typeof(ValueAcceptedEvent), nameof(ValueAcceptedEventHandler))]
        internal class Init : MachineState
        {
        }
    }
}
