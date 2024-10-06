using Firebase.Database;
using System;
using System.Threading.Tasks;

namespace JusGiveaway;

public partial class CountdownPage : ContentPage
{

    private readonly SQLiteDBHelper dbHelper;
    private readonly List<UserData> userData;
    private readonly FirebaseClient firebaseClient;
    private DateTime _targetDate;
    private bool _isCountdownRunning;
    public CountdownPage(Dictionary<string, string> firebaseGiveawayData)
	{
		InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData();
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();

        StartCountdown(new DateTime(2024, 9, 20, 22, 45, 0),firebaseGiveawayData); // Set your target date here
    }

    private void StartCountdown(DateTime targetDate, Dictionary<string, string> firebaseGiveawayData)
    {
        _targetDate = targetDate;
        _isCountdownRunning = true;

        //BindableObject.Dispatcher.StartTimer

        // Use Dispatcher to update UI on the main thread
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (_isCountdownRunning)
            {
                TimeSpan remainingTime = _targetDate - DateTime.Now;

                if (remainingTime.TotalSeconds <= 0)
                {
                    //Countdown is complete, navigate to another page
                    _isCountdownRunning = false;
                    CountdownLabel.Text = "00d: 00h: 00m: 00s";
                    OnCountdownCompleted(firebaseGiveawayData);
                    return false; // Stops the timer
                }
                else
                {
                    //Update countdown label with remaining time
                    CountdownLabel.Text = string.Format(
                        "{0}d: {1}h: {2}m: {3}s",
                        remainingTime.Days,
                        remainingTime.Hours,
                        remainingTime.Minutes,
                        remainingTime.Seconds
                    );
                }
            }
            return true; // Continue the timer until stopped
        });
    }

    private async void OnCountdownCompleted(Dictionary<string, string> firebaseGiveawayData)
    {
        // Perform navigation to another page (replace with your target page)
        await Task.Delay(1000); // Optionally delay before navigating
        await Navigation.PushAsync(new HeadsOrTails(firebaseGiveawayData));

    }
}