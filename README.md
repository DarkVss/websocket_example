<h1>Simple websocket server</h1>

<hr>

<h2>Features</h2>

- Multi-client (only one connection with unique Identifier)
- Personal messsage queue for each connection
- Commands implements by classes (eg. [Ping class](Communication/Command/Ping.cs))
- Communication by JSON-docs
- Colored console logger
- Callbacks: server start, new connection, new message for connection, server stopped, connection closed
