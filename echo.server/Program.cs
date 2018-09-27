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

                using (var client = server.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("Connected");
                    stream.Write(Encoding.ASCII.GetBytes("Greetings! Some salt for auth!\n"));
                    stream.Flush();
                    Console.WriteLine("Greetings send");

                    using (var reader = new StreamReader(stream, Encoding.ASCII))
                    {
                        Console.WriteLine("Received: " + reader.ReadLine());
                    }
                }
            }
        }
    }
}