#nullable enable
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocket_example.Callback;
using WebSocket_example.Communication;
using WebSocket_example.Communication.IO;

namespace WebSocket_example{
    public class Server{
        protected readonly IPAddress NetworkInterface;
        protected readonly int Port;

        public OnConnect? OnConnect;
        public OnStart? OnStart;
        public OnMessage? OnMessage;
        public OnClose? OnClose;
        public OnStop? OnStop;

        protected readonly TcpListener TcpServer;

        public Server(IPAddress networkInterface, int port = 8100){
            Routes.InitInstance();

            this.NetworkInterface = networkInterface;
            this.Port = port;

            this.TcpServer = new TcpListener(this.NetworkInterface, this.Port);
        }

        public void Start(){
            try{
                this.TcpServer.Start();
            } catch (SocketException e){
                throw new Exception($"Can't use this socket(port) - {e.Message}");
            }

            this.OnStart?.Invoke();

            while (true){
                ThreadPool.QueueUserWorkItem(this.AddConnection, this.TcpServer.AcceptTcpClient());
            }
        }

        public void Stop(){
            Session.Pool().RemoveAll();
            this.TcpServer.Stop();
            this.OnStop?.Invoke();
        }

        protected void AddConnection(object? data){
            if (data != null){
                TcpClient newClient = (TcpClient) data;
                NetworkStream newStream = newClient.GetStream();

                while (newStream.DataAvailable == false){
                    Thread.Sleep(100);
                }

                byte[] bytes = new byte[newClient.Available];
                try{
                    newStream.Read(bytes, 0, newClient.Available);
                } catch{
                    return;
                }

                Headers headers = Headers.Parse(bytes);

                if (headers.Url == null){
                    newStream.Close();
                    newClient.Close();

                    return;
                }

                // Handshake
                byte[] response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(
                                headers.Field("Sec-Websocket-Key") + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" // constant UID from RFC6455 section 1.3
                            )
                        )
                    ) + "\r\n\r\n"
                );
                newStream.Write(response, 0, response.Length);

                (bool status, string connectionData) = this.OnConnect?.Invoke(headers) ?? (true, null);
                if (string.IsNullOrEmpty(connectionData) == true){
                    connectionData = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
                }

                if (status == false){
                    Connection.SilentWrite(newClient.GetStream(), new Output().Message(connectionData));

                    newStream.Close();
                    newClient.Close();

                    return;
                }

                Session.Pool().Add(new Connection(newClient, connectionData, ref this.OnMessage, ref this.OnClose));
            }
        }
    }
}