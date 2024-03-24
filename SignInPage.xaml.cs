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
    public SignInPage()
	{
		InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData();
        SignInEmailEntry.Text = userData[0].EmailAddress;
    }

    private async Task<object> GetGameSetupData()
    {
        // Create a Firebase client
        FirebaseClient firebaseClient;
        firebaseClient = new FirebaseClient("https://headsortails-c1e0b-default-rtdb.europe-west1.firebasedatabase.app/");
        // Read the value from the Firebase Realtime Database
        var res = await firebaseClient.Child("GameSetupData").OnceAsync<object>();
        var res2 = await firebaseClient.Child("GameSetupData/Giver").OnceSingleAsync<string>();

        
        return res;
    }

    public async Task<Dictionary<string, string>> GetGameSetupDataAsync()
    {
        try
        {
            // Create a Firebase client
            FirebaseClient firebaseClient;
            firebaseClient = new FirebaseClient("https://headsortails-c1e0b-default-rtdb.europe-west1.firebasedatabase.app/");
            // Get data from Firebase Realtime Database
            var data = await firebaseClient.Child("GameSetupData").OnceSingleAsync<Dictionary<string, string>>();
          
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting data: {ex.Message}");
            return null;
        }
    }

    private async void OnSignInCompleted(object sender, EventArgs e)
    {
        var gameSetupData = await GetGameSetupDataAsync();

        //create first game in sqlite db
        //SQLiteDBHelper dbHelper = new();
        //List<UserData> items = dbHelper.GetUserData();

        if (dbHelper.GetRowWithMaxId() == null)
        {
            GameData newGame = new()
            {
                UID = userData[0].UID,
                MaxPossibleWinnings = int.Parse(gameSetupData["MaxPossibleWinningsPerPerson"]),
                MinCashOut = int.Parse(gameSetupData["MinCashOut"]),
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
        UserCredential? user = await FirebaseAuthHelper.Instance.SignInUserWithEmailAndPasswordAsync(email, pwd);
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