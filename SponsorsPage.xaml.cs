using System.Threading.Tasks;
using System.Windows.Input;

namespace JusGiveaway;

public partial class SponsorsPage : ContentPage
{
    public SponsorsPage(string sponsorName,string sponsorInstagramName)
	{
        InitializeComponent();
        BindingContext = this;
        SponsorInstagramLabel.Text = sponsorName;

        // Find the TapGestureRecognizer associated with the Span
        if (SponsorInstagramLabel.GestureRecognizers[0] is TapGestureRecognizer tapGesture)
        {
            // Update the CommandParameter with additional text
            tapGesture.CommandParameter = $"https://www.instagram.com/{sponsorInstagramName}/";
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync(); // For navigating back in the stack
    }

    public ICommand OpenSponsorInstagramCommand => new Command<string>((url) =>
    {
        if (!string.IsNullOrEmpty(url))
        {
            // This will open the URL in the default browser
            //Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            Launcher.OpenAsync(new Uri(url));
        }
    });
}