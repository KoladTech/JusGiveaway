using Firebase.Auth;
using System.Xml;
using System.Reflection;
using Firebase.Database;
using Microsoft.Maui.Storage;
using static JusGiveaway.CustomAlertPage;
using CommunityToolkit.Maui.Alerts;

namespace JusGiveaway;

public partial class SignInPage : ContentPage
{
    private readonly SQLiteDBHelper dbHelper;
    private readonly List<UserData> userData;
    private readonly FirebaseClient firebaseClient;// = FirebaseClientHelper.Instance.GetFirebaseClient();
    private UserCredential? user;

    public SignInPage()
	{
		InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData();

        //does Resources contain stuff yet?
        //Application.Current.Resources["PlayerName"] = userData.Name;


        //this causes an error if there is no email to pull from DB
        //check DB before allowing navigation to login page or validate somehow else
        SignInEmailEntry.Text = userData[0].EmailAddress;
        user = null;
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();
    }

    private async void OnSignInCompleted(object sender, EventArgs e)
    {
        var gameSetupData = await CommonFunctions.GetGameDataFromFirebaseAsync(firebaseClient);

        //create first game in sqlite db
        //SQLiteDBHelper dbHelper = new();
        //List<UserData> items = dbHelper.GetUserData();

        if (dbHelper.GetRowWithMaxId() == null)
        {
            GameData newGame = new()
            {
                UID = userData[0].UID,
                MaxPossibleWinnings = int.Parse(gameSetupData["MaxPossibleWinningsPerPerson"]),
                MinCashOut = int.Parse(gameSetupData["MinCashoutPerPerson"]),
            };
            dbHelper.InsertItem(newGame);
        }

        //check data entered
        //List<GameData> game = dbHelper.GetGameData();
        // Navigate to MainPage after sign in
        await Navigation.PushAsync(new MainPage());
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        CommonFunctions.ToggleActivityIndicator(SignInProgressIndicator, SignInButton);
        await CommonFunctions.AnimateButton((Button)sender);

        string email = SignInEmailEntry.Text;
        string pwd = SignInPwdEntry.Text;

        //SQLiteDBHelper dbHelper = new();
        List<UserData> items = dbHelper.GetUserData();

        //sign in
        user = await FirebaseAuthHelper.Instance.SignInUserWithEmailAndPasswordAsync(email, pwd);

        CommonFunctions.ToggleActivityIndicator(SignInProgressIndicator, SignInButton);
        if (user != null)
        {
            // Registration successful

            await Toast.Make($"{user.User.Info.DisplayName} signed in successfully!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();

            //await CommonFunctions.DisplayCustomAlertPage(
            //    "Success", $"{user.User.Info.DisplayName} signed in successfully!",
            //    "Ok", "", true, false, AlertType.Info, Navigation);

            // Navigate to the next page or perform any other action
            OnSignInCompleted(sender, e);
            Preferences.Set("UserSignedIn", true);
        }
        else
        {
            // Sign In failed
            await CommonFunctions.DisplayCustomAlertPage(
                "Sign In Failed", "Please check your credentials",
                "Close", "", true, false, AlertType.Error, Navigation);
        }
    }

    private void OnSignInEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        SignInButton.IsEnabled = EnableSignInButton();
    }

    private void OnSignInPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        SignInButton.IsEnabled = EnableSignInButton();
    }

    private void OnPasswordRevealButtonClicked(object sender, EventArgs e)
    {
        // Toggle password visibility
        SignInPwdEntry.IsPassword = !SignInPwdEntry.IsPassword;

        // Change the eye icon accordingly
        PasswordRevealButton.Source = SignInPwdEntry.IsPassword ? "eye_image_close.png" : "eye_image_open.png";
    }

    private bool EnableSignInButton()
    {
        if (SignInEmailEntry.Text is null ||SignInEmailEntry.Text == "")
            return false;
        if (SignInPwdEntry.Text is null || SignInPwdEntry.Text == "")
            return false;
        return true;
    }
}