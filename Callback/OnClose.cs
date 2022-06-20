namespace WebSocket_example.Callback{
    /// <summary>
    /// Callback before client connection closed
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="closedByClient">TRUE - connection was closed by client side, FALSE - server initiated</param>
    public delegate void OnClose(Connection connection, bool closedByClient);
}