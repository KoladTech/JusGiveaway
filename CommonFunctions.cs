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

        public static int GetGameWinMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["GameWinMonetaryValue"]);
        }
        public static int GetGameLossMonetaryValue(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["GameLossMonetaryValue"]);
        }

        public static int GetLeftoverGiveawayFunds(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["LeftoverGiveawayFunds"]);
        }

        public static int GetTotalResetsAllowed(Dictionary<string, string> giveawayData)
        {
            return int.Parse(giveawayData["TotalResetsAllowed"]);
        }
    }

}
