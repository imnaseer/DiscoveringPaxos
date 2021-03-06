﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Machines;

namespace DiscoveringPaxos.Events.Request
{
    public class AcceptorInitEvent : Event
    {
        public AcceptorInitEvent(string name, Dictionary<string, MachineId> proposers, Dictionary<string, MachineId> learners)
        {
            this.Name = name;
            this.Proposers = proposers;
            this.Learners = learners;
        }

        public string Name { get; private set; }

        public Dictionary<string, MachineId> Proposers { get; private set; }

        public Dictionary<string, MachineId> Learners { get; private set; }
    }
}
