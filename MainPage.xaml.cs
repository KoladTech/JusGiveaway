using Firebase.Auth;

namespace JusGiveaway;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        //return new Window(new NavigationPage(new MainPage()));

        //await Navigation.PushAsync(new MainPage());
    }

    private async void OnHeadsOrTailsGameButtonClicked(object sender, EventArgs e)
    {
        await CommonFunctions.AnimateButton((Button)sender);

        await Navigation.PushAsync(new HeadsOrTails());

    }

}

