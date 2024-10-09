using Firebase.Auth;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Reflection;
using static JusGiveaway.CustomAlertPage;
using CommunityToolkit.Maui.Alerts;

namespace JusGiveaway;

public partial class RegistrationPage : ContentPage
{
    private readonly SQLiteDBHelper dbHelper;
    private UserData userData;
    private readonly Regex emailRegex = new(@"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$");

	public RegistrationPage()
	{
		InitializeComponent();
        dbHelper = new();

        // Check if the code has been run before
        if (!Preferences.ContainsKey("SQLiteDBCreated"))
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
            Preferences.Set("SQLiteDBCreated", true);
        }

        // Attach event handlers for each Entry control in the form
        NameEntry.Focused += OnEntryFocused;
        EmailEntry.Focused += OnEntryFocused;
        PwdEntry.Focused += OnEntryFocused;
        PwdConfirmEntry.Focused += OnEntryFocused;
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
        CommonFunctions.ToggleActivityIndicator(RegistrationInProgressIndicator, RegisterButton);
        await CommonFunctions.AnimateButton((Button)sender);

        string userName = NameEntry.Text;
        string email = EmailEntry.Text;
        string pwd = PwdEntry.Text;

        UserCredential? user = null;
        try
        {
            // Register user with email and password
            user = await FirebaseAuthHelper.Instance.RegisterUserWithEmailAndPasswordAsync(email, pwd, userName);
        }
        catch (Exception ex)
        {
            await CommonFunctions.DisplayCustomAlertPage(
                "Failed to register user", ex.Message,
                "Close", "", true, false, AlertType.Error, Navigation);
            return;
        }
        finally
        {
            CommonFunctions.ToggleActivityIndicator(RegistrationInProgressIndicator, RegisterButton);
        }
        if (user != null)
        {
            // Registration successful
            await Toast.Make($"{user.User.Info.DisplayName} registered successfully!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();

            //await CommonFunctions.DisplayCustomAlertPage(
            //    "Success", $"{user.User.Info.DisplayName} registered successfully!", "Ok", "", true, false,
            //    AlertType.Info, Navigation);
            //var customAlertPage = new CustomAlertPage("Success", $"{user.User.Info.DisplayName} registered successfully!", "Ok", "", true, false);
            //await Navigation.PushModalAsync(customAlertPage);
            //await customAlertPage.WaitForUserResponseAsync();
            // Navigate to the next page or perform any other action
            OnRegistrationCompleted(sender, e, user.User);
        }
        else
        {
            // Registration failed
            await CommonFunctions.DisplayCustomAlertPage(
                "Error", "Failed to register user",
                "Close", "", true, false, AlertType.Error, Navigation);
            //var customAlertPage = new CustomAlertPage("Error", "Failed to register user", "Ok", "", true, false);
            //await Navigation.PushModalAsync(customAlertPage);
            //await customAlertPage.WaitForUserResponseAsync();
        }
    }
    private async void OnRegistrationCompleted(object sender, EventArgs e, User user)
    {
        //store userdata in sqlite db
        //SQLiteDBHelper dbHelper = new();
        userData = new()
        {
            Name = user.Info.DisplayName,
            UID = user.Uid,
            EmailAddress = user.Info.Email,
            DeviceInfo = JsonConvert.SerializeObject(getPhoneInfo())
        };

        try
        {
            dbHelper.InsertItem(userData);
        }
        catch (Exception)
        {
            await CommonFunctions.DisplayCustomAlertPage(
                "Error", "Failed to save user info, please contact app admins",
                "Close", "", true, false, AlertType.Error, Navigation);
        }
        Preferences.Set("UserRegistered", true);
        // Navigate to SignInPage after registration
        await Navigation.PushAsync(new SignInPage());
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
                EmailEntry.TextColor = Color.Parse("red");
                //EmailEntry.TextColor = Color.FromRgb(255, 0, 0); // Optional: Set text color to indicate invalid input
                // Optionally show an error message to the user
            }
            else
            {
                // Valid email format
                //EmailEntry.TextColor = Color.Parse("red");
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
        // Update the UI based on the validation
        LengthCriteria.IsVisible = !(password.Length >= 8);
        NumberCriteria.IsVisible = !(password.Any(char.IsDigit));
        SpecialCharCriteria.IsVisible = !(password.Any(ch => !char.IsLetterOrDigit(ch)));
        UpperCaseCriteria.IsVisible = !(password.Any(char.IsUpper));

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
        bool enableRegistrationBtn = enableRegisterButton(PwdEntry.Text);

        if (enableRegistrationBtn)
        {
            PwdConfirmEntry.TextColor = Color.FromRgb(0, 0, 0);
        }
        else
        {
            PwdConfirmEntry.TextColor = Color.Parse("red");
        }
        RegisterButton.IsEnabled = enableRegistrationBtn;
    }

    //private async void OnLoginTapped(object sender, EventArgs e)
    //{
    //    // Navigate to the login page or perform an action
    //    await Shell.Current.GoToAsync("//LoginPage");  // Example: Navigate to the login page
    //}

    private void OnPasswordRevealButtonClicked(object sender, EventArgs e)
    {
        // Toggle password visibility
        PwdEntry.IsPassword = !PwdEntry.IsPassword;

        // Change the eye icon accordingly
        PasswordRevealButton.Source = PwdEntry.IsPassword ? "eye_image_close.png" : "eye_image_open.png";
    }

    private void OnConfirmPasswordRevealButtonClicked(object sender, EventArgs e)
    {
        // Toggle password visibility
        PwdConfirmEntry.IsPassword = !PwdConfirmEntry.IsPassword;

        // Change the eye icon accordingly
        ConfirmPasswordRevealButton.Source = PwdConfirmEntry.IsPassword ? "eye_image_close.png" : "eye_image_open.png";
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

    // Method to handle the focus event and scroll the view
    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        var entry = sender as Entry;
        if (entry != null)
        {
            // Add some padding to bring whole form into view
            RegistrationPageVerticalStackLayout.Padding = new Thickness(20, 20, 20, 350);
        }

        if(entry == PwdEntry)
        {
            PasswordCriteriaStacklayout.IsVisible = true;
        }
    }
}