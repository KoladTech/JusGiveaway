﻿using SQLite;

namespace HeadsOrTails
{
    public partial class App : Application
    {
        public static SQLiteConnection? Database { get; set; }
        SQLiteDBHelper dbHelper;

        public App()
        {
            InitializeComponent();
            dbHelper = new();

            //// Check if the user has already registered
            //bool isRegistered = CheckRegistrationStatus();

            //Preferences.Remove("UserRegistered");
            //Preferences.Remove("UserSignedIn");
            //Preferences.Remove("CodeExecuted");


            //if (!Preferences.ContainsKey("UserSignedIn"))
            //{
            //    // If not registered, navigate to the registration page
            //    MainPage = new NavigationPage(new RegistrationPage());
            //}
            //else
            //{
            //    // If registered, navigate to the main page
            //    MainPage = new NavigationPage(new MainPage());
            //}

            //MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            // Set the initial page to RegistrationPage
            if (!Preferences.ContainsKey("UserRegistered"))
            {
                return new Window(new NavigationPage(new RegistrationPage()));
            }

            // Set the next page to SignInPage
            if (!Preferences.ContainsKey("UserSignedIn"))
            {
                return new Window(new NavigationPage(new SignInPage()));
            }

            return new Window(new NavigationPage(new MainPage()));
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
            // Perform your save operations here using dbHelper
            // Store data in Application Properties
            if (Application.Current != null)
            {
                GameData currentGameState = dbHelper.GetRowWithMaxId();
                currentGameState.HeadsCount = (int)Current.Resources["HeadsCount"];
                currentGameState.TailsCount = (int)Current.Resources["TailsCount"];
                currentGameState.CurrentWinnings = (int)Current.Resources["CurrentWinnings"];

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
