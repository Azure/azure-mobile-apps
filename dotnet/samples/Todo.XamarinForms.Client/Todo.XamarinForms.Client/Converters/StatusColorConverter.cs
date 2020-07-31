using System;
using System.Globalization;
using Xamarin.Forms;

namespace Todo.XamarinForms.Client.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? (Color)Application.Current.Resources["CompletedColor"] :
                (Color)Application.Current.Resources["ActiveColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
