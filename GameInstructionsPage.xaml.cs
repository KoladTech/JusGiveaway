using System.Threading.Tasks;
using System.Windows.Input;

namespace JusGiveaway;

public partial class GameInstructionsPage : ContentPage
{    public string GameInstructionsTitle { get; set; }
    public string GameInstructions { get; set; }
    private TaskCompletionSource<bool> _taskCompletionSource;
    public GameInstructionsPage(
        string gameInstructionsTitle,
        string gameInstructions)
    {
        InitializeComponent();
        GameInstructionsTitle = gameInstructionsTitle;
        GameInstructions = gameInstructions;
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

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // This could be navigation back, closing a popup, or hiding the view
        Navigation.PopModalAsync(); // For navigating back in the stack

        // If you're using a modal or popup, you can use:
        // this.IsVisible = false; (for hiding the ContentView)
    }

    public ICommand OpenLinkCommand => new Command<string>((url) =>
    {
        if (!string.IsNullOrEmpty(url))
        {
            // This will open the URL in the default browser
            Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
    });


}