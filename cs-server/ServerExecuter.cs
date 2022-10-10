using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerExecuter : Executer
    {
        readonly Dictionary<string, ClientHandler> clients;

        public ServerExecuter(Dictionary<string, ClientHandler> clients)
        {
            this.clients = clients;
        }

        public void SetName(Command command)
        {
            string senderName = command.sender;
            string newName = command.GetName();
            if (senderName == newName)
            {
                return;
            }
            if (clients.ContainsKey(newName))
            {
                clients[senderName].sendQueue.Enqueue("名称重复");
                return;
            }
            lock (clients)
            {
                ClientHandler handler = clients[senderName];
                clients.Remove(senderName);
                clients.Add(newName, handler);
            }
            clients[newName].sendQueue.Enqueue($"名称已变更为{newName}");

            //测试代码：通知客户端更改目标
            clients[newName].sendQueue.Enqueue(new Command()
            {
                cmd = "SetTarget",
                parameters = new object[] { newName }
            });
        }

        public void SendTo(Command command)
        {
            string message = command.GetMessage();
            string[] receivers = command.GetReceivers();
            foreach (string id in receivers)
            {
                lock (clients)
                {
                    if (clients.ContainsKey(id))
                        clients[id].sendQueue.Enqueue($"{command.sender}: {message}");
                    else
                    {
                        throw new Exception($"{command.sender}试图向{id}发送消息，但其不存在。");
                    }
                }
            }
        }
    }
}
