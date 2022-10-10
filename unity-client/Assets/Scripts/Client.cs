using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkLibrary;

public class Client
{
    public string id = string.Empty;
    public bool run = false;
    Queue<object> sendQueue = new();
    Queue<object> receiveQueue = new();

    Socket server;
    Thread receiverThread;
    Thread senderThread;
    string host;
    int port;

    public Client(string host, int port)
    {
        this.host = host;
        this.port = port;
    }

    public Client(string host, int port, string id) : this(host, port)
    {
        Send(Command.SetName(id));
        this.id = id;
    }

    public void Connect()
    {
        run = true;
        Println("正在解析ip地址");
        if (!IPAddress.TryParse(host, out IPAddress ip))
        {
            IPHostEntry iPHostEntry = Dns.GetHostEntry(host);
            ip = iPHostEntry.AddressList[0];
        }
        Println($"ip: {ip}");
        IPEndPoint ipe = new(ip, port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Println("正在连接服务器");
        server.Connect(ip, port);
        receiverThread = new Thread(() => { ReceiveLoop(); });
        receiverThread.Start();
        senderThread = new Thread(() => { SendLoop(); });
        senderThread.Start();
        Println("等待服务器响应");
    }

    public void Send(object package)
    {
        sendQueue.Enqueue(package);
    }

    public object GetObject()
    {
        if (receiveQueue.Count > 0)
            return receiveQueue.Dequeue();
        else
            return null;
    }

    void SendLoop()
    {
        while (run)
        {
            if (sendQueue.Count > 0)
            {
                try
                {
                    object _object = sendQueue.Dequeue();
                    TCPTool.Send(server, _object);
                }
                catch (InsufficientBufferingException)
                {
                    receiveQueue.Enqueue("数据溢出");
                }
                catch (SocketException e)
                {
                    receiveQueue.Enqueue($"{e}");
                    Stop();
                    return;
                }
            }
        }
    }

    void ReceiveLoop()
    {
        while (run)
        {
            object _object = TCPTool.Receive(server);
            if(_object == null)
            {
                Stop();
            }
            receiveQueue.Enqueue(_object);
        }
    }

    public void Stop()
    {
        TCPTool.Close(server);
        run = false;
    }

    public void Println(string message)
    {
        receiveQueue.Enqueue(message);
    }
}