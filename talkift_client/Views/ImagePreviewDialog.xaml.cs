using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class ImagePreviewDialog : ContentDialog
    {
        public string? ImagePath { get; private set; }

        public ImagePreviewDialog()
        {
            this.InitializeComponent();
            DialogRoot.Title = LanguageService.GetString("ImagePreview");
        }

        public void SetImage(string filePath)
        {
            ImagePath = filePath;
            try
            {
                var bitmap = new BitmapImage(new Uri(filePath));
                PreviewImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                CrashLogger.LogException("ImagePreviewDialog.SetImage", ex);
            }
        }
    }
}
