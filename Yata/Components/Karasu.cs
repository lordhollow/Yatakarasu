using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yata.Components
{
    internal sealed class LaunchResult
    {
        internal string Status { get; }
        internal string Message { get; }
        internal bool Success => Status == "ok";
        internal bool Suppressed => Status == "blocked";
        internal LaunchResult(string status, string message) { Status = status; Message = message; }
    }

    class Karasu : IUpdatable
    {
        public int UpdateRate => 10;
        public bool Active { get; private set; }
        public DateTime LatestRequestTime { get; private set; }
        internal Task<LaunchResult> LastRequest { get; private set; }
        readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        readonly int port;
        readonly Func<string, Task<LaunchResult>> transport;
        int cnt = 9999;

        public Karasu() : this(54892) { }
        internal Karasu(int port, Func<string, Task<LaunchResult>> transport = null) { this.port = port; this.transport = transport; }
        public void Send(string cmd) { LastRequest = RequestAsync(cmd); }
        public void SendImmidiate(string cmd) { _ = RequestAsync(cmd); }
        public bool Update()
        {
            if (++cnt > 10) { _ = RequestAsync("HEARTBEAT"); cnt = 0; }
            return false;
        }
        internal async Task<LaunchResult> RequestAsync(string command)
        {
            if (!await sendLock.WaitAsync(5000).ConfigureAwait(false)) return new LaunchResult("error", "Karasuへの送信待ちがタイムアウトしました。");
            try
            {
                if (transport != null) return await transport(command).ConfigureAwait(false);
                return await Task.Run(() => Exchange(command)).ConfigureAwait(false);
            }
            catch (Exception ex) { Active = false; return new LaunchResult("error", ex.GetBaseException().Message); }
            finally { sendLock.Release(); }
        }
        LaunchResult Exchange(string command)
        {
            bool attempted = false;
            try
            {
                var id = Guid.NewGuid().ToString("N");
                var request = Encoding.UTF8.GetBytes("REQ3\t" + id + "\t" + Convert.ToBase64String(Encoding.UTF8.GetBytes(command)));
                if (request.Length > 65536) return new LaunchResult("error", "実行要求が長すぎます。");
                using (var client = new TcpClient())
                {
                    var connect = client.ConnectAsync(System.Net.IPAddress.Loopback, port);
                    if (!connect.Wait(3000)) throw new TimeoutException("Karasuに接続できません。");
                    connect.GetAwaiter().GetResult();
                    var stream = client.GetStream(); stream.ReadTimeout = 5000; stream.WriteTimeout = 3000;
                    LatestRequestTime = DateTime.Now;
                    attempted = true;
                    stream.Write(request, 0, request.Length);
                    client.Client.Shutdown(SocketShutdown.Send);
                    var result = ParseResponse(ReadResponse(stream), id);
                    Active = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Active = false;
                return new LaunchResult(attempted ? "unknown" : "error", attempted
                    ? "起動結果を確認できません。重複起動を避けるため自動再送しません。YataとKarasuの両方を更新・再起動してください。"
                    : "Karasuに接続できません: " + ex.GetBaseException().Message);
            }
        }
        internal static string ReadResponse(Stream stream)
        {
            using (var buffer = new MemoryStream())
            {
                var block = new byte[1024]; int read;
                while ((read = stream.Read(block, 0, block.Length)) != 0)
                {
                    if (buffer.Length + read > 8192) throw new InvalidDataException("Karasuの応答が長すぎます。");
                    buffer.Write(block, 0, read);
                }
                return new UTF8Encoding(false, true).GetString(buffer.ToArray());
            }
        }
        internal static LaunchResult ParseResponse(string response, string id)
        {
            var parts = response.Split('\t');
            if (parts.Length != 4 || parts[0] != "RESULT" || parts[1] != id || (parts[2] != "ok" && parts[2] != "error" && parts[2] != "blocked"))
                throw new InvalidDataException("Karasuの応答を照合できません。");
            return new LaunchResult(parts[2], new UTF8Encoding(false, true).GetString(Convert.FromBase64String(parts[3])));
        }
    }
}
