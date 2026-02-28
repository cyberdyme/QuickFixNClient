# QuickFixNClient

FIX 4.4 initiator (client) and acceptor (server) built with [QuickFIX/N](https://github.com/connamara/quickfixn) on .NET 9.

The client connects to the server on `127.0.0.1:5001`, sends a `NewOrderSingle` (35=D) on logon, and the server responds with an `ExecutionReport` (35=8) fill.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Quick Start

Start the acceptor first, then the client in a separate terminal:

```bash
# Terminal 1 — Server
cd FixAcceptorServer
dotnet run
```

```bash
# Terminal 2 — Client
cd FixInitiatorClient
dotnet run
```

## Project Structure

| Directory | Description |
|---|---|
| `FixAcceptorServer/` | FIX acceptor — listens on port 5001, receives orders, sends execution reports |
| `FixInitiatorClient/` | FIX initiator — connects to the acceptor, sends a market order on logon |
| `DataDictionaries/` | Reference FIX data dictionaries (FIX 4.0 through 5.0 SP2) |

## Example Output

**Server:**
```
[OnLogon] Client logged on: FIX.4.4:SERVER->CLIENT
[NewOrderSingle] Received 35=D
  ClOrdID : 6a7c4ac3db5e4e93b143e6a652982a44
  Symbol  : AAPL
  Side    : 1
  Qty     : 100
  OrdType : 1
[SendExecReport] Sending ExecutionReport: ClOrdID=6a7c4ac3db5e4e93b143e6a652982a44, ExecType=FILL, FillPx=150.00, FillQty=100
```

**Client:**
```
[OnLogon] Logged on: FIX.4.4:CLIENT->SERVER
[SendOrder] Sending NewOrderSingle: Symbol=AAPL, Side=BUY, Qty=100, OrdType=MARKET
[ExecutionReport] Received 35=8
  ClOrdID   : 6a7c4ac3db5e4e93b143e6a652982a44
  ExecType  : 2
  OrdStatus : 2
  CumQty    : 100
  AvgPx     : 150.00
  LastQty   : 100
  LastPx    : 150.00
```

## Configuration

Session settings are in `client.cfg` and `server.cfg` respectively. Key settings:

| Setting | Client | Server |
|---|---|---|
| ConnectionType | initiator | acceptor |
| SenderCompID | CLIENT | SERVER |
| TargetCompID | SERVER | CLIENT |
| Port | connects to 5001 | listens on 5001 |
| HeartBtInt | 30s | 30s |

## Troubleshooting

- **Sequence number mismatch** — Delete the `store/` directory in the project's output folder (`bin/Debug/net9.0/store/`) to reset.
- **Connection refused** — Make sure the acceptor is running before starting the client.
