using System;
using System.IO;
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

            Console.WriteLine($"Parsed input as ip: {ip}, port: {port} and command: {command}");

            //Setup connection to device
            TcpClient tcpClient = new TcpClient(ip, port);
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = 200;

            //write
            var bytes = Encoding.ASCII.GetBytes(command + (char) 0x0D);
            networkStream.Write(bytes, 0, bytes.Length);

            tcpClient.Close();
        }
    }
}
