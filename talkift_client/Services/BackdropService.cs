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
        public static double CurrentOpacity { get; private set; } = 1.0;

        public static async Task LoadBackdropAsync()
        {
            try
            {
                var saved = await _storage.LoadAsync<string>("backdrop");
                if (!string.IsNullOrEmpty(saved) && Enum.TryParse<BackdropType>(saved, out var type))
                {
                    CurrentBackdrop = type;
                }

                var opacity = await _storage.LoadAsync<double?>("opacity");
                if (opacity.HasValue)
                {
                    CurrentOpacity = opacity.Value;
                }
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("BackdropService.LoadBackdropAsync", ex);
            }
        }

        public static async Task SetBackdropAsync(BackdropType type)
        {
            CurrentBackdrop = type;
            await _storage.SaveAsync("backdrop", type.ToString());
        }

        public static async Task SetOpacityAsync(double opacity)
        {
            CurrentOpacity = Math.Clamp(opacity, 0.1, 1.0);
            await _storage.SaveAsync("opacity", CurrentOpacity);
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
            catch (Exception ex)
            {
                CrashLogger.LogException("BackdropService.ApplyBackdrop", ex);
                window.SystemBackdrop = null;
            }
        }

        public static void ApplyOpacity(Microsoft.UI.Xaml.UIElement element, double opacity)
        {
            if (element != null)
            {
                element.Opacity = opacity;
            }
        }

        public static void ApplyCurrentBackdrop(Window window)
        {
            ApplyBackdrop(window, CurrentBackdrop);
        }
    }
}
