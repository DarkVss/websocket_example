using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket_example.Communication;
using WebSocket_example.Communication.IO;

namespace WebSocket_example{
    internal static class Program{
        private static readonly IPAddress Host = IPAddress.Any;
        private const int Port = 8100;

        public static void Main(string[] args){
            Server server = new Server(Host, Port){
                // The server was successfully started
                OnStart = () => { Logger.Success($"Server ready"); },
                // New client connected. Return ( status[true/false], additionalData[connectionIdentifier/Message error] )
                OnConnect = (headers) => {
                    string connectionIdentifier = headers.Cookie("PHPSESSID");

                    Logger.Info($"New Connection. Connection Identifier: `{connectionIdentifier}`");

                    return (true, connectionIdentifier);
                },
                // New message from client. jst reaction what message is valid json
                OnMessage = (connection, input) => {
                    Logger.Info($"New message from `{connection.Uid}`:\n\t`{input}`");
                },
                // Before connection close
                OnClose = (connection, closedByClient) => { Logger.Info($"Connection closed by `{(closedByClient ? "client" : "server")}`"); },
                // Server is stopped
                OnStop = () => { Logger.Success($"Server stopped"); },
            };

            try{
                server.Start();
            } catch (Exception e){
                Logger.Error($"Fail: {e}");
            }
        }
    }
}