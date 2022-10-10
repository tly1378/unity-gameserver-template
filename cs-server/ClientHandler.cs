using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkLibrary;

namespace GameServer
{
    class ClientHandler
    {
        bool run = true;

        public Socket socket;
        public Thread receiverThread;
        public Thread senderThread;
        public Action callback;
        public readonly Queue<object> sendQueue = new Queue<object>();
        public readonly Queue<object> receiveQueue = new Queue<object>();


        public ClientHandler(Socket socket)
        {
            this.socket = socket;

            receiverThread = new Thread(() => { ReceiveLoop(); })
            {
                IsBackground = true
            };
            receiverThread.Start();

            senderThread = new Thread(() => { SendLoop(); })
            {
                IsBackground = true
            };
            senderThread.Start();
        }

        void ReceiveLoop()
        {
            while (run)
            {
                try
                {
                    object _object = TCPTool.Receive(socket);
                    receiveQueue.Enqueue(_object);
                }
                catch (Exception e)
                {
                    receiveQueue.Enqueue($"{e}");
                    Stop();
                }
            }
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
                        TCPTool.Send(socket, _object);
                    }
                    catch (Exception e)
                    {
                        receiveQueue.Enqueue($"{e}");
                        Stop();
                    }
                }
            }
        }

        public void Stop()
        {
            run = false;
            TCPTool.Close(socket);
            callback();
        }

        public object GetNextOutput()
        {
            if (receiveQueue.Count > 0)
                return receiveQueue.Dequeue();
            else
                return null;
        }
    }
}