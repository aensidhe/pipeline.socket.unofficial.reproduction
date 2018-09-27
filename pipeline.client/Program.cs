using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;

namespace pipeline.client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(IPAddress.Loopback, 3333);
                using (var sc = SocketConnection.Create(socket))
                {
                    WorkWithSc(sc).GetAwaiter().GetResult();
                }
            }
        }

        private static async Task WorkWithSc(SocketConnection sc)
        {
            var resultTask = sc.Input.ReadAsync();
            var result = resultTask.IsCompletedSuccessfully ? resultTask.Result : await resultTask;
            Console.WriteLine(Encoding.ASCII.GetString(result.Buffer.ToArray()));
            sc.Input.AdvanceTo(result.Buffer.End);
            
            var guid = Guid.NewGuid();
            var bytes = sc.Output.GetMemory(100);
            var length = Encoding.ASCII.GetBytes($"Some authentication packet + {guid}\n", bytes.Span);
            sc.Output.Advance(length);
            await Flush(sc.Output);
            Console.WriteLine("Auth sent:   " + guid);
        }
        
        private static ValueTask<bool> Flush(PipeWriter writer)
        {
            bool GetResult(FlushResult flush)
            // tell the calling code whether any more messages
            // should be written
                => !(flush.IsCanceled || flush.IsCompleted);

            async ValueTask<bool> Awaited(ValueTask<FlushResult> incomplete)
                => GetResult(await incomplete);

            // apply back-pressure etc
            var flushTask = writer.FlushAsync();

            return flushTask.IsCompletedSuccessfully
                ? new ValueTask<bool>(GetResult(flushTask.Result))
                : Awaited(flushTask);
        }
    }
}