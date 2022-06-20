<h1>Simple websocket server</h1>

<hr>

<h2>Features</h2>
<ul>
<li>Multi-client (only one connection with unique Identifier)</li>
<li>Personal messsage queue for each connection</li>
<li>Commands implements by classes (eg. [Ping class](Communication/Command/Ping.cs))</li>
<li>Communication by JSON-docs</li>
<li>Colored console logger</li>
<li>Callbacks: server start, new connection, new message for connection, server stopped, connection closed</li>
</ul>