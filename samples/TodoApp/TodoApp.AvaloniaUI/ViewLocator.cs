// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using TodoApp.AvaloniaUI.ViewModels;

namespace TodoApp.AvaloniaUI
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
