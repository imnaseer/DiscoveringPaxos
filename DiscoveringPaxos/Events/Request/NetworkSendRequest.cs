using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class NetworkSendRequest : Event
    {
        public NetworkSendRequest(MachineId from, MachineId to, Event eventObject)
        {
            this.From = from;
            this.To = to;
            this.Event = eventObject;
        }

        public MachineId From { get; private set; }

        public MachineId To { get; private set; }

        public Event Event { get; private set; }
    }
}
