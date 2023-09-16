namespace uwap.TCP;

/// <summary>
/// Delegate for methods that handle when a new connection has been received.
/// </summary>
public delegate void ConnectionReceivedHandler(TcpConnection connection);

/// <summary>
/// Delegate for methods that handle when a connection has been lost.
/// </summary>
public delegate void ConnectionLostHandler(TcpConnection connection);

/// <summary>
/// Delegate for methods that handle when a new message has been received.
/// </summary>
public delegate void MessageReceivedHandler(TcpConnection connection, string message);