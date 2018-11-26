
# Discovering Paxos through P&#35;

This repo contains source code for a Paxos implementation as described in [Paxos Made Simple](https://lamport.azurewebsites.net/pubs/paxos-simple.pdf) written in P#.

The source code is gradually derived from a simple attempt all the way to the complete protocol in the style of [Paxos from the ground up](http://imnaseer.net/paxos-from-the-ground-up.html). The incremental attempts can be seen in the commit history of this repo.

P#'s automated tester explores the non-determinism encoded in the Paxos implementation and points out the safety and liveness bugs in each version of the implementation.

# Running the tester

The P# tester can be run using the following command

```
PSharpTester.exe -test:DiscoveringPaxos.dll -i:1000 -max-steps:1000 -coverage:code
```

The code coverage parameter is useful to ensure all interesting parts of the implementation have been exercised by the tester. The P# tester emits reproducable schedules which can be used to replay buggy scenario as many times as required. The replayer can be run using teh following command:

```
PSharpReplayer.exe /test:DiscoveringPaxos.dll /replay:Output\DiscoveringPaxos.dll\PSharpTesterOutput\DiscoveringPaxos_0_0.schedule /break
```

