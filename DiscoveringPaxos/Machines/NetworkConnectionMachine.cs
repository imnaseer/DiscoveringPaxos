using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;

namespace DiscoveringPaxos.Machines
{
    public class NetworkConnectionMachine : Machine
    {
        public void NetworkSendRequestHandler()
        {
        }

        [Start]
        [OnEventDoAction(typeof(NetworkSendRequest), nameof(NetworkSendRequestHandler))]
        internal class Init : MachineState
        {
        }
    }
}
