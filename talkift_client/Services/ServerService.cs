using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Talkift.Client.Services
{
    public class ServerService
    {
        public async Task<bool> TestConnectionAsync(string address, int port)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };
                var url = $"http://{address}:{port}/api/health";
                var response = await client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public string BuildWebSocketUrl(string address, int port)
        {
            return $"ws://{address}:{port}/ws";
        }

        public string BuildHttpUrl(string address, int port)
        {
            return $"http://{address}:{port}";
        }
    }
}
