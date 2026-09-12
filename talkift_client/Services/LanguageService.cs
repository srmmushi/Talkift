using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Talkift.Client.Services
{
    public class LanguageService
    {
        private static readonly StorageService _storage = new();
        private static Dictionary<string, string> _strings = new();
        private static string _currentLanguage = "en";

        public static string CurrentLanguage => _currentLanguage;

        public static event Action? LanguageChanged;

        public static async Task LoadLanguageAsync()
        {
            var saved = await _storage.LoadAsync<string>("language");
            if (!string.IsNullOrEmpty(saved))
            {
                _currentLanguage = saved;
            }
            await LoadStringsAsync(_currentLanguage);
        }

        public static async Task SetLanguageAsync(string lang)
        {
            if (_currentLanguage == lang) return;
            _currentLanguage = lang;
            await _storage.SaveAsync("language", lang);
            await LoadStringsAsync(lang);
            LanguageChanged?.Invoke();
        }

        public static string GetString(string key)
        {
            return _strings.TryGetValue(key, out var value) ? value : key;
        }

        public static string GetLanguageDisplayName(string lang)
        {
            return lang switch
            {
                "zh" => "\u4E2D\u6587",
                "en" => "English",
                _ => lang
            };
        }

        public static string[] GetAvailableLanguages()
        {
            return new[] { "en", "zh" };
        }

        public static int GetLanguageIndex(string lang)
        {
            var langs = GetAvailableLanguages();
            for (int i = 0; i < langs.Length; i++)
            {
                if (langs[i] == lang) return i;
            }
            return 0;
        }

        private static async Task LoadStringsAsync(string lang)
        {
            try
            {
                var assembly = typeof(LanguageService).Assembly;
                var resourceName = $"Talkift.Client.Resources.Strings.{lang}.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();
                    _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                }
                else
                {
                    _strings = new Dictionary<string, string>();
                }
            }
            catch
            {
                _strings = new Dictionary<string, string>();
            }
        }
    }
}
