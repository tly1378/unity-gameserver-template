using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GameServer
{
    class Program
    {
        static Server server;

        static void Main(string[] _)
        {
            server = new Server("192.168.0.3", 8519);
            server.Start();

            Timer timer = new Timer();
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(Update);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();

            while (true) { }
        }

        static void Update(object sender, ElapsedEventArgs e)
        {
            while (server.Log().Count > 0)
            {
                Console.WriteLine(server.Log().Dequeue());
            }
        }
    }
}
