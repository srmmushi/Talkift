using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class NotificationBadge : UserControl
    {
        public NotificationBadge()
        {
            this.InitializeComponent();
        }

        public void SetCount(int count)
        {
            if (count > 0)
            {
                CountText.Text = count > 99 ? "99+" : count.ToString();
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
