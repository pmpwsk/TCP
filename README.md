# TCP library
Cross-platform .NET library written in C# that allows you to easily create server-client communication using TCP messages.

The messages consist of text only (encoded in UTF-8) and end with a line break, so if you want to send bytes or a message with line breaks, you'll have to implement escape characters or something like base64 encoding on both ends.

Website: https://uwap.org/projects/tcp

Guides: https://uwap.org/guides/tcp

## Main features
- Server listening for clients
- Clients connecting to a server
- Events: ConnectionReceived, ConnectionLost, MessageReceived
- Using the .NET thread pool
- Server dual-mode (listening on IPv4 and IPv6 at once without 
- Same connection class for server and clientstwo listener threads)
- Locks so threads don't write to a connection at the same time
- Automatic splitting of traffic into individual messages encoded in UTF-8 (no line breaks, see above!)
- Extracting the IP address and port of the machine on the other end of connections

## Installation
You can get the NuGet package here: [uwap.TCP](https://www.nuget.org/packages/uwap.TCP/)

You can also download the source code from GitHub and add a reference to it from your project.
