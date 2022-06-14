﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace WebSocket_example{
    internal class Program{
        public static void Main(string[] args){
            Console.WriteLine("Initialization...");
            string host = "127.0.0.1";
            int port = 8100;

            var server = new TcpListener(IPAddress.Parse(host), port);

            server.Start();
            Console.WriteLine("Server has started on {0}:{1}, Waiting for a connection...", host, port);

            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("A client connected.");

            NetworkStream stream = client.GetStream();

            // enter to an infinite cycle to be able to handle every change in stream
            while (true){
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ; // match against "get"

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);
                string s = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase)){
                    Console.WriteLine("=====Handshaking from client=====\n{0}", s);

                    // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                    // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                    // 3. Compute SHA-1 and Base64 hash of the new value
                    // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                    string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                    string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                    string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                    // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                    byte[] response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                    stream.Write(response, 0, response.Length);
                } else{
                    bool fin = (bytes[0] & 0b10000000) != 0,
                        mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"
                    int opcode = bytes[0] & 0b00001111; // expecting 1 - text message
                    int offset = 2;
                    int msglen = bytes[1] & 0b01111111;

                    if (msglen == 126){
                        // bytes are reversed because websocket will print them in Big-Endian, whereas
                        // BitConverter will want them arranged in little-endian on windows
                        msglen = BitConverter.ToUInt16(new byte[]{bytes[3], bytes[2]}, 0);
                        offset = 4;
                    } else if (msglen == 127){
                        // To test the below code, we need to manually buffer larger messages — since the NIC's autobuffering
                        // may be too latency-friendly for this code to run (that is, we may have only some of the bytes in this
                        // websocket frame available through client.Available).
                        msglen = (int) BitConverter.ToUInt64(
                            new byte[]{bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2]}, 0);
                        offset = 10;
                    }

                    if (msglen == 0){
                        Console.WriteLine("msglen == 0");
                    } else if (mask){
                        byte[] decoded = new byte[msglen];
                        byte[] masks = new byte[4]{bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]};
                        offset += 4;

                        for (int i = 0; i < msglen; ++i)
                            decoded[i] = (byte) (bytes[offset + i] ^ masks[i % 4]);

                        string text = Encoding.UTF8.GetString(decoded);
                        Console.WriteLine("{0}\n", text);

                        Queue<string>
                            que = new Queue<string>(SplitTexInGroups(text,
                                125)); //Make it so the message is never longer then 125 (Split the message into parts & store them in a queue)
                        int len = que.Count;


                        while (que.Count > 0){
                            var header = GetHeader(
                                que.Count > 1 ? false : true,
                                que.Count == len ? false : true
                            ); //Get the header for a part of the queue

                            byte[] list = Encoding.UTF8.GetBytes(que.Dequeue()); //Get part of the message out of the queue
                            header = (header << 7) + list.Length; //Add the length of the part we are going to send
                            byte[] h = IntToByteArray((ushort) header);
                            //Send the header & message to client
                            stream.Write(h, 0, h.Length);
                            stream.Write(list, 0, list.Length);
                        }
                    } else{
                        Console.WriteLine("mask bit not set");
                    }
                }
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

        protected static int GetHeader(bool finalFrame, bool contFrame){
            int header = finalFrame ? 1 : 0; //fin: 0 = more frames, 1 = final frame
            header = (header << 1) + 0; //rsv1
            header = (header << 1) + 0; //rsv2
            header = (header << 1) + 0; //rsv3
            header = (header << 4) + (contFrame ? 0 : 1); //opcode : 0 = continuation frame, 1 = text
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