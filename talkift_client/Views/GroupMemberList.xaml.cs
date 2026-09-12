using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Talkift.Client.Services;

namespace Talkift.Client.Views
{
    public sealed partial class GroupMemberList : UserControl
    {
        private readonly ObservableCollection<string> _members = new();

        public GroupMemberList()
        {
            this.InitializeComponent();
            ApplyLocalization();
            MemberListView.ItemsSource = _members;
        }

        private void ApplyLocalization()
        {
            TitleText.Text = LanguageService.GetString("Members");
        }

        public void SetMembers(List<string> members)
        {
            _members.Clear();
            foreach (var m in members)
                _members.Add(m);
            CountText.Text = $"({_members.Count})";
        }
    }
}
