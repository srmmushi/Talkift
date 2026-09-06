using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;
using Talkift.Client.Models;

namespace Talkift.Client.Services
{
    public class CredentialService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _credDir;

        public CredentialService()
        {
            _credDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Talkift", "credentials");
            Directory.CreateDirectory(_credDir);
        }

        public async Task SaveCredentialAsync(StoredCredential credential)
        {
            var json = JsonSerializer.Serialize(credential, JsonOptions);
            var protectedData = await ProtectStringAsync(json);
            var filePath = GetFilePath(credential.ServerId);
            await File.WriteAllTextAsync(filePath, protectedData);
        }

        public async Task<StoredCredential?> LoadCredentialAsync(string serverId)
        {
            try
            {
                var filePath = GetFilePath(serverId);
                if (!File.Exists(filePath))
                    return null;

                var protectedData = await File.ReadAllTextAsync(filePath);
                var json = await UnprotectStringAsync(protectedData);
                if (string.IsNullOrEmpty(json))
                    return null;

                return JsonSerializer.Deserialize<StoredCredential>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task DeleteCredentialAsync(string serverId)
        {
            var filePath = GetFilePath(serverId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAllCredentialsAsync()
        {
            if (Directory.Exists(_credDir))
            {
                foreach (var file in Directory.GetFiles(_credDir))
                {
                    File.Delete(file);
                }
            }
            await Task.CompletedTask;
        }

        private string GetFilePath(string serverId)
        {
            var safeId = serverId.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
            return Path.Combine(_credDir, $"{safeId}.json");
        }

        private static async Task<string> ProtectStringAsync(string plainText)
        {
            var provider = new DataProtectionProvider("LOCAL=user");
            var plainBuffer = CryptographicBuffer.CreateFromByteArray(
                System.Text.Encoding.UTF8.GetBytes(plainText));

            var protectedBuffer = await provider.ProtectAsync(plainBuffer);
            return CryptographicBuffer.EncodeToBase64String(protectedBuffer);
        }

        private static async Task<string> UnprotectStringAsync(string protectedBase64)
        {
            try
            {
                var provider = new DataProtectionProvider();
                var protectedBuffer = CryptographicBuffer.DecodeFromBase64String(protectedBase64);
                var plainBuffer = await provider.UnprotectAsync(protectedBuffer);
                CryptographicBuffer.CopyToByteArray(plainBuffer, out var plainBytes);
                return System.Text.Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
