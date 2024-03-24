using Firebase.Auth;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Reflection;

namespace JusGiveaway;

public partial class RegistrationPage : ContentPage
{
    private readonly Regex emailRegex = new(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");

	public RegistrationPage()
	{
		InitializeComponent();

        // Check if the code has been run before
        if (!Preferences.ContainsKey("CodeExecuted"))
        {
            // Code to run only once goes here
            //TODO Only do this when app first runs
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("JusGiveaway.HeadsorTailsGameData.db3"))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    File.WriteAllBytes(SQLiteDBHelper.DbPath, memoryStream.ToArray());
                }
            }
            // Set a flag indicating that the code has been executed
            Preferences.Set("CodeExecuted", true);
        }
    }
    private async void OnRegistrationCompleted(object sender, EventArgs e, User user)
    {
        //store userdata in sqlite db
        SQLiteDBHelper dbHelper = new();
        UserData userData = new()
        {
            Name = user.Info.DisplayName,
            UID = user.Uid,
            EmailAddress = user.Info.Email,
            DeviceInfo = JsonConvert.SerializeObject(getPhoneInfo())
        };
        dbHelper.InsertItem(userData);
        Preferences.Set("UserRegistered", true);
        // Navigate to SignInPage after registration
        await Navigation.PushAsync(new SignInPage());
    }

    private Dictionary<string, string> getPhoneInfo()
	{
        Dictionary<string, string> phoneInfo = new Dictionary<string, string>
        {
            {"Model", DeviceInfo.Model },
            {"Manufacturer", DeviceInfo.Manufacturer },
            {"Version", DeviceInfo.VersionString },
            {"Name", DeviceInfo.Name },
            {"Platform", DeviceInfo.Platform.ToString() },
            {"Type", DeviceInfo.DeviceType.ToString() },
            {"Idiom", DeviceInfo.Idiom.ToString() },
        };
        return phoneInfo;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await CommonFunctions.AnimateButton((Button)sender);

        string userName = NameEntry.Text;
        string email = EmailEntry.Text;
        string pwd = PwdEntry.Text;

        // Register user with email and password
        UserCredential? user = await FirebaseAuthHelper.Instance.RegisterUserWithEmailAndPasswordAsync(email, pwd, userName);
        if (user != null)
        {
            // Registration successful
            var customAlertPage = new CustomAlertPage("Success", $"{user.User.Info.DisplayName} registered successfully!", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            await customAlertPage.WaitForUserResponseAsync();
            // Navigate to the next page or perform any other action
            OnRegistrationCompleted(sender, e, user.User);
        }
        else
        {
            // Registration failed
            var customAlertPage = new CustomAlertPage("Error", "Failed to register user", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            await customAlertPage.WaitForUserResponseAsync();
        }
    }

    private void OnUserNameTextChanged(object sender, TextChangedEventArgs e)
    {
        // Get the Entry control
        Entry entry = (Entry)sender;

        // Remove any spaces from the entered text
        entry.Text = entry.Text.Replace(" ", string.Empty);

        RegisterButton.IsEnabled = enableRegisterButton(PwdEntry.Text);
    }

    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {

        if (!string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            if (!emailRegex.IsMatch(EmailEntry.Text))
            {
                // Invalid email format
                EmailEntry.TextColor = Color.FromRgb(255, 0, 0); // Optional: Set text color to indicate invalid input
                // Optionally show an error message to the user
            }
            else
            {
                // Valid email format
                EmailEntry.TextColor = Color.FromRgb(0, 0, 0); // Reset text color
            }
        }
        else
        {
            // Empty input, disable button
            RegisterButton.IsEnabled = false;
        }
        RegisterButton.IsEnabled = enableRegisterButton(PwdEntry.Text);
    }

    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        string password = e.NewTextValue;

        RegisterButton.IsEnabled = enableRegisterButton(password);
    }

    private bool IsPasswordValid(string password)
    {
        // Check for at least one uppercase letter, one lowercase letter, one digit,
        // one special character and a minimum length of six characters
        return (password.Length >= 6 &&
                password.Any(char.IsUpper) &&
                password.Any(char.IsLower) &&
                password.Any(char.IsDigit) &&
                password.Any(ch => !char.IsLetterOrDigit(ch)));
    }

    private void OnConfirmPwdTextChanged(object sender, TextChangedEventArgs e)
    {
        RegisterButton.IsEnabled = enableRegisterButton(PwdEntry.Text);
    }

    private bool enableRegisterButton(string password) {

        if (NameEntry.Text == "")
            return false;
        if (EmailEntry.Text == "")
            return false;
        if (password == null || !IsPasswordValid(password))
            return false;
        if (PwdEntry.Text != PwdConfirmEntry.Text)
            return false;
        if (!string.IsNullOrWhiteSpace(EmailEntry.Text))
            if (!emailRegex.IsMatch(EmailEntry.Text))
                return false;
        return true; 
    }
}