using System.ComponentModel;

namespace TodoMauiApp.MVVM;

public class ViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// The event handler required by <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Sets a backing store value and notify watchers of the change.  The type must
    /// implement <see cref="IEquatable{T}"/> for proper comparisons.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="storage">The backing store</param>
    /// <param name="value">The new value</param>
    /// <param name="propertyName"></param>
    protected void SetProperty<T>(ref T storage, T value, string propertyName = null)
    {
        if (!storage.Equals(value))
        {
            storage = value;
            NotifyPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Notifies the data context that the property named has changed value.
    /// </summary>
    /// <param name="propertyName">The name of the property</param>
    protected void NotifyPropertyChanged(string propertyName = null)
    {
        if (propertyName != null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
