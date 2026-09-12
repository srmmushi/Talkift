using System;

namespace Talkift.Client.Engines;

public sealed class UiEngineOptions
{
    public bool UseMica { get; set; } = true;
    public bool UseAcrylic { get; set; }
    public double DefaultOpacity { get; set; } = 1.0;
    public bool EnableAnimations { get; set; } = true;
    public bool EnableConnectedAnimations { get; set; } = true;
    public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(300);
    public bool EnableBackdrop { get; set; } = true;
}
