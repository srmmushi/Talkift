using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class FileAttachmentPreview : UserControl
    {
        public string? FilePath { get; private set; }

        public event EventHandler? RemoveRequested;

        public FileAttachmentPreview()
        {
            this.InitializeComponent();
        }

        public void SetFile(string filePath)
        {
            FilePath = filePath;
            FileNameText.Text = Path.GetFileName(filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                var size = fileInfo.Length;
                FileSizeText.Text = size > 1024 * 1024
                    ? $"{size / (1024.0 * 1024.0):F1} MB"
                    : $"{size / 1024.0:F1} KB";
            }

            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            FilePath = null;
        }

        private void RemoveFileButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
