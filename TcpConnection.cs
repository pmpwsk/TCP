using System.Net;
using System.Net.Sockets;
using System.Text;

namespace uwap.TCP;

/// <summary>
/// A TCP connection, either as a client to a server or from a server to a client.
/// </summary>
public class TcpConnection
{
    #region Events
    /// <summary>
    /// Event that is called when a new message has been received.
    /// </summary>
    public event MessageReceivedHandler? MessageReceived;

    /// <summary>
    /// Event that is called when the connection has been lost.
    /// </summary>
    public event ConnectionLostHandler? ConnectionLost;
    #endregion

    #region Public properties
    /// <summary>
    /// The IP address of the machine on the other end of the connection.
    /// </summary>
    public readonly IPAddress? RemoteAddress;

    /// <summary>
    /// The port of the machine on the other end of the connection.
    /// </summary>
    public readonly ushort? RemotePort;

    /// <summary>
    /// Whether this connection is still available or not.
    /// </summary>
    public bool Connected => Connection.Connected;
    #endregion

    #region Private properties
    /// <summary>
    /// The object for the TCP connection.
    /// </summary>
    private readonly TcpClient Connection;

    /// <summary>
    /// The stream to send text.
    /// </summary>
    private readonly StreamWriter Writer;

    /// <summary>
    /// The stream to receive text.
    /// </summary>
    private readonly StreamReader Reader;

    /// <summary>
    /// CTS to cancel the listener when disconnecting.
    /// </summary>
    private readonly CancellationTokenSource CancellationTokenSource;

    /// <summary>
    /// Lock so only one thread at a time can write and messages are being finished before disconnecting.
    /// </summary>
    private readonly ReaderWriterLockSlim Lock;
    #endregion

    #region Object creation
    /// <summary>
    /// Creates a new TCP connection object for an existing connection.
    /// </summary>
    internal static TcpConnection Accept(TcpClient connection)
    {
        return new(connection, true);
    }

    /// <summary>
    /// Creates a new TCP connection object to connect to the given server, the connection is started right away.
    /// </summary>
    public static TcpConnection Connect(string serverAddress, ushort serverPort, bool useThreadPool = false)
    {
        if (serverPort <= 0)
            throw new ArgumentOutOfRangeException("serverPort", "The port must be positive.");

        TcpClient connection = new();
        connection.Connect(serverAddress, serverPort);

        return new(connection, useThreadPool);
    }

    /// <summary>
    /// Creates a new TCP connection and starts listening to it.
    /// </summary>
    private TcpConnection(TcpClient connection, bool useThreadPool)
    {
        Connection = connection;

        Writer = new(connection.GetStream(), Encoding.UTF8);
        Reader = new(connection.GetStream(), Encoding.UTF8);

        CancellationTokenSource = new();
        Lock = new();

        if (Connection.Client.RemoteEndPoint != null && Connection.Client.RemoteEndPoint is IPEndPoint ipEndPoint)
        {
            RemoteAddress = ipEndPoint.Address;
            RemotePort = (ushort)ipEndPoint.Port;
        }

        if (useThreadPool)
            ThreadPool.QueueUserWorkItem(async x => await Listen());
        else new Thread(async () => await Listen()).Start();
    }
    #endregion

    #region Methods for existing connections
    /// <summary>
    /// Stops listening (which also ends and disposes the connection).
    /// </summary>
    public void Disconnect()
    {
        if (!Connection.Connected)
            throw new Exception("Already disconnected.");

        Lock.EnterWriteLock();
        try
        {
            CancellationTokenSource.Cancel();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Sends the given message to the machine at the end of the connection.
    /// </summary>
    public void Send(string message)
    {
        if (!Connection.Connected)
            throw new Exception("Not connected.");
        if (message.Contains('\n'))
            throw new Exception("The message must not contain any line breaks as that signals the end of a message. To use line breaks, you'll have to implement escape characters or something like base64 encoding on both ends.");

        Lock.EnterWriteLock();
        try
        {
            Writer.WriteLine(message);
            Writer.Flush();
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Listens for new connections and accepts them. If an error occurs (such as connection loss), the connection is seen as disconnected and gets disposed.
    /// </summary>
    private async Task Listen()
    {
        CancellationToken cancellationToken = CancellationTokenSource.Token;
        while(true)
        {
            try
            {
                string? message = await Reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (message == null || CancellationTokenSource.IsCancellationRequested)
                    break;
                MessageReceived?.Invoke(this, message);
            }
            catch
            {
                break;
            }
        }

        ConnectionLost?.Invoke(this);
        Writer.Close();
        Writer.Dispose();
        Reader.Close();
        Reader.Dispose();
        Connection.Close();
        Connection.Dispose();
        ConnectionLost = null;
        MessageReceived = null;
    }
    #endregion
}
