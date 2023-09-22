namespace TodoMauiApp.MVVM;

internal interface IMVVMHelper
{
    Task RunOnUiThreadAsync(Action func);

    Task DisplayErrorAlertAsync(string title, string message);
}
