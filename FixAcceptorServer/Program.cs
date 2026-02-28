using QuickFix;
using QuickFix.Store;
using QuickFix.Logger;
namespace FixAcceptorServer;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== FIX 4.4 Acceptor Server ===");
        Console.WriteLine("Loading configuration from server.cfg...");

        var settings = new SessionSettings("server.cfg");
        var app = new FixServerApp();
        var storeFactory = new FileStoreFactory(settings);
        var logFactory = new FileLogFactory(settings);
        var acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);

        Console.BackgroundColor = ConsoleColor.DarkBlue;
        try { Console.Clear(); } catch (IOException) { }
        Console.WriteLine("******** FIX Acceptor Server ****************");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\nCtrl+C detected. Shutting down...");
            cts.Cancel();
        };

        try
        {
            acceptor.Start();
            Console.WriteLine("Acceptor started. Listening on port 5001...");
            Console.WriteLine("Press ENTER or Ctrl+C to quit.\n");

            var readTask = Task.Run(() => Console.ReadLine());
            Task.WaitAny(readTask, Task.Delay(Timeout.Infinite, cts.Token)
                .ContinueWith(_ => { }, TaskScheduler.Default));
        }
        catch (OperationCanceledException)
        {
            // Expected on Ctrl+C
        }
        finally
        {
            Console.WriteLine("Stopping acceptor...");
            acceptor.Stop();
            Console.WriteLine("Acceptor stopped. Goodbye.");
        }
    }
}
