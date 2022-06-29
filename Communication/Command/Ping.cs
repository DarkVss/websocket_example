using WebSocket_example.Communication.IO;

namespace WebSocket_example.Communication.Command {
    public class Ping : Commands {
        public Ping(Connection connection, Input input) : base(connection, input) { }

        public override void Execute() {
            this.Connection.SendMessage(
                new Output().Status(true).Message("Pong")
            );
        }
    }
}