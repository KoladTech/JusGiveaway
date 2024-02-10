using SQLite;

namespace HeadsOrTails
{
    public partial class App : Application
    {
        public static SQLiteConnection? Database { get; set; }

        public App()
        {
            InitializeComponent();

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
                GameData currentGameState = new();

                currentGameState.ID = (int)Application.Current.Resources["ID"];
                currentGameState.PlayerName = (string)Application.Current.Resources["PlayerName"];
                currentGameState.PlayingHeads = (int)Application.Current.Resources["PlayingHeads"];
                currentGameState.HeadsCount = (int)Current.Resources["HeadsCount"];
                currentGameState.TailsCount = (int)Application.Current.Resources["TailsCount"];
                currentGameState.MaxPossibleWinnings = (int)Application.Current.Resources["MaxPossibleWinnings"];
                currentGameState.CurrentWinnings = (int)Application.Current.Resources["CurrentWinnings"];
                currentGameState.TotalResetsUsed = (int)Application.Current.Resources["TotalResetsUsed"];

                dbHelper.UpdateItem(currentGameState);
            }
        }
    }
}
