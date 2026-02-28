# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

Each project is built/run independently from its own directory:

```bash
# Acceptor (server)
cd FixAcceptorServer && dotnet build && dotnet run

# Initiator (client) — start after the acceptor is listening
cd FixInitiatorClient && dotnet build && dotnet run
```

Start the acceptor first, then the client. There are no tests, linting, or CI configured.

## What This Is

A FIX 4.4 protocol client/server pair built with **QuickFIX/N** on **.NET 9**, communicating over `127.0.0.1:5001`.

- **FixAcceptorServer** — Listens on port 5001, receives `NewOrderSingle` (35=D) orders, and responds with `ExecutionReport` (35=8) fills.
- **FixInitiatorClient** — Connects to the acceptor, sends a `NewOrderSingle` on logon, and processes `ExecutionReport` responses.

Session identities: `FIX.4.4:SERVER->CLIENT` (acceptor) / `FIX.4.4:CLIENT->SERVER` (initiator).

## Architecture

Both projects follow the same two-file pattern:

**FixAcceptorServer/**
- **Program.cs** — Entry point. Reads `server.cfg`, wires up `ThreadedSocketAcceptor` with `FileStoreFactory`/`FileLogFactory`, and blocks until Enter or Ctrl+C.
- **FixServerApp.cs** — Implements `IApplication` + extends `MessageCracker`. Typed handler `OnMessage(NewOrderSingle, SessionID)` receives orders and sends back a filled `ExecutionReport` (hardcoded fill price 150.00).

**FixInitiatorClient/**
- **Program.cs** — Entry point. Reads `client.cfg`, wires up `SocketInitiator` with `FileStoreFactory`/`FileLogFactory`, and blocks until Enter or Ctrl+C.
- **FixClientApp.cs** — Implements `IApplication` + extends `MessageCracker`. Typed handler `OnMessage(ExecutionReport, SessionID)` prints fill details. `SendNewOrderSingle` fires automatically on logon.

## Key Files

- `FixAcceptorServer/server.cfg` — Acceptor session config (`SenderCompID=SERVER`, `TargetCompID=CLIENT`, `SocketAcceptPort=5001`)
- `FixInitiatorClient/client.cfg` — Initiator session config (`SenderCompID=CLIENT`, `TargetCompID=SERVER`, connects to `127.0.0.1:5001`)
- `*/FIX44.xml` — FIX 4.4 data dictionary (copied to output on build, same file in both projects)
- `DataDictionaries/` — Reference collection of FIX data dictionaries (FIX 4.0 through 5.0 SP2), not used by the build

## QuickFIX/N Conventions

- Add new message handlers as `public void OnMessage(MessageType msg, SessionID sid)` methods on the relevant app class (`FixServerApp` or `FixClientApp`). `MessageCracker.Crack()` dispatches automatically by MsgType tag.
- Session-level config goes in `server.cfg`/`client.cfg` under `[DEFAULT]` or `[SESSION]` sections. Add new sessions by adding `[SESSION]` blocks.
- The acceptor uses `ThreadedSocketAcceptor` (from `QuickFix` namespace); the initiator uses `SocketInitiator` (from `QuickFix.Transport`).
- NuGet packages: `QuickFIXn.Core` (engine) and `QuickFIXn.FIX44` (typed FIX 4.4 messages). To support other FIX versions, add the corresponding `QuickFIXn.FIXxx` package.

## Runtime Artifacts

Each project writes to `store/` and `log/` directories relative to its working directory (typically `bin/Debug/net9.0/`). These contain FIX session state and message logs. Delete the `store/` directory to reset sequence numbers.
