using WebSocket_example.Communication.IO;

namespace WebSocket_example.Communication{
    public abstract class Commands{
        protected readonly Input Input;
        protected readonly string ConnectionIdentifier;

        public Commands(string connectionIdentifier,Input input){
            this.Input = input;
            this.ConnectionIdentifier = connectionIdentifier;
        }

        /// <summary>
        /// Execute command. If something need to say client - say it inside
        /// </summary>
        public abstract void Execute();
    }
}