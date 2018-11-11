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
            foreach (var acceptor in acceptors)
            {
                Send(acceptor, new ProposalRequest(this.Id, new Proposal(this.name, proposalCounter++, value)));
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
    }
}
