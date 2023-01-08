// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TodoApp.AvaloniaUI.ViewModels;

namespace TodoApp.AvaloniaUI.Views
{
    public partial class TodoListView : ReactiveUserControl<TodoListViewModel>
    {
        public TodoListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { /* handle view activations, etc. */ });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
