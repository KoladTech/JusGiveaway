namespace JusGiveaway;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CustomAlertPage : ContentPage
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string Yes {  get; set; }
    public string No { get; set; }
    public bool ShowYes { get; set; }
    public bool ShowNo { get; set; }
    public bool IsYesClicked { get; private set; }

    private TaskCompletionSource<bool> _taskCompletionSource;


    public CustomAlertPage(string title, string message, string yes = "Yes", string no = "No", bool showYes = true, bool showNo = true)
    {
        InitializeComponent();
        Title = title;
        Message = message;
        Yes = yes;
        No = no;
        ShowYes = showYes;
        ShowNo = showNo;
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