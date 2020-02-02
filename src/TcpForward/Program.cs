using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpForward
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse input
            string ip = args[0];
            int port = int.Parse(args[1]);
            string command = args[2];

            bool waitForResponse = args.Contains("w");
            string? filePath = null;
            if (args.Length > 3)
            {
                filePath = args[4];
            }

            string file = filePath == null ? "no file" : filePath;
            Console.WriteLine($"Parsed input as ip: {ip}, port: {port}, command: {command}, wait for response: {waitForResponse}, write to file: {file}");

            //Setup connection to device
            TcpClient tcpClient = new TcpClient(ip, port);
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = 200;

            //write
            var bytes = Encoding.ASCII.GetBytes(command + (char) 0x0D);
            networkStream.Write(bytes, 0, bytes.Length);

            string? response = null;
            if (waitForResponse)
            {
                Thread.Sleep(200);
                MemoryStream stream = new MemoryStream();
                while (networkStream.DataAvailable)
                {
                    var buffer = new byte[2048];
                    int respLength = networkStream.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, buffer.Length);
                    Thread.Sleep(50);
                }
                response = System.Text.Encoding.ASCII.GetString(stream.GetBuffer()).Replace("\0", string.Empty);
                Console.WriteLine($"Response: {response}");
            }

            if (filePath != null && response != null)
            {
                if (File.Exists(filePath))
                {
                    File.AppendAllText(filePath, response);
                }
                else
                {
                    Console.WriteLine("File doesn't exist - not writing.");
                }
            }

            tcpClient.Close();
        }
    }
}
