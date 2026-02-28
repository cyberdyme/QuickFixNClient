using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace FixAcceptorServer;

/// <summary>
/// FIX 4.4 acceptor application that receives NewOrderSingle messages
/// and responds with ExecutionReport fills.
/// </summary>
public class FixServerApp : MessageCracker, IApplication
{
    public void OnCreate(SessionID sessionId)
    {
        Console.WriteLine($"[OnCreate] Session created: {sessionId}");
    }

    public void OnLogon(SessionID sessionId)
    {
        Console.WriteLine($"[OnLogon] Client logged on: {sessionId}");
    }

    public void OnLogout(SessionID sessionId)
    {
        Console.WriteLine($"[OnLogout] Client logged out: {sessionId}");
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionId)
    {
        Console.WriteLine($"[ToAdmin] >> {message.Header.GetString(Tags.MsgType)}");
    }

    public void FromAdmin(QuickFix.Message message, SessionID sessionId)
    {
        Console.WriteLine($"[FromAdmin] << {message.Header.GetString(Tags.MsgType)}");
    }

    public void ToApp(QuickFix.Message message, SessionID sessionId)
    {
        Console.WriteLine($"[ToApp] >> {message.Header.GetString(Tags.MsgType)}");
    }

    public void FromApp(QuickFix.Message message, SessionID sessionId)
    {
        Console.WriteLine($"[FromApp] << {message.Header.GetString(Tags.MsgType)}");
        Crack(message, sessionId);
    }

    /// <summary>
    /// Typed handler for NewOrderSingle (35=D).
    /// Receives the order and sends back a filled ExecutionReport.
    /// </summary>
    public void OnMessage(NewOrderSingle order, SessionID sessionId)
    {
        var symbol = order.Symbol.Value;
        var side = order.Side.Value;
        var qty = order.OrderQty.Value;
        var clOrdId = order.ClOrdID.Value;

        Console.WriteLine("============================================================");
        Console.WriteLine($"[NewOrderSingle] Received 35=D");
        Console.WriteLine($"  ClOrdID : {clOrdId}");
        Console.WriteLine($"  Symbol  : {symbol}");
        Console.WriteLine($"  Side    : {side}");
        Console.WriteLine($"  Qty     : {qty}");
        Console.WriteLine($"  OrdType : {order.OrdType.Value}");
        Console.WriteLine("============================================================");

        // Send back a fill
        decimal fillPrice = 150.00m;

        var execReport = new ExecutionReport(
            new OrderID(Guid.NewGuid().ToString("N")),
            new ExecID(Guid.NewGuid().ToString("N")),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            order.Symbol,
            new Side(side),
            new LeavesQty(0m),
            new CumQty(qty),
            new AvgPx(fillPrice)
        );

        execReport.Set(order.ClOrdID);
        execReport.Set(order.Symbol);
        execReport.Set(new LastQty(qty));
        execReport.Set(new LastPx(fillPrice));

        Console.WriteLine($"[SendExecReport] Sending ExecutionReport: ClOrdID={clOrdId}, ExecType=FILL, FillPx={fillPrice}, FillQty={qty}");

        bool sent = Session.SendToTarget(execReport, sessionId);
        Console.WriteLine(sent
            ? "[SendExecReport] ExecutionReport sent successfully."
            : "[SendExecReport] FAILED to send ExecutionReport.");
    }
}
