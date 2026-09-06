using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Talkift.Client.Services
{
    public class StorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _dataDir;

        public StorageService()
        {
            _dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Talkift");
            Directory.CreateDirectory(_dataDir);
        }

        public string DataDir => _dataDir;

        public async Task<T?> LoadAsync<T>(string key) where T : class
        {
            try
            {
                var filePath = Path.Combine(_dataDir, $"{key}.json");
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveAsync<T>(string key, T data) where T : class
        {
            var filePath = Path.Combine(_dataDir, $"{key}.json");
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task DeleteAsync(string key)
        {
            var filePath = Path.Combine(_dataDir, $"{key}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }
    }
}
