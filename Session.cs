using System.Collections.Generic;
using WebSocket_example.Communication.IO;

namespace WebSocket_example{
    public class Session{
        private static Session _instance;

        protected readonly Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();

        protected Session(){ }

        public static Session Pool() => _instance ??= new Session();

        public void Add(Connection connection){
            this.Remove(connection.Uid, Message.CONNECTION_CLOSE_REGISTER_SAME_UID);

            this.Connections.Add(connection.Uid, connection);
        }

        public void Remove(string uid, string reason = null){
            if (Connections.ContainsKey(uid) == true){
                this.Connections[uid].Close(reason);
                this.Connections.Remove(uid);
            }
        }

        public void RemoveAll(){
            foreach (string key in this.Connections.Keys){
                this.Remove(key);
            }
        }

        public int ConnectionsCount(){
            return this.Connections.Count;
        }

        public void SendMessage(string connectionIdentifier, Output output){
            if (this.Connections.ContainsKey(connectionIdentifier) == true){
                this.Connections[connectionIdentifier].SendMessage(output);
            }
        }
    }
}