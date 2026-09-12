using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class VoiceRecorder : UserControl
    {
        private readonly DispatcherTimer _timer;
        private int _seconds;
        private bool _isRecording;

        public event EventHandler? CancelRequested;
        public event EventHandler<string>? VoiceReady;

        public VoiceRecorder()
        {
            this.InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
        }

        public void StartRecording()
        {
            _seconds = 0;
            _isRecording = true;
            UpdateTimeDisplay();
            RecordingIndicator.IsActive = true;
            _timer.Start();
            this.Visibility = Visibility.Visible;
        }

        public void StopRecording()
        {
            _isRecording = false;
            _timer.Stop();
            RecordingIndicator.IsActive = false;
        }

        private void Timer_Tick(object sender, object e)
        {
            _seconds++;
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            var ts = TimeSpan.FromSeconds(_seconds);
            RecordingTimeText.Text = ts.ToString(@"mm\:ss");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
            Hide();
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
            Hide();
            VoiceReady?.Invoke(this, $"voice_{_seconds}s.wav");
        }

        private void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
