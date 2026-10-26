using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Yata.Components
{
    internal sealed class CodexRateLimitClient
    {
        readonly Func<string> resolve;
        readonly TimeSpan timeout;
        internal CodexRateLimitClient(Func<string> resolve = null, TimeSpan? timeout = null)
        { this.resolve = resolve ?? FindExecutable; this.timeout = timeout ?? TimeSpan.FromSeconds(20); }

        internal async Task<CodexUsageSnapshot> ReadAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var path = resolve();
            if (string.IsNullOrEmpty(path)) throw new FileNotFoundException("Codex is not installed.");
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            using (var process = new Process())
            {
                deadline.CancelAfter(timeout);
                process.StartInfo = new ProcessStartInfo(path, "app-server --listen stdio://")
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };
                process.ErrorDataReceived += (_, __) => { }; // Drain without retaining private payloads.
                deadline.Token.ThrowIfCancellationRequested();
                if (!process.Start()) throw new IOException("Cannot start Codex.");
                process.BeginErrorReadLine();
                using (deadline.Token.Register(() => Stop(process)))
                {
                    try
                    {
                        var json = new JavaScriptSerializer();
                        await Send(process, json.Serialize(new { id = 1, method = "initialize", @params = new { clientInfo = new { name = "yatakarasu_usage_monitor", version = "1.0.0" } } })).ConfigureAwait(false);
                        await Response(process, 1, deadline.Token).ConfigureAwait(false);
                        await Send(process, "{\"method\":\"initialized\",\"params\":{}}").ConfigureAwait(false);
                        await Send(process, "{\"id\":2,\"method\":\"account/rateLimits/read\"}").ConfigureAwait(false);
                        var result = await Response(process, 2, deadline.Token).ConfigureAwait(false);
                        return CodexUsageSnapshot.Parse(json.Serialize(result));
                    }
                    finally
                    {
                        // EOF lets app-server release its resources on a successful read.
                        try { process.StandardInput.Close(); } catch (IOException) { }
                        if (!process.WaitForExit(1000)) Stop(process);
                    }
                }
            }
        }
        static async Task Send(Process process, string message)
        {
            await process.StandardInput.WriteLineAsync(message).ConfigureAwait(false);
            await process.StandardInput.FlushAsync().ConfigureAwait(false);
        }
        static async Task<Dictionary<string, object>> Response(Process process, int id, CancellationToken cancellation)
        {
            var json = new JavaScriptSerializer();
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                var line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
                cancellation.ThrowIfCancellationRequested();
                if (line == null) throw new IOException("Codex closed the connection.");
                var response = json.DeserializeObject(line) as Dictionary<string, object>;
                if (response == null || !response.TryGetValue("id", out var responseId) || !Equals(responseId, id)) continue;
                if (response.ContainsKey("error")) throw new InvalidOperationException("Codex rate limit request failed. Check Codex sign-in.");
                var result = CodexUsageSnapshot.Object(response, "result");
                if (result == null) throw new FormatException("Codex result is missing.");
                return result;
            }
        }
        static void Stop(Process process)
        {
            // Only the process created for this read is owned by this client.
            try { if (!process.HasExited) process.Kill(); }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
        }
        internal static string FindExecutable()
        {
            var configured = Environment.GetEnvironmentVariable("YATA_CODEX_PATH");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                if (!Path.IsPathRooted(configured) || !configured.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || !File.Exists(configured))
                    throw new FileNotFoundException("YATA_CODEX_PATH must name an existing Codex executable.");
                return configured;
            }
            foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(directory)) continue;
                var candidate = Path.Combine(directory.Trim('"'), "codex.exe");
                if (Path.IsPathRooted(candidate) && File.Exists(candidate)) return candidate;
            }
            // The desktop app's versioned bin directory may not be on a running GUI's PATH.
            var desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
            if (Directory.Exists(desktop))
                return Directory.EnumerateDirectories(desktop).Select(dir => new FileInfo(Path.Combine(dir, "codex.exe")))
                    .Where(file => file.Exists).OrderByDescending(file => file.LastWriteTimeUtc).Select(file => file.FullName).FirstOrDefault();
            return null;
        }
    }
}
