using Firebase.Auth;
using Firebase.Database;

namespace JusGiveaway;

public partial class MainPage : ContentPage
{
    private readonly FirebaseClient firebaseClient;
    public MainPage()
    {
        InitializeComponent(); 
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();
    }

    private async void OnHeadsOrTailsGameButtonClicked(object sender, EventArgs e)
    {
        await CommonFunctions.AnimateButton((Button)sender);

        var firebaseGiveawayData = await CommonFunctions.GetGameDataFromFirebaseAsync(firebaseClient);

        await Navigation.PushAsync(new HeadsOrTails(firebaseGiveawayData));
    }

}

