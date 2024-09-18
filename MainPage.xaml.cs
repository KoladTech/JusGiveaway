using Firebase.Auth;
using Firebase.Database;
using static JusGiveaway.CustomAlertPage;

namespace JusGiveaway;

public partial class MainPage : ContentPage
{
    private readonly SQLiteDBHelper dbHelper;
    private readonly List<UserData> userData;
    private readonly FirebaseClient firebaseClient;

    public MainPage()
    {
        InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData();
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();

        PlayerNameLabel.Text += userData[0].Name;
    }

    private async void OnHeadsOrTailsGameButtonClicked(object sender, EventArgs e)
    {
        await CommonFunctions.AnimateButton((Button)sender);
        Dictionary<string,string>? firebaseGiveawayData = null;

        try
        {
            firebaseGiveawayData = await CommonFunctions.GetGameDataFromFirebaseAsync(firebaseClient);
        }
        catch (Exception)
        {
            await CommonFunctions.DisplayCustomAlertPage(
                "Error", "Error launching game, please check your internet connection and try again", 
                "Close", "", true, false, AlertType.Error, Navigation);
        }

        //setup dummy data that can be passed around when there is no network
        if (firebaseGiveawayData != null)
        {
            await Navigation.PushAsync(new HeadsOrTails(firebaseGiveawayData));
        }
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await CommonFunctions.DisplayCustomAlertPage($"Sponsored by", "This giveaway is sponsored by DavidBukola Foundation",
            "Close", "",
            true, false,
            AlertType.Info,
            Navigation) ;

        //await CommonFunctions.AnimateButton((ImageButton)sender);

        //var firebaseGiveawayData = await CommonFunctions.GetGameDataFromFirebaseAsync(firebaseClient);

        //await Navigation.PushAsync(new HeadsOrTails(firebaseGiveawayData));
    }

}

