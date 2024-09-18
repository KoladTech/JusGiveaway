using Android.Content;

namespace JusGiveaway;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CustomAlertPage : ContentPage
{
    public string AlertTitle { get; set; }
    public string AlertMessage { get; set; }
    public string PrimaryBtnText {  get; set; }
    public string SecondaryBtnText { get; set; }
    public bool ShowPrimaryBtn { get; set; }
    public bool ShowSecondaryBtn { get; set; }
    public Color PrimaryButtonColor { get; set; }
    public Color SecondaryButtonColor { get; set; }

    private TaskCompletionSource<bool> _taskCompletionSource;

    //declare an enum and based on enum choose color
    public enum AlertType
    {
        Info,
        Warning,
        Error
    }
    public string ToAlertTypeString(AlertType alertType)
    {
        return alertType switch
        {
            AlertType.Info => "Info",
            AlertType.Warning => "Warning",
            AlertType.Error => "Error",
            _ => throw new ArgumentOutOfRangeException(nameof(alertType), alertType, null)
        };
    }
    public string GetBtnColorFromAlertType(AlertType alertType)
    {
        return alertType switch
        {
            AlertType.Info => "green",
            AlertType.Warning => "orange",
            AlertType.Error => "red",
            _ => throw new ArgumentOutOfRangeException(nameof(alertType), alertType, null)
        };
    }

    public CustomAlertPage(
        string alertTitle, 
        string alertMessage, 
        string primaryBtnText = "Yes", 
        string secondaryBtnText = "No", 
        bool showPrimaryBtn = true, 
        bool showSecondaryBtn = true,
        AlertType alertType = AlertType.Info)
    {
        InitializeComponent();
        AlertTitle = alertTitle;
        AlertMessage = alertMessage;
        PrimaryBtnText = primaryBtnText;
        SecondaryBtnText = secondaryBtnText;
        ShowPrimaryBtn = showPrimaryBtn;
        ShowSecondaryBtn = showSecondaryBtn;
        PrimaryButtonColor = Color.Parse(GetBtnColorFromAlertType(alertType));
        SecondaryButtonColor = Color.Parse(GetBtnColorFromAlertType(alertType));
        BindingContext = this;
        _taskCompletionSource = new TaskCompletionSource<bool>();
    }


    public async Task<bool> WaitForUserResponseAsync()
    {
        await _taskCompletionSource.Task;
        return _taskCompletionSource.Task.Result;
    }

    // Method to dismiss the modal alert and return false
    private async void OnNoClicked(object sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(false); // Set the result to false
        await Navigation.PopModalAsync(); // Pop the modal page from the navigation stack
    }

    // Method to return true when Yes is clicked and dismiss the modal alert
    private async void OnYesClicked(object sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(true); // Set the result to true
        await Navigation.PopModalAsync(); // Pop the modal page from the navigation stack
    }
}