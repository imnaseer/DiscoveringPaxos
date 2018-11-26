using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;

namespace DiscoveringPaxos.Machines
{
    public class NetworkMachine : Machine
    {
        public void NetworkSendRequestHandler()
        {
            var sendRequestEvent = (NetworkSendRequest)ReceivedEvent;

            var from = sendRequestEvent.From;
            var to = sendRequestEvent.To;
            var eventObject = sendRequestEvent.Event;

            Send(to, eventObject);
        }

        [Start]
        [OnEventDoAction(typeof(NetworkSendRequest), nameof(NetworkSendRequestHandler))]
        internal class Init : MachineState
        {
        }
    }
}
