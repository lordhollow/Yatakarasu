using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

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
            //TODO::キューイング
            sendCmd = cmd;
        }

        public void SendImmidiate(string cmd)
        {
            SendRequest(cmd);
        }

        string sendCmd = null;

        int cnt = 9999;

        public DateTime LatestRequestTime { get; private set; }

        public bool Update()
        {
            //if (!Active) return false;
            try
            {
                if (!string.IsNullOrEmpty(sendCmd))
                {
                    if (Active)
                    {
                        SendRequest(sendCmd);
                    }
                    sendCmd = "";
                    cnt = 0;
                }
                cnt++;
                //ハートビートは30秒以内に届くようにする。ローカルだし20秒に1回ぐらい送っていればOK.
                if (cnt > 10)
                {
                    SendRequest("HEARTBEAT");
                    cnt = 0;
                }
            }
            catch
            {
                Active = false;
            }
            return false;
        }

        private async void SendRequest(string message)
        {
            await Task.Run(() =>
            {
                try
                {
                    LatestRequestTime = DateTime.Now;
                    var client = new TcpClient();
                    client.Connect("localhost", 54892);
                    var dt = System.Text.Encoding.UTF8.GetBytes(message);
                    client.GetStream().Write(dt, 0, dt.Length);
                    client.Close();
                    Active = true;
                }
                catch
                {
                    Active = false;
                }
            });
        }
    }

}
