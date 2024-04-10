using Firebase.Auth;
using System.Xml;
using System.Reflection;
using Firebase.Database;
using Microsoft.Maui.Storage;

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
        await CommonFunctions.AnimateButton((Button)sender);

        string email = SignInEmailEntry.Text;
        string pwd = SignInPwdEntry.Text;

        //SQLiteDBHelper dbHelper = new();
        List<UserData> items = dbHelper.GetUserData();

        //sign in
        user = await FirebaseAuthHelper.Instance.SignInUserWithEmailAndPasswordAsync(email, pwd);
        if (user != null)
        {
            // Registration successful
            //await DisplayAlert("Success", $"User {user.User.Info.DisplayName} signed in successfully!", "OK");
            var customAlertPage = new CustomAlertPage("Success", $"{user.User.Info.DisplayName} signed in successfully!", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            await customAlertPage.WaitForUserResponseAsync();

            // Navigate to the next page or perform any other action
            OnSignInCompleted(sender, e);
            Preferences.Set("UserSignedIn", true);

        }
        else
        {
            // Sign In failed
            //await DisplayAlert("Sign In Failed", "Please check your credentials", "OK");
            var customAlertPage = new CustomAlertPage("Sign In Failed!", "Please check your credentials", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            await customAlertPage.WaitForUserResponseAsync();
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

    private bool EnableSignInButton()
    {
        if (SignInEmailEntry.Text is null ||SignInEmailEntry.Text == "")
            return false;
        if (SignInPwdEntry.Text is null || SignInPwdEntry.Text == "")
            return false;
        return true;
    }
}