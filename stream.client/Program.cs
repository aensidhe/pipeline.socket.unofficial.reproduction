using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace stream.client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(IPAddress.Loopback, 3333);
                using (var stream = new NetworkStream(socket))
                {
                    using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
                        Console.WriteLine(reader.ReadLine());

                    var guid = Guid.NewGuid();
                    stream.Write(Encoding.ASCII.GetBytes($"Some authentication packet + {guid}\n"));
                    Console.WriteLine("Auth sent:   " + guid);

                    using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
                        Console.WriteLine(reader.ReadLine());
                }
            }
        }
    }
}