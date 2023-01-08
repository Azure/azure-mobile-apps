// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using ReactiveUI;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace TodoApp.AvaloniaUI.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public ViewModelBase()
        {
            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Task.Run(async () => await OnActivated());
                Disposable.Create(async () => await OnDeactivated()).DisposeWith(disposables);
            });
        }

        /// <summary>
        /// The handler for the IActivatableViewModel
        /// </summary>
        public ViewModelActivator Activator { get; }

        /// <summary>
        /// Normally over-ridden - called when the view is activated.
        /// </summary>
        /// <returns></returns>
        public virtual Task OnActivated()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Normally over-ridden - called when the view is deactivated.
        /// </summary>
        /// <returns></returns>
        public virtual Task OnDeactivated()
        {
            return Task.CompletedTask;
        }
    }
}
