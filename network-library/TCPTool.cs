using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkLibrary
{
    public class InsufficientBufferingException : Exception
    {
        public InsufficientBufferingException(string message) : base(message) { }
    }

    public static class TCPTool
    {
        public const int BUFFERSIZE = 1024;

        public static void Send(Socket socket, object pack)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream mStream = new MemoryStream())
            {
                formatter.Serialize(mStream, pack);
                mStream.Flush();
                byte[] buffer;
                if (mStream.Length > BUFFERSIZE)
                {
                    buffer = Encoding.UTF8.GetBytes($"{1}");
                    socket.Send(buffer, buffer.Length, SocketFlags.None);
                    throw new InsufficientBufferingException($"数据大小为{mStream.Length}字节；超过了上限{BUFFERSIZE}字节");
                }
                buffer = mStream.GetBuffer();
                socket.Send(buffer, (int)mStream.Length, SocketFlags.None);
                Console.WriteLine($"对{socket.RemoteEndPoint as IPEndPoint}传输了{mStream.Length}字节的数据");
            }
        }

        public static object Receive(Socket socket)
        {
            object result = null;
            byte[] buffer = new byte[BUFFERSIZE];
            int length = socket.Receive(buffer);
            if (length > 0)
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    mStream.Write(buffer, 0, BUFFERSIZE);
                    mStream.Flush();
                    mStream.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter formatter = new BinaryFormatter();
                    result = formatter.Deserialize(mStream);
                }
            }
            return result;
        }

        public static void Close(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}