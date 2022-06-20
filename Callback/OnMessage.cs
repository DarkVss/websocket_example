using WebSocket_example.Communication.IO;

namespace WebSocket_example.Callback{
    /// <summary>
    /// Callback on new client message received. Can throw Exception (from this package), message was return to client as error.
    /// It's only check no processing logic
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="input"></param>
    public delegate void OnMessage(Connection connection, Input input);
}