using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JusGiveaway.CustomAlertPage;

namespace JusGiveaway
{
    public static class CommonFunctions
    {
        public const string coinFrontImage = "kobo_front.png";
        public const string coinBackImage = "kobo_back.png";

        public static async Task AnimateButton(Button button)
        {
            await button.ScaleTo(1.15, 100);
            await button.ScaleTo(1, 100);
        }

        public static async Task AnimateButton(ImageButton button)
        {
            await button.ScaleTo(1.15, 100);
            await button.ScaleTo(1, 100);
        }

        public static async Task AnimateSpan(Label label)
        {
            // Save the original scale
            double originalScale = label.Scale;

            // Animate the label to 1.5 times its original scale
            await label.ScaleTo(1.5, 250, Easing.CubicInOut);

            // Animate back to the original scale
            await label.ScaleTo(originalScale, 250, Easing.CubicInOut);
        }

        public static async Task<Dictionary<string, string>> GetGameDataFromFirebaseAsync(FirebaseClient firebaseClient)
        {
            try
            {
                // Get data from Firebase Realtime Database
                var data = await firebaseClient.Child("Giveaways/A").OnceSingleAsync<Dictionary<string, string>>();

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving data: {ex.Message}");
                throw;
            }
        }

        public static async Task<bool> DisplayCustomAlertPage(
            string title, 
            string message, 
            string primaryBtnText, 
            string secondaryBtnText, 
            bool showPrimaryBtn, 
            bool showSecondaryBtn,
            AlertType alertType,
            INavigation navigation)
        {
            var customAlertPage = new CustomAlertPage(title, message, 
                primaryBtnText, secondaryBtnText, 
                showPrimaryBtn, showSecondaryBtn,
                alertType);
            await navigation.PushModalAsync(customAlertPage);
            return await customAlertPage.WaitForUserResponseAsync();
        }

        public static void ToggleActivityIndicator(ActivityIndicator activityIndicator, Button buttonClicked)
        {
            buttonClicked.IsEnabled = !buttonClicked.IsEnabled;
            //activityIndicator.IsVisible = !activityIndicator.IsVisible;
            activityIndicator.IsRunning = !activityIndicator.IsRunning;
        }

        public static async Task<double> GetRemainingGiveawayFundsAsync(FirebaseClient firebaseClient)
        {
            // Read updated values from the Firebase Realtime Database
            var firebaseGiveawayData = await GetGameDataFromFirebaseAsync(firebaseClient);

            if (firebaseGiveawayData == null)
            {
                return 0;
            }

            return double.Parse(firebaseGiveawayData["LeftoverGiveawayFunds"]);
        }

        public static int GetRoundSmallLossMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["RoundSmallLossMonetaryValue"]);
        }
        public static int GetRoundBigLossMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["RoundBigLossMonetaryValue"]);
        }
        public static int GetRoundSmallWinMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["RoundSmallWinMonetaryValue"]);
        }
        public static int GetRoundBigWinMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["RoundBigWinMonetaryValue"]);
        }
        public static int GetRoundDrawMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["RoundDrawMonetaryValue"]);
        }
        public static int GetLeftoverGiveawayFunds(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["LeftoverGiveawayFunds"]);
        }
        public static int GetMinCashoutPerPerson(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["MinCashoutPerPerson"]);
        }

        public static int GetTotalResetsAllowed(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["TotalResetsAllowed"]);
        }

        public static int GetMonetaryResetPenalty(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["MonetaryResetPenalty"]);
        }

        public static void RemovePagesFromNavigationStack(INavigation navigation)
        {
            // Remove the previous page(s) from the navigation stack
            // this will prevent the back button going back to unwanted pages
            var existingPages = navigation.NavigationStack.ToList();
            var lastPage = navigation.NavigationStack.Last();
            foreach (var page in existingPages)
            {
                // Remove all pages except the most recent
                if (page != lastPage)
                {
                    navigation.RemovePage(page);
                }
            }
        }

        // General method to animate both increment and decrement
        public static async void AnimateLabelChange(Label numberLabel, int startValue, int endValue, int duration = 2000)
        {
            int stepDuration = 20; // Duration between each step (in milliseconds)
            int steps = duration / stepDuration; // Total number of steps for the animation

            // Calculate increment/decrement value per step based on whether we are incrementing or decrementing
            double incrementValue = (double)(endValue - startValue) / steps;
            double currentValue = startValue;

            // Perform the animation in a loop
            for (int i = 0; i < steps; i++)
            {
                currentValue += incrementValue; // Increment/decrement the value
                numberLabel.Text = $"N{currentValue.ToString("N0")}"; // Update the label text
                await Task.Delay(stepDuration); // Wait for the next step
            }

            // Set the final value to ensure precision
            numberLabel.Text = $"N{endValue.ToString("N0")}";
        }
    }

}
