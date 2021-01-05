using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ConfirmedToday_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            socket.Connect("127.0.0.1", 7777);

            if (socket.Connected)
                Console.WriteLine("서버 연결 완료");
            string message = string.Empty;
            while ((message = Console.ReadLine()) != "x")
            {
                try
                {
                    byte[] sendBuff = Encoding.UTF8.GetBytes(message);
                    socket.Send(sendBuff);
                    byte[] buff = new byte[16384];
                    int n = socket.Receive(buff);

                    string result = Encoding.UTF8.GetString(buff, 0, n);
                    Console.WriteLine("서버로부터 응답 : \n{0}", result);
                }
                catch (IOException e)
                {
                    if (!socket.Connected)
                    {
                        Console.WriteLine($"서버 연결 종료 : {e.Message}");
                    }
                }
            }
        }
    }
}
