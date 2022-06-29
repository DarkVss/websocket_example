using WebSocket_example.Communication.IO;

namespace WebSocket_example.Communication{
    public abstract class Commands{
        protected readonly Input Input;
        protected readonly Connection Connection;

        public Commands(Connection connection,Input input){
            this.Input = input;
            this.Connection = connection;
        }

        /// <summary>
        /// Execute command. If something need to say client - say it inside
        /// </summary>
        public abstract void Execute();
    }
}