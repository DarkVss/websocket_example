#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebSocket_example.Callback;
using WebSocket_example.Communication;
using WebSocket_example.Communication.IO;

namespace WebSocket_example{
    public class Connection{
        protected readonly TcpClient Client;
        protected readonly NetworkStream Stream;
        protected readonly OnMessage? OnMessage;
        protected readonly OnClose? OnClose;
        public string Uid{ get; }

        protected readonly CancellationTokenSource CancellationTokenReceiver;
        protected readonly CancellationTokenSource CancellationTokenDispatcher;

        protected Queue<Output> DispatchQueue = new Queue<Output>();

        public Connection(TcpClient tcpClient, string uid, ref OnMessage? onMessage, ref OnClose? onClose){
            this.Client = tcpClient;
            this.Stream = this.Client.GetStream();
            this.Uid = uid;

            this.OnMessage = onMessage;
            this.OnClose = onClose;

            this.CancellationTokenReceiver = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(this.Receiver, this.CancellationTokenReceiver.Token);

            this.CancellationTokenDispatcher = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(this.Dispatcher, this.CancellationTokenDispatcher.Token);
        }

        protected void Receiver(object? state){
            if (state != null){
                CancellationToken token = (CancellationToken) state;
                while (token.IsCancellationRequested == false){
                    while (this.Stream.DataAvailable == false){
                        Thread.Sleep(100);
                        if (token.IsCancellationRequested == true){
                            return;
                        }
                    }

                    byte[] bytes = new byte[this.Client.Available];
                    this.Stream.Read(bytes, 0, this.Client.Available);

                    switch (bytes[0]){
#region Protocol commands
                        case 0x88:{
                            Session.Pool().Remove(this.Uid);
                        }
                            return;
#endregion
#region Communications
                        case 0x81:{
                            int offset = 2;
                            int msglen = bytes[1] & 0b01111111;

                            if (msglen == 126){
                                msglen = BitConverter.ToUInt16(new byte[]{bytes[3], bytes[2]}, 0);
                                offset = 4;
                            } else if (msglen == 127){
                                msglen = (int) BitConverter.ToUInt64(
                                    new byte[]{bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2]}, 0);
                                offset = 10;
                            }

                            if (msglen != 0){
                                byte[] decoded = new byte[msglen];
                                byte[] masks = new byte[4]{bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]};
                                offset += 4;

                                for (int i = 0; i < msglen; ++i){
                                    decoded[i] = (byte) (bytes[offset + i] ^ masks[i % 4]);
                                }

                                string message = Encoding.UTF8.GetString(decoded);

                                try{
                                    Input input = Input.Parse(message);
                                    if (input.IsValid == false){
                                        throw new Exception(Message.COMMUNICATION_INVALID_MESSAGE);
                                    }

                                    this.OnMessage?.Invoke(this, input);

                                    Routes.GetInstance().Execute(this.Uid,input);
                                } catch (Exception e){
                                    this.SilentWrite(e.Message);
                                }
                            }
                        }
                            break;
#endregion
#region Unknown commands
                        default:{
                            this.SilentWrite(Message.COMMUNICATION_UNKNOWN_PROTOCOL_COMMAND);
                        }
                            break;
#endregion
                    }
                }
            }
        }

        protected void Dispatcher(object? state){
            if (state != null){
                CancellationToken token = (CancellationToken) state;
                while (token.IsCancellationRequested == false){
                    while (this.DispatchQueue.Count == 0){
                        Thread.Sleep(100);
                        if (token.IsCancellationRequested == true){
                            return;
                        }
                    }

                    while (this.DispatchQueue.Count != 0){
                        if (token.IsCancellationRequested == true){
                            return;
                        }

                        this.SilentWrite(this.DispatchQueue.Dequeue());
                    }
                }
            }
        }

        public void SendMessage(Output output){
            this.DispatchQueue.Enqueue(output);
        }

        public void Close(string? reason = null){
            this.CancellationTokenReceiver.Cancel();
            this.CancellationTokenDispatcher.Cancel();

            if (reason != null){
                this.SilentWrite(reason);
            }

            this.OnClose?.Invoke(this, true);

            this.Stream.Close();
            this.Client.Close();
        }

        protected void SilentWrite(string message){
            SilentWrite(this.Stream, new Output().Message(message));
        }

        protected void SilentWrite(Output output){
            SilentWrite(this.Stream, output);
        }


        public static void SilentWrite(NetworkStream stream, Output output){
            try{
                Queue<string> messageParts = new Queue<string>(SplitTexInGroups(output.ToJson(), 125));
                int packageCount = messageParts.Count;

                while (messageParts.Count > 0){
                    // Forming the header for a part of the queue
                    var header = GetHeader(messageParts.Count == 1, messageParts.Count != packageCount);

                    byte[] messagePart = Encoding.UTF8.GetBytes(messageParts.Dequeue());
                    header = (header << 7) + messagePart.Length;
                    byte[] h = IntToByteArray((ushort) header);

                    //Send the header & message to client
                    stream.Write(h, 0, h.Length);
                    stream.Write(messagePart, 0, messagePart.Length);
                }
            } catch (System.IO.IOException e){
                Logger.Error($"Failed write: {e}");
            }
        }

        public static IEnumerable<string> SplitTexInGroups(string original, int size){
            var p = 0;
            var l = original.Length;
            while (l - p > size){
                yield return original.Substring(p, size);
                p += size;
            }

            yield return original.Substring(p);
        }

        protected static int GetHeader(bool isFinalFrame, bool isContinuationFrame){
            int header = isFinalFrame ? 1 : 0; //fin: 0 = more frames, 1 = final frame
            header = (header << 1) + 0; //rsv1
            header = (header << 1) + 0; //rsv2
            header = (header << 1) + 0; //rsv3
            header = (header << 4) + (isContinuationFrame ? 0 : 1); //opcode : 0 = continuation frame, 1 = text
            header = (header << 1) + 0; //mask: server -> client = no mask

            return header;
        }

        protected static byte[] IntToByteArray(ushort value){
            var ary = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian){
                Array.Reverse(ary);
            }

            return ary;
        }
    }
}