using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Talkift.Client.Models;

namespace Talkift.Client.Services
{
    public class BackdropService
    {
        private static readonly StorageService _storage = new();

        public static BackdropType CurrentBackdrop { get; private set; } = BackdropType.Mica;

        public static async Task LoadBackdropAsync()
        {
            var saved = await _storage.LoadAsync<string>("backdrop");
            if (!string.IsNullOrEmpty(saved) && Enum.TryParse<BackdropType>(saved, out var type))
            {
                CurrentBackdrop = type;
            }
        }

        public static async Task SetBackdropAsync(BackdropType type)
        {
            CurrentBackdrop = type;
            await _storage.SaveAsync("backdrop", type.ToString());
        }

        public static void ApplyBackdrop(Window window, BackdropType type)
        {
            if (window == null) return;

            try
            {
                switch (type)
                {
                    case BackdropType.Mica:
                        window.SystemBackdrop = new MicaBackdrop();
                        break;
                    case BackdropType.Acrylic:
                        window.SystemBackdrop = new DesktopAcrylicBackdrop();
                        break;
                    case BackdropType.Default:
                    default:
                        window.SystemBackdrop = null;
                        break;
                }
            }
            catch
            {
                window.SystemBackdrop = null;
            }
        }

        public static void ApplyCurrentBackdrop(Window window)
        {
            ApplyBackdrop(window, CurrentBackdrop);
        }
    }
}
