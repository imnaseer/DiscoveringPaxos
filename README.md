
# Discovering Paxos through P&#35;

This repo contains source code for a Paxos implementation as described in [Paxos Made Simple](https://lamport.azurewebsites.net/pubs/paxos-simple.pdf) written in P#.

The source code is gradually derived from a simple attempt all the way to the complete protocol in the style of [Paxos from the ground up](http://imnaseer.net/paxos-from-the-ground-up.html). The incremental attempts can be seen in the commit history of this repo.

P#'s automated tester explores the non-determinism encoded in the Paxos implementation and points out the safety and liveness bugs in each version of the implementation.

# Pulling in the P&#35; dependency

The features this project requires from P# for trace visualizer are not part of any released PSharp version but part of an experimental feature project. This project thus adds that experimental branch in the P# repo as a submodule.

Please make sure to pull in the PSharp submodule code after cloning by running the following commands:

```
git submodule init
git submodule update
```

You'll then have to build P# by opening a powershell window and invoking build.ps1 in PSharp\Scripts directory. You'll be able to build this project successfully after the above steps have been executed.

# Running the tester

The P# tester can be run using the following command from the bin\Debug or bin\Release directory

```
..\..\..\PSharp\bin\net46\PSharpTester.exe -test:DiscoveringPaxos.dll -i:10000 -max-steps:1000 -sch:pct:10
```

The code coverage parameter is useful to ensure all interesting parts of the implementation have been exercised by the tester. The P# tester emits reproducable schedules which can be used to replay buggy scenario as many times as required. The replayer can be run using teh following command:

```
..\..\..\PSharp\bin\net46\PSharpTester.exe\PSharpReplayer.exe /test:DiscoveringPaxos.dll /replay:Output\DiscoveringPaxos.dll\PSharpTesterOutput\DiscoveringPaxos_0_0.schedule /break
```

