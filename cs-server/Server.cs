using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Server
    {
        public bool run = true;

        Thread listenerThread;
        Thread handlerThread;

        readonly Dictionary<string, ClientHandler> clients;
        readonly Queue<object> log = new Queue<object>();
        readonly string host;
        readonly int port;
        readonly Executer executer;

        public Server(string host, int port)
        {
            this.host = host;
            this.port = port;
            clients = new Dictionary<string, ClientHandler>();
            executer = new ServerExecuter(clients);
        }

        public void Start()
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipe);
            server.Listen(0);

            //线程：监听客户端的接入
            Task.Run(() =>
            {
                while (run)
                {
                    Socket client = server.Accept();
                    ClientHandler handler = new ClientHandler(client);
                    lock (clients)
                    {
                        clients.Add((client.RemoteEndPoint as IPEndPoint).ToString(), handler);
                    }
                    handler.callback = () => { Remove(handler); };
                    handler.sendQueue.Enqueue(Command.Success());
                    Log($"{client.RemoteEndPoint as IPEndPoint}连入");
                }
            });

            //线程：统一处理客户端的指令
            handlerThread = new Thread(() =>
            {
                while (run)
                {
                    lock (clients)
                    {
                        foreach (string id in clients.Keys)
                        {
                            object package = clients[id].GetNextOutput();
                            if (package is Command command)
                            {
                                Execute(command, id);
                            }
                            else if (package != null)
                            {
                                Log(package);
                            }
                        }
                    }
                }
            })
            {
                IsBackground = true
            };
            handlerThread.Start();

            Log("服务器初始化已完成");
        }

        private void Execute(Command command, string sender = null)
        {
            if (sender != null)
                command.SetSender(sender);
            executer.Execute(command);
        }

        public void Remove(string id)
        {
            lock (clients) { clients.Remove(id); }
        }

        public void Remove(ClientHandler handler)
        {
            lock (clients)
            {
                List<KeyValuePair<string, ClientHandler>> keyValuePairs = clients.ToList();
                foreach (KeyValuePair<string, ClientHandler> keyValuePair in keyValuePairs)
                {
                    if (handler == keyValuePair.Value)
                    {
                        clients.Remove(keyValuePair.Key);
                    }
                }
            }
        }

        public Queue<object> Log(object output = null)
        {
            if (output != null)
                log.Enqueue(output);
            return log;
        }
    }
}
