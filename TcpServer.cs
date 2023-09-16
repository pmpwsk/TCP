using System.Net;
using System.Net.Sockets;

namespace uwap.TCP;

/// <summary>
/// The TCP server.
/// </summary>
public class TcpServer
{
    /// <summary>
    /// Event that is called when a new connection has been received.
    /// </summary>
    public event ConnectionReceivedHandler? ConnectionReceived;

    /// <summary>
    /// The thread that listens for new connections and accepts them.
    /// </summary>
    private readonly Thread ListenerThread;

    /// <summary>
    /// The port that should be listened on.
    /// </summary>
    private readonly ushort Port;

    /// <summary>
    /// Whether to support IPv6 or not.
    /// </summary>
    private readonly bool SupportIPv6;

    /// <summary>
    /// Creates a new TCP server that will listen on the given port.
    /// </summary>
    public TcpServer(ushort port, bool supportIPv6 = true)
    {
        if (port <= 0)
            throw new ArgumentOutOfRangeException("port", "The port must be positive.");

        Port = port;
        SupportIPv6 = supportIPv6;
        ListenerThread = new(async () => await Listen());
    }

    /// <summary>
    /// Starts listening.
    /// </summary>
    public void Start()
    {
        ListenerThread.Start();
    }

    /// <summary>
    /// Stops listening.
    /// </summary>
    public void Stop()
    {
        ListenerThread.Interrupt();
    }

    /// <summary>
    /// Listens for new connections, accepts them and calls ConnectionReceived.
    /// </summary>
    private async Task Listen()
    {
        TcpListener listener;
        if (SupportIPv6)
        {
            listener = new(IPAddress.IPv6Any, Port);
            listener.Server.DualMode = true;
        }
        else listener = new(IPAddress.Any, Port);

        listener.Start();

        while (true)
            ConnectionReceived?.Invoke(TcpConnection.Accept(await listener.AcceptTcpClientAsync()));
    }
}