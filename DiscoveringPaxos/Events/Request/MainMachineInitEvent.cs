using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace DiscoveringPaxos.Events.Request
{
    public class MainMachineInitEvent : Event
    {
        public MainMachineInitEvent(PSharpRuntime runtime)
        {
            this.Runtime = runtime;
        }

        public PSharpRuntime Runtime { get; private set; }

        public override string GetEventRepresentation()
        {
            return "main-machine-init";
        }
    }
}
