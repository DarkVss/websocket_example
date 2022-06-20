using WebSocket_example.Communication.IO;

namespace WebSocket_example.Communication.Command{
    public class Test : Commands{
        public Test(string connectionIdentifier, Input input) : base(connectionIdentifier, input){ }

        public override void Execute(){
            Session.Pool().SendMessage(
                this.ConnectionIdentifier,
                new Output().Status(true).Message("Pong").Data(Input)
            );
        }
    }
}