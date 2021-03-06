﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using DiscoveringPaxos.Events.Request;

namespace DiscoveringPaxos.Machines
{
    public class MainMachine : Machine
    {
        private static int numProposers = 3;
        private static int numAcceptors = 5;
        private static int numLearners = 1;
        private static int maxAcceptorFailureCount = 2;

        private static Dictionary<string, MachineId> proposerNameToMachineId;
        private static Dictionary<string, MachineId> acceptorNameToMachineId;
        private static Dictionary<string, MachineId> learnerNameToMachineId;

        public void InitOnEntry()
        {
            var initEvent = (MainMachineInitEvent)ReceivedEvent;
            var runtime = initEvent.Runtime;

            var networkMachine = runtime.CreateMachine(typeof(NetworkMachine));

            proposerNameToMachineId = CreateMachineIds(
                runtime,
                typeof(Proposer),
                GetProposerName,
                numProposers);

            acceptorNameToMachineId = CreateMachineIds(
                runtime,
                typeof(Acceptor),
                GetAcceptorName,
                numAcceptors);

            learnerNameToMachineId = CreateMachineIds(
                runtime,
                typeof(Learner),
                GetLearnerName,
                numLearners);

            foreach (var name in proposerNameToMachineId.Keys)
            {
                runtime.CreateMachine(
                    proposerNameToMachineId[name],
                    typeof(Proposer),
                    new ProposerInitEvent(
                        name,
                        acceptorNameToMachineId,
                        networkMachine));
            }

            foreach (var name in acceptorNameToMachineId.Keys)
            {
                runtime.CreateMachine(
                    acceptorNameToMachineId[name],
                    typeof(Acceptor),
                    new AcceptorInitEvent(
                        name,
                        proposerNameToMachineId,
                        learnerNameToMachineId));
            }

            foreach (var name in learnerNameToMachineId.Keys)
            {
                runtime.CreateMachine(
                    learnerNameToMachineId[name],
                    typeof(Learner),
                    new LearnerInitEvent(
                        name,
                        acceptorNameToMachineId));
            }

            for (int i = 0; i < numProposers; i++)
            {
                var proposerName = GetProposerName(i);
                var value = GetValue(i);
                runtime.SendEvent(proposerNameToMachineId[proposerName], new ClientProposeValueRequest(null, value));
            }

            int failureCount = 0;
            for (int i = 0; i < numAcceptors; i++)
            {
                if (Random() && failureCount < maxAcceptorFailureCount)
                {
                    failureCount++;
                    Send(acceptorNameToMachineId[GetAcceptorName(i)], new HaltAcceptorEvent());
                }
            }
        }

        private static Dictionary<string, MachineId> CreateMachineIds(
          PSharpRuntime runtime,
          Type machineType,
          Func<int, string> machineNameFunc,
          int numMachines)
        {
            var result = new Dictionary<string, MachineId>();

            for (int i = 0; i < numMachines; i++)
            {
                var name = machineNameFunc(i);
                var machineId = runtime.CreateMachineIdFromName(machineType, name);
                result[name] = machineId;
            }

            return result;
        }

        private static string GetProposerName(int index)
        {
            return "p" + (index + 1);
        }

        private static string GetAcceptorName(int index)
        {
            return "a" + (index + 1);
        }

        private static string GetLearnerName(int index)
        {
            return "l" + (index + 1);
        }

        private static string GetValue(int index)
        {
            return "v" + (index + 1);
        }

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        internal class Init : MachineState
        {
        }
    }
}
