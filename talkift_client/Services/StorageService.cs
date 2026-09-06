using System;
using System.Diagnostics;
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

        public async Task<T?> LoadAsync<T>(string key)
        {
            try
            {
                var filePath = Path.Combine(_dataDir, $"{key}.json");
                if (!File.Exists(filePath))
                    return default;

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StorageService.LoadAsync<{typeof(T).Name}> failed for key '{key}': {ex.Message}");
                return default;
            }
        }

        public async Task SaveAsync<T>(string key, T data)
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
