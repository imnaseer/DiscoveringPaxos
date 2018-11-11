using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using Microsoft.PSharp.TestingServices;
using DiscoveringPaxos.Events.Request;
using DiscoveringPaxos.Machines;
using DiscoveringPaxos.Spec;

namespace DiscoveringPaxos
{
    public class DiscoveringPaxos
    {
        [Microsoft.PSharp.Test]
        public static void PaxosTest(PSharpRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(SafetyMonitor));
            runtime.CreateMachine(typeof(MainMachine), new MainMachineInitEvent(runtime));

            //var config = Configuration.Create().WithDebuggingEnabled(true);
            //config.SchedulingIterations = 100;
            //config.MaxSchedulingSteps = 1000;

            //var report = TestingEngineFactory.CreateBugFindingEngine(config, (runtime) =>
            //{
            //    runtime.RegisterMonitor(typeof(SafetyMonitor));
            //    runtime.CreateMachine(typeof(MainMachine), new MainMachineInitEvent(runtime));
            //}).Run().TestReport;

            //Console.WriteLine(report.NumOfFoundBugs);
        }
    }
}
