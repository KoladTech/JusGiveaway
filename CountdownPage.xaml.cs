using Firebase.Database;
using System;
using System.Threading.Tasks;
using static JusGiveaway.CustomAlertPage;

namespace JusGiveaway;

public partial class CountdownPage : ContentPage
{

    private readonly SQLiteDBHelper dbHelper;
    private readonly List<UserData> userData;
    private readonly FirebaseClient firebaseClient;
    private DateTime giveawayStartDate;
    private bool _isCountdownRunning;
    public CountdownPage(Dictionary<string, string> firebaseGiveawayData)
	{
		InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData();
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();

        StartCountdown(firebaseGiveawayData);
    }

    private async void StartCountdown(Dictionary<string, string> firebaseGiveawayData)
    {
        GetGiveawayStartDate(firebaseGiveawayData);
        _isCountdownRunning = true;

        while (_isCountdownRunning)
        {
            TimeSpan remainingTime = giveawayStartDate - DateTime.Now;

            if (remainingTime.TotalSeconds <= 0)
            {
                // Countdown is complete, navigate to another page
                _isCountdownRunning = false;
                DaysLabel.Text = "00";
                HoursLabel.Text = "00";
                MinutesLabel.Text = "00";
                SecondsLabel.Text = "00";
                OnCountdownCompleted(firebaseGiveawayData);
                break; // Exit the loop
            }
            else
            {
                // Update the label text based on the remaining time
                DaysLabel.Text = remainingTime.Days.ToString("D2");    // Display as 2-digit
                HoursLabel.Text = remainingTime.Hours.ToString("D2");
                MinutesLabel.Text = remainingTime.Minutes.ToString("D2");
                SecondsLabel.Text = remainingTime.Seconds.ToString("D2");
            }

            await Task.Delay(1000); // Wait for 1 second before updating again
        }
    }

    private async void OnCountdownCompleted(Dictionary<string, string> firebaseGiveawayData)
    {
        // Perform navigation to another page (replace with your target page)
        await Task.Delay(1000); // Optionally delay before navigating
        await Navigation.PushAsync(new HeadsOrTails(firebaseGiveawayData));

    }

    private async void GetGiveawayStartDate(Dictionary<string, string> firebaseGiveawayData)
    {
        string dateString = firebaseGiveawayData["StartDate"];

        if (DateTime.TryParseExact(dateString, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out giveawayStartDate))
        {
            Console.WriteLine(giveawayStartDate); // Successfully parsed: 10/08/2024 00:00:00
        }
        else
        {
            await CommonFunctions.DisplayCustomAlertPage("Error", "Invalid starting date format retrieved from database", "Close", "", true, false, AlertType.Error, Navigation);
        }

    }
}