using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace echo.server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new TcpListener(IPAddress.Loopback, 3333);
            server.Start();
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");

                try
                {
                    using (var client = server.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine("Connected");
                        stream.Write(Encoding.ASCII.GetBytes("Greetings! Some salt for auth!\n"));
                        stream.Flush();
                        Console.WriteLine("Greetings send");

                        var buffer = new byte[10000];
                        var read = stream.Read(buffer);
                        var s = Encoding.ASCII.GetString(buffer.AsSpan(0, read));
                        var strings = s.Split(" + ");
                        if (strings.Length > 1 && Guid.TryParse(strings[1], out var id))
                        {
                            Console.WriteLine(s);

                            stream.Write(Encoding.ASCII.GetBytes($"Auth is ok + {id}!\n"));
                            stream.Flush();
                        }
                        else
                        {
                            Console.WriteLine("Auth failed!\n");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}