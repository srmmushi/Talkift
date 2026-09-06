using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Talkift.Client.Services
{
    public class ServerService
    {
        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(10) };

        public async Task<bool> TestConnectionAsync(string address, int port)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var url = $"http://{address}:{port}/api/health";
                var response = await Client.GetAsync(url, cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ServerService.TestConnectionAsync failed for {address}:{port}: {ex.Message}");
                return false;
            }
        }

        public static string BuildWebSocketUrl(string address, int port, bool useTls = false)
        {
            var scheme = useTls ? "wss" : "ws";
            return $"{scheme}://{address}:{port}/ws";
        }

        public string BuildHttpUrl(string address, int port)
        {
            return $"http://{address}:{port}";
        }
    }
}
