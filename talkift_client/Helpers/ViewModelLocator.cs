using System;
using System.Collections.Generic;

namespace Talkift.Client;

public static class ViewModelLocator
{
    private static readonly Dictionary<Type, object> _viewModels = new();
    private static readonly Dictionary<Type, Type> _mappings = new();

    static ViewModelLocator()
    {
        Register<ShellWindow, ShellViewModel>();
        Register<LoginPage, LoginViewModel>();
        Register<ChatPage, ChatPageViewModel>();
        Register<ConversationList, ConversationListViewModel>();
        Register<MessageListPage, MessageListViewModel>();
        Register<MessageInput, MessageInputViewModel>();
        Register<SettingsPage, SettingsViewModel>();
        Register<ProfilePage, ProfileViewModel>();
        Register<SearchPanel, SearchViewModel>();
        Register<ContactPage, ContactViewModel>();
    }

    public static void Register<TView, TViewModel>()
        where TView : class
        where TViewModel : class
    {
        _mappings[typeof(TView)] = typeof(TViewModel);
    }

    public static TViewModel GetViewModel<TView>(TView view) where TView : class
    {
        var viewType = typeof(TView);
        if (_mappings.TryGetValue(viewType, out var vmType) && _viewModels.TryGetValue(vmType, out var vm))
        {
            return (TViewModel)vm;
        }
        throw new KeyNotFoundException($"No ViewModel registered for {viewType.Name}");
    }

    public static TViewModel GetOrCreateViewModel<TView>() where TView : class
    {
        var vmType = _mappings[typeof(TView)];
        if (_viewModels.TryGetValue(vmType, out var vm) && vm is TViewModel typedVm)
        {
            return typedVm;
        }

        var constructor = vmType.GetConstructor(Array.Empty<Type>());
        if (constructor == null)
            throw new InvalidOperationException($"No parameterless constructor found for {vmType.Name}");

        var instance = (TViewModel)constructor.Invoke(Array.Empty<object>());
        _viewModels[vmType] = instance;
        return instance;
    }

    public static void Clear()
    {
        _viewModels.Clear();
    }

    public static bool IsRegistered<TView>() where TView : class
    {
        return _mappings.ContainsKey(typeof(TView));
    }
}
