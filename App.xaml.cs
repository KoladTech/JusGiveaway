using SQLite;

namespace HeadsOrTails
{
    public partial class App : Application
    {
        public static SQLiteConnection? Database { get; set; }

        public App()
        {
            InitializeComponent();

            //// Check if the user has already registered
            //bool isRegistered = CheckRegistrationStatus();

            //if (!isRegistered)
            //{
            //    // If not registered, navigate to the registration page
            //    MainPage = new NavigationPage(new RegistrationPage());
            //}
            //else
            //{
            //    // If registered, navigate to the main page
            //    MainPage = new NavigationPage(new MainPage());
            //}

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Save progress in the SQLite database
            SaveProgressToDatabase();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void SaveProgressToDatabase()
        {
            // Perform database operations to save progress
            // For example, you can use your SQLiteDBHelper class to save data
            SQLiteDBHelper dbHelper = new();
            // Perform your save operations here using dbHelper
            // Store data in Application Properties
            if (Application.Current != null)
            {
                GameData currentGameState = new()
                {
                    ID = (int)Application.Current.Resources["ID"],
                    PlayerName = (string)Application.Current.Resources["PlayerName"],
                    PlayerNumber = (string)Application.Current.Resources["PlayerNumber"],
                    UniqueID = (string)Application.Current.Resources["UniqueID"],
                    PlayingHeads = (int)Application.Current.Resources["PlayingHeads"],
                    SelectedSides = (int)Application.Current.Resources["SelectedSides"],
                    HeadsCount = (int)Current.Resources["HeadsCount"],
                    TailsCount = (int)Application.Current.Resources["TailsCount"],
                    MaxPossibleWinnings = (int)Application.Current.Resources["MaxPossibleWinnings"],
                    CurrentWinnings = (int)Application.Current.Resources["CurrentWinnings"],
                    TotalResetsUsed = (int)Application.Current.Resources["TotalResetsUsed"]
                };

                dbHelper.UpdateItem(currentGameState);
            }
        }
        private bool CheckRegistrationStatus()
        {
            // Implement logic to check if the user has already registered
            // You can check a flag in the application's settings or a local database
            // Return true if registered, false otherwise
            return false; // Placeholder logic, replace with actual implementation
        }
    }
}
