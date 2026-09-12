using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Talkift.Client.Views
{
    public sealed partial class UserStatusIndicator : UserControl
    {
        private static readonly Brush OnlineBrush = new SolidColorBrush(Colors.Green);
        private static readonly Brush OfflineBrush = new SolidColorBrush(Colors.Gray);
        private static readonly Brush AwayBrush = new SolidColorBrush(Colors.Orange);
        private static readonly Brush BusyBrush = new SolidColorBrush(Colors.Red);

        public UserStatusIndicator()
        {
            this.InitializeComponent();
        }

        public void SetStatus(string status)
        {
            StatusDot.Fill = status?.ToLower() switch
            {
                "online" => OnlineBrush,
                "away" => AwayBrush,
                "busy" => BusyBrush,
                _ => OfflineBrush
            };
        }
    }
}
