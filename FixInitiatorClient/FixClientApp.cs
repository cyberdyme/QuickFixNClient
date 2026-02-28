using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace FixInitiatorClient;

/// <summary>
/// FIX 4.4 client application that implements QuickFix IApplication callbacks
/// and uses MessageCracker for typed message dispatch.
/// </summary>
public class FixClientApp : MessageCracker, IApplication
{
    public void OnCreate(SessionID sessionId)
    {
        Console.WriteLine($"[OnCreate] Session created: {sessionId}");
    }

    public void OnLogon(SessionID sessionId)
    {
        Console.WriteLine($"[OnLogon] Logged on: {sessionId}");
        SendNewOrderSingle(sessionId);
    }

    public void OnLogout(SessionID sessionId)
    {
        Console.WriteLine($"[OnLogout] Logged out: {sessionId}");
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
    /// Typed handler for ExecutionReport (35=8).
    /// MessageCracker dispatches here automatically from Crack().
    /// </summary>
    public void OnMessage(ExecutionReport report, SessionID sessionId)
    {
        Console.WriteLine("============================================================");
        Console.WriteLine("[ExecutionReport] Received 35=8");
        Console.WriteLine($"  ClOrdID   : {report.ClOrdID.Value}");
        Console.WriteLine($"  ExecType  : {report.ExecType.Value}");
        Console.WriteLine($"  OrdStatus : {report.OrdStatus.Value}");

        if (report.IsSetCumQty())
            Console.WriteLine($"  CumQty    : {report.CumQty.Value}");

        if (report.IsSetAvgPx())
            Console.WriteLine($"  AvgPx     : {report.AvgPx.Value}");

        if (report.IsSetLastQty())
            Console.WriteLine($"  LastQty   : {report.LastQty.Value}");

        if (report.IsSetLastPx())
            Console.WriteLine($"  LastPx    : {report.LastPx.Value}");

        Console.WriteLine("============================================================");
    }

    /// <summary>
    /// Sends a single NewOrderSingle (35=D) immediately after logon.
    /// </summary>
    private static void SendNewOrderSingle(SessionID sessionId)
    {
        var order = new NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString("N")),
            new Symbol("AAPL"),
            new Side(Side.BUY),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.MARKET)
        );

        order.Set(new OrderQty(100m));

        Console.WriteLine($"[SendOrder] Sending NewOrderSingle: Symbol=AAPL, Side=BUY, Qty=100, OrdType=MARKET, ClOrdID={order.ClOrdID.Value}");

        bool sent = Session.SendToTarget(order, sessionId);
        Console.WriteLine(sent
            ? "[SendOrder] Order sent successfully."
            : "[SendOrder] FAILED to send order.");
    }
}
