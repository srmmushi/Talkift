using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Talkift.Client.Engines;
using Talkift.Client.Services;
using Talkift.Client.Services.V2;

namespace Talkift.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTalkiftServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatEngine, ChatEngine>();
        services.AddSingleton<IUiEngine, UiEngine>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IMessageStore, MessageStore>();
        services.AddSingleton<IFileTransferService, FileTransferService>();

        services.AddSingleton<ChatEngineOptions>();
        services.AddSingleton<UiEngineOptions>();

        return services;
    }

    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ChatPageViewModel>();
        services.AddTransient<ConversationListViewModel>();
        services.AddTransient<MessageListViewModel>();
        services.AddTransient<MessageInputViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<ContactViewModel>();
        return services;
    }

    public static IServiceCollection AddPages(this IServiceCollection services)
    {
        services.AddTransient<ShellWindow>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ChatPage>();
        services.AddTransient<ConversationList>();
        services.AddTransient<MessageListPage>();
        services.AddTransient<MessageInput>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<ProfilePage>();
        services.AddTransient<SearchPanel>();
        services.AddTransient<ContactPage>();
        return services;
    }

    public static IServiceCollection AddControls(this IServiceCollection services)
    {
        services.AddTransient<MessageBubble>();
        services.AddTransient<ConversationItem>();
        services.AddTransient<AvatarControl>();
        services.AddTransient<TypingIndicator>();
        services.AddTransient<ConnectionStatusBadge>();
        services.AddTransient<EmptyStateControl>();
        services.AddTransient<AttachmentPreview>();
        services.AddTransient<EmojiPicker>();
        services.AddTransient<ImagePreviewDialog>();
        services.AddTransient<QuickActionToolbar>();
        services.AddTransient<PinMessagePanel>();
        services.AddTransient<MessageSearchPanel>();
        services.AddTransient<ReplyBar>();
        services.AddTransient<MessageReactionBar>();
        services.AddTransient<FileAttachmentPreview>();
        services.AddTransient<VoiceRecorder>();
        services.AddTransient<ThemeToggle>();
        services.AddTransient<ChatHeader>();
        services.AddTransient<GroupMemberList>();
        services.AddTransient<UserProfileCard>();
        services.AddTransient<NotificationBadge>();
        services.AddTransient<UserStatusIndicator>();
        services.AddTransient<MessageThreadPanel>();
        services.AddTransient<ChatWallpaperPicker>();
        services.AddTransient<MessageContextMenu>();
        return services;
    }
}
