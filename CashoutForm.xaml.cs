using Firebase.Database;
using Firebase.Database.Query;

namespace JusGiveaway;

public partial class CashoutForm : ContentPage
{
    private readonly UserData userData;
    private GameData activeGame;
    private readonly FirebaseClient firebaseClient;

    public CashoutForm(UserData userData, GameData gameData)
	{
		InitializeComponent();
        this.userData = userData;
        this.activeGame = gameData;
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();

    }

    private async void OnSubmitCashoutClicked(object sender, EventArgs e)
    {
        // Get values from input fields
        string firstName = FirstName.Text;
        string lastName = LastName.Text;
        string bankAccountNo = BankAccountNo.Text;
        string bankName = BankName.Text;

        // Update user information label
        UserInfoLabel.Text = $"User: {firstName} {lastName}\nCashout Amount: $100"; // Change $100 to the actual cashout amount

        CashOutDetails cashOutDetails = new()
        {
            Name = userData.Name,
            UID = userData.UID,
            EmailAddress = userData.EmailAddress,
            Sex = null,
            DeviceInfo = userData.DeviceInfo ?? "",
            BankAccountNumber = "0123456789",
            CashoutAmount = activeGame.CurrentWinnings
        };

        try
        {
            //Post the dictionary to Firebase under the specified parent and child keys
            await firebaseClient.Child("Cashouts").Child(userData.UID).PatchAsync(cashOutDetails);

            var customAlertPage = new CustomAlertPage("Cash Out Success", "We will be in touch soon", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            bool cashedout = await customAlertPage.WaitForUserResponseAsync();

            if (cashedout)
            {
                UpdateLeftoverGiveawayFunds();
            }
        }
        catch (FirebaseException ex)
        {
            Console.WriteLine($"Error posting data to Firebase: {ex.Message}");
            // Handle the exception as needed. Maybe store in sqlite and try 
            //to send to firebase later
            var customAlertPage = new CustomAlertPage("Error", "Cash out failed, please try agian", "Ok", "", true, false);
            await Navigation.PushModalAsync(customAlertPage);
            await customAlertPage.WaitForUserResponseAsync();
        }
    }

    private async void UpdateLeftoverGiveawayFunds()
    {
        var firebaseObj = firebaseClient.Child("Giveaways/A/LeftoverGiveawayFunds");
        var leftOverGiveawayFunds = await firebaseObj.OnceSingleAsync<string>();
        await firebaseObj.PutAsync(int.Parse(leftOverGiveawayFunds) - activeGame.CurrentWinnings);
    }
}