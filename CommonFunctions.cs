using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    public static class CommonFunctions
    {
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
                return null;
            }
        }
    }

}
