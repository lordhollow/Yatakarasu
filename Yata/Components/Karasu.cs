using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace Yata.Components
{
    /// <summary>
    /// karasu.exeとの通信
    /// </summary>
    class Karasu : IUpdatable
    {
        public int UpdateRate => 10;

        public bool Active { get; private set; } = true;

        public Karasu()
        {
        }

        public void Send(string cmd)
        {
            pending.Enqueue(cmd);
        }

        public void SendImmidiate(string cmd)
        {
            _ = SendRequest(cmd);
        }

        readonly ConcurrentQueue<string> pending = new ConcurrentQueue<string>();
        readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);

        int cnt = 9999;

        public DateTime LatestRequestTime { get; private set; }

        public bool Update()
        {
            //if (!Active) return false;
            try
            {
                string command;
                while (pending.TryDequeue(out command))
                {
                    _ = SendRequest(command);
                    cnt = 0;
                }
                cnt++;
                //ハートビートは30秒以内に届くようにする。ローカルだし20秒に1回ぐらい送っていればOK.
                if (cnt > 10)
                {
                    _ = SendRequest("HEARTBEAT");
                    cnt = 0;
                }
            }
            catch
            {
                Active = false;
            }
            return false;
        }

        private async Task SendRequest(string message)
        {
            await sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
            await Task.Run(() =>
            {
                try
                {
                    LatestRequestTime = DateTime.Now;
                    using (var client = new TcpClient())
                    {
                        client.Connect(System.Net.IPAddress.Loopback, 54892);
                        var dt = System.Text.Encoding.UTF8.GetBytes(message);
                        client.GetStream().Write(dt, 0, dt.Length);
                    }
                    Active = true;
                }
                catch
                {
                    Active = false;
                }
            }).ConfigureAwait(false);
            }
            finally { sendLock.Release(); }
        }
    }

}
