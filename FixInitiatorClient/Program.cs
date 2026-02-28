using QuickFix;
using QuickFix.Store;
using QuickFix.Logger;
using QuickFix.Transport;

namespace FixInitiatorClient;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== FIX 4.4 Initiator Client ===");
        Console.WriteLine("Loading configuration from client.cfg...");

        var settings = new SessionSettings("client.cfg");
        var app = new FixClientApp();
        var storeFactory = new FileStoreFactory(settings);
        var logFactory = new FileLogFactory(settings);
        var initiator = new SocketInitiator(app, storeFactory, settings, logFactory);

        // Set background color to purple
        Console.BackgroundColor = ConsoleColor.DarkGreen; // Closest to purple

        // Clear the console to apply the color to the entire screen
        try { Console.Clear(); } catch (IOException) { }
        Console.WriteLine("******** FIX Server ****************");


        // Handle Ctrl+C for graceful shutdown
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\nCtrl+C detected. Shutting down...");
            cts.Cancel();
        };

        try
        {
            initiator.Start();
            Console.WriteLine("Initiator started. Connecting to 127.0.0.1:5001...");
            Console.WriteLine("Press ENTER or Ctrl+C to quit.\n");

            // Block until ENTER is pressed or Ctrl+C is received
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
            Console.WriteLine("Stopping initiator...");
            initiator.Stop();
            Console.WriteLine("Initiator stopped. Goodbye.");
        }
    }
}
