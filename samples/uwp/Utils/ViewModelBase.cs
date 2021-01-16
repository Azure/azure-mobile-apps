using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ZumoQuickstart.Utils
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets a bindable property.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">The field backing store</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the property was set properly</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raise the PropertyChanged event for a property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Displays an alert on the screen
        /// </summary>
        /// <param name="title">The title of the alert</param>
        /// <param name="message">The message content</param>
        /// <returns></returns>
        protected async Task DisplayAlertAsync(string title, string message)
        {
            var dialog = new MessageDialog(message, title);
            dialog.Commands.Add(new UICommand("OK") { Id = 0 });
            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }
    }
}
