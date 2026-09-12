using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class ChatWallpaperPicker : ContentDialog
    {
        private static readonly List<string> PresetColors = new()
        {
            "#FFFFFF", "#F5F5F5", "#E3F2FD", "#E8F5E9",
            "#FFF3E0", "#FCE4EC", "#F3E5F5", "#E0F7FA",
            "#1A1A2E", "#16213E", "#0F3460", "#1B1B2F"
        };

        public string? SelectedColor { get; private set; }

        public ChatWallpaperPicker()
        {
            this.InitializeComponent();
            DialogRoot.Title = LanguageService.GetString("ChatWallpaper");
            ChooseText.Text = LanguageService.GetString("ChooseWallpaper");
            CustomText.Text = LanguageService.GetString("OrCustomColor");

            WallpaperGridView.ItemsSource = PresetColors;
            WallpaperGridView.ItemClick += WallpaperGridView_ItemClick;
        }

        private void WallpaperGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is string color)
            {
                SelectedColor = color;
            }
        }
    }
}
