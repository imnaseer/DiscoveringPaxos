using System;
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
        private static int numNodes = 3;
        private static int maxAcceptorFailureCount = 0;

        private static Dictionary<string, MachineId> nodeNameToMachineId;

        public void InitOnEntry()
        {
            var initEvent = (MainMachineInitEvent)ReceivedEvent;
            var runtime = initEvent.Runtime;

            nodeNameToMachineId = CreateMachineIds(
                runtime,
                typeof(Node),
                GetNodeName,
                numNodes);

            foreach (var name in nodeNameToMachineId.Keys)
            {
                runtime.CreateMachine(
                    nodeNameToMachineId[name],
                    typeof(Node),
                    new NodeInitEvent(
                        name,
                        nodeNameToMachineId));
            }

            for (int i = 0; i < numNodes; i++)
            {
                var nodeName = GetNodeName(i);
                var value = GetValue(i);
                runtime.SendEvent(nodeNameToMachineId[nodeName], new ClientProposeValueRequest(null, value));
            }

            int failureCount = 0;
            for (int i = 0; i < numNodes; i++)
            {
                if (Random() && failureCount < maxAcceptorFailureCount)
                {
                    failureCount++;
                    Send(nodeNameToMachineId[GetNodeName(i)], new HaltAcceptorEvent());
                }
            }
        }

        public override string ToString()
        {
            return "main";
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

        private static string GetNodeName(int index)
        {
            return "n" + (index + 1);
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
