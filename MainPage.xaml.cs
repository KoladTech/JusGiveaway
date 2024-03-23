using Color = Microsoft.Maui.Graphics.Color;
using Firebase.Database;
using Firebase.Database.Query;


namespace HeadsOrTails
{
    public partial class MainPage : ContentPage
    {
        private string uID = string.Empty;
        private int headsCount = 0;
        private int tailsCount = 0;
        private int totalFlips = 0;
        private bool hasSelectedSides = false;
        private bool isHeadsChosen = true;
        private int score = 0;
        private int scoreAdjuster = 1; //helps with calculation of score
        private int currentWinnings = 0;
        private int maxPossibleWinnings = 0;
        private double totalGiveawayFunds = 0;
        private int totalResetsAllowed = 0;
        private const string coinFrontImage = "kobo_front.png";
        private const string coinBackImage = "kobo_back.png";

        // Define a counter to keep track of the number of resets
        private int resetCounter = 0;
        private int gameSessionId = 0;
        private readonly SQLiteDBHelper dbHelper;
        private readonly UserData userData;
        private GameData activeGame;
        FirebaseClient firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();

        public MainPage()
        {
            InitializeComponent();

            dbHelper = new();
            userData = dbHelper.GetUserData()[0];
            activeGame = dbHelper.GetRowWithMaxId();

            //MySQLHelper.ConnectAndQuery();

            //// Check if the code has been run before
            //if (!Preferences.ContainsKey("CodeExecuted"))
            //{
            //    // Code to run only once goes here
            //    //TODO Only do this when app first runs
            //    var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            //    using (Stream stream = assembly.GetManifestResourceStream("HeadsOrTails.HeadsorTailsGameData.db3"))
            //    {
            //        using (MemoryStream memoryStream = new MemoryStream())
            //        {
            //            stream.CopyTo(memoryStream);

            //            File.WriteAllBytes(SQLiteDBHelper.DbPath, memoryStream.ToArray());
            //        }
            //    }
            //    // Set a flag indicating that the code has been executed
            //    Preferences.Set("CodeExecuted", true);
            //}

            gameSessionId = dbHelper.GetMaxId();

            List<GameData> items = dbHelper.GetGameData();
            foreach (GameData item in items)
            {
                //Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Price: {item.Price}");
                isHeadsChosen = true;
            }

            GameData lastGame = new GameData();
            lastGame = dbHelper.GetRowWithMaxId();
            
            if (lastGame != null && lastGame.SelectedSides == 1)
            {
                uID = lastGame.UID;
                hasSelectedSides = lastGame.SelectedSides == 1;
                isHeadsChosen = lastGame.PlayingHeads == 1;
                headsCount = lastGame.HeadsCount;
                tailsCount = lastGame.TailsCount;
                totalFlips = headsCount + tailsCount;
                maxPossibleWinnings = lastGame.MaxPossibleWinnings;
                currentWinnings = lastGame.CurrentWinnings;
                resetCounter = lastGame.TotalResetsUsed;

                // Set UI with DB values
                UpdateCounts();

                //since we already have a game saved, setup the game from where we left off
                startGameUISetup(userData.Name, (isHeadsChosen ? "Heads" : "Tails"), maxPossibleWinnings, currentWinnings);

                SaveDataToProperties();
            }
            else if (lastGame != null)
            {
                hasSelectedSides = false;
                maxPossibleWinnings = activeGame.MaxPossibleWinnings;
                resetCounter = activeGame.TotalResetsUsed;
            }

            //TODO
            // Rotate the coin endlessly, until it dissappears from screen
            //for (int i = 0; i < 1000; i++)
            //{
            //    // Alternate between front and back images during rotation
            //    UserChoiceImage.Source = i % 2 == 0 ? "kobo_front.png" : "kobo_back.png";

            //    // Rotate the coin around the X-axis
            //    await UserChoiceImage.RotateXTo(180, 100, Easing.SinInOut); // Rotate the coin around the X-axis by 180 degrees in 200 milliseconds
            //    CoinImage.RotationX = 0; // Reset rotation to 0
            //}
            UpdateTotalGiveawayFundsLabelTimer();
        }

        public async Task<double> GetRemainingGiveawayFundsAsync()
        {
            var giveaway = await firebaseClient.Child("GiveAways/A").OnceSingleAsync<Dictionary<string, string>>();
            //totalResetsAllowed = int.Parse(giveaway["TotalResetsAllowed"]);
            return double.Parse(giveaway["LeftoverGiveawayFunds"]);
        }
        private void SaveDataToProperties()
        {
            // Store data in Application Properties
            if (Application.Current != null)
            {
                Application.Current.Resources["ID"] = activeGame.ID;
                Application.Current.Resources["PlayerName"] = userData.Name;
                Application.Current.Resources["UniqueID"] = userData.UID;
                Application.Current.Resources["PlayingHeads"] = isHeadsChosen ? 1 : 0;
                Application.Current.Resources["SelectedSides"] = hasSelectedSides ? 1 : 0;
                Application.Current.Resources["HeadsCount"] = headsCount;
                Application.Current.Resources["TailsCount"] = tailsCount;
                Application.Current.Resources["MaxPossibleWinnings"] = activeGame.MaxPossibleWinnings;
                Application.Current.Resources["CurrentWinnings"] = activeGame.CurrentWinnings;
                Application.Current.Resources["TotalResetsUsed"] = resetCounter;
            }
        }

        private async void OnChooseHeadsTapped(object sender, EventArgs e)
        {
            isHeadsChosen = true;
            await CoinBackImage.ScaleTo(0.8, 100);
            await CoinFrontImage.ScaleTo(1, 100);
            scoreAdjuster = 1;
            SelectionButton.IsEnabled = true;
        }

        private async void OnChooseTailsTapped(object sender, EventArgs e)
        {
            isHeadsChosen = false;
            await CoinFrontImage.ScaleTo(0.8, 100);
            await CoinBackImage.ScaleTo(1, 100);
            scoreAdjuster = -1;
            SelectionButton.IsEnabled = true;
        }


        private async void OnLockInSelectionClicked(object sender, EventArgs e)
        {
            await CommonFunctions.AnimateButton((Button)sender);

            string choice = isHeadsChosen ? "Heads" : "Tails";
            // Prompt the user to confirm their selection
            var customAlertPage = new CustomAlertPage($"Confirm Selection?", $"{userData.Name} : {choice}");
            await Navigation.PushModalAsync(customAlertPage);
            bool isConfirmed = await customAlertPage.WaitForUserResponseAsync();

            if (isConfirmed)
            {
                //uID = EncryptionHelper.Encrypt(playerPhoneNumber);

                hasSelectedSides = true;
                startGameUISetup(userData.Name, choice, maxPossibleWinnings, 0);

                SaveDataToProperties();
                GameData gameData = new()
                {
                    UID = userData.UID,
                    ID = activeGame.ID,
                    SelectedSides = 1,
                    PlayingHeads = isHeadsChosen ? 1 : 0,
                    MaxPossibleWinnings = maxPossibleWinnings,
                    TotalResetsUsed = resetCounter,
                    //store more stuff here...
                };
                activeGame.SelectedSides = 1;
                activeGame.PlayingHeads = isHeadsChosen ? 1 : 0;
                dbHelper.UpdateItem(activeGame);
                totalGiveawayFunds = await GetRemainingGiveawayFundsAsync();
                TotalGiveawayFundsLabel.Text = $"N{totalGiveawayFunds.ToString("NO")}";
            }
        }

        private async void OnCoinTapped(object sender, EventArgs e)
        {
            CashOutButton.IsVisible = false;
            CoinImage.IsEnabled = false;
            totalFlips += 50;

            Random random = new Random();

            // Rotate the coin animation
            for (int i = 0; i < 15; i++)
            {
                // Alternate between front and back images during rotation
                CoinImage.Source = i % 2 == 0 ? coinFrontImage : coinBackImage;

                // Rotate the coin around the X-axis
                await CoinImage.RotateXTo(180, 100, Easing.SinInOut); // Rotate the coin around the X-axis by 180 degrees in 200 milliseconds
                CoinImage.RotationX = 0; // Reset rotation to 0
            }

            // Generate a random number to determine the outcome of the flip
            int result = random.Next(2);

            // Update heads or tails count based on the random result
            if (result == 0)
            {
                //headsCount++;
                headsCount += 50;
                CoinImage.Source = coinFrontImage;
            }
            else
            {
                //tailsCount++;
                tailsCount += 50;
                CoinImage.Source = coinBackImage;
            }
            //score = (headsCount - tailsCount) * scoreAdjuster;

            // Update UI
            UpdateCounts();
            CoinImage.IsEnabled = true;

            CheckForWin();

            SaveDataToProperties();
        }

        private void UpdateCounts()
        {
            scoreAdjuster = isHeadsChosen ?1 : -1;
            score = (headsCount - tailsCount) * scoreAdjuster;
            ScoreLabel.Text = $"Score: {score}";
            ScoreLabel.TextColor = (score >= 10) ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0);

            TotalLabel.Text = $"Total Flips: {totalFlips}";

            // Calculate percentages
            double headsPercentage = (double)headsCount / totalFlips * 100;
            double tailsPercentage = (double)tailsCount / totalFlips * 100;

            // Update percentage bars
            HeadsProgressBar.Progress = headsPercentage / 100;
            TailsProgressBar.Progress = tailsPercentage / 100;

            HeadsProgressBarLabel.Text = $"Heads: {headsCount}";
            TailsProgressBarLabel.Text = $"Tails: {tailsCount}";

            // Update tails percentage label
            HeadsPercentageLabel.Text = $"{headsPercentage.ToString("0.0")}%";
            TailsPercentageLabel.Text = $"{tailsPercentage.ToString("0.0")}%";
        }


        private async void CheckForWin()
        {
            bool playerWon = false;
            bool canCashOut = currentWinnings >= activeGame.MinCashOut;
            string roundCompleteMsg = $"Round {(totalFlips / 100)} complete!!!";
            string cashOutMsg = string.Empty;
            string winMsg = string.Empty;
            string lossMsg = "You did not win this time.";
            string additionalMsg = string.Empty;

            // Check for win only when total flips reach a multiple of 100
            if (totalFlips >= 100 && totalFlips % 100 == 0)
            {
                if (isHeadsChosen && score >= 10)
                {
                    playerWon = true;
                    currentWinnings += 1000; // Increment score by 1000
                }
                else if (!isHeadsChosen && score >= 10)
                {
                    playerWon = true;
                    currentWinnings += 1000; // Increment score by 1000
                }
                else
                {
                    playerWon = false;

                    if (currentWinnings > 0)
                    {
                        lossMsg = "You lost N1000!";
                        currentWinnings -= 1000; // Deduct 1000 from score
                    }
                }

                canCashOut = currentWinnings >= activeGame.MinCashOut;
                cashOutMsg = canCashOut ? "\nYou can cashout or Play for more" : "";
                winMsg = $"You win N1000! {cashOutMsg}";
                CashOutButton.IsVisible = canCashOut;

                PotentialWinningLabel.Text = $"Current Winnings: N{currentWinnings}";

                if (playerWon)
                {
                    if (isMaxWinAchieved())
                    {
                        winMsg = $"You can now cashout your winnings of N{currentWinnings}";
                    }
                    additionalMsg = winMsg;
                }
                else
                {
                    // Player did not meet winning criteria
                    additionalMsg = lossMsg;
                }

                var customAlertPage = new CustomAlertPage(roundCompleteMsg, additionalMsg, "Ok", "Cancel", true, false);
                await Navigation.PushModalAsync(customAlertPage);
                await customAlertPage.WaitForUserResponseAsync();

                //save progress to db
                GameData currentGameState = dbHelper.GetRowWithMaxId();

                //currentGameState.ID = gameSessionId;
                //currentGameState.PlayerName = playerName;
                //currentGameState.PlayingHeads = isHeadsChosen ? 1 : 0;
                currentGameState.HeadsCount = headsCount;
                currentGameState.TailsCount = tailsCount;
                //currentGameState.MaxPossibleWinnings = totalPossibleWinnings;
                currentGameState.CurrentWinnings = currentWinnings;
                //currentGameState.TotalResetsUsed = resetCounter;

                var et = dbHelper.UpdateItem(currentGameState);
                et = et;
            }
        }
        // Event handler for the reset button
        private async void OnResetClicked(object sender, EventArgs e)
        {
            // Disable the button
            Button button = (Button)sender;
            button.IsEnabled = false;

            try
            {
                await CommonFunctions.AnimateButton((Button)sender);

                //ResetButtonFrame.BackgroundColor = Color.FromRgb(169, 169, 169);
                //ResetButton.IsEnabled = false;

                var customAlertPage = new CustomAlertPage("Reset Game?", $"Your total maximum winnings will be reduced to N{1000 * (totalResetsAllowed - resetCounter - 1)}!");
                await Navigation.PushModalAsync(customAlertPage);
                bool reset = await customAlertPage.WaitForUserResponseAsync();

                //bool reset = await DisplayAlert("Reset Game?", $"Your total maximum winnings will be reduced to N{1000 * (MaxResets-resetCounter-1)}!", "Yes", "No");
                if (reset)
                {
                    if (resetCounter < totalResetsAllowed)
                    {
                        // Reduce totalPossibleWinnings by 1000
                        maxPossibleWinnings -= 1000;

                        // Increment the reset counter
                        resetCounter++;

                        // Reset game state and UI
                        ResetGame();

                        SaveDataToProperties();
                    }
                }
                if (resetCounter == totalResetsAllowed - 1)
                {
                    ResetButton.IsEnabled = false;
                }

                ResetButton.IsEnabled = true;
                hasSelectedSides = false;
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        // Function to reset the game state and UI
        private void ResetGame()
        {
            // Reset game-related variables (e.g., headsCount, tailsCount, totalFlips, score)
            headsCount = 0;
            tailsCount = 0;
            totalFlips = 0;
            score = 0;
            currentWinnings = 0;
            hasSelectedSides = false;

            //TODO : new entry in db, maybe do it here then update in lock in
            GameData newGameState = new()
            {
                UID = activeGame.UID,
                SelectedSides = 0,
                MaxPossibleWinnings = maxPossibleWinnings,
                TotalResetsUsed = resetCounter,
                MinCashOut = activeGame.MinCashOut
            };
            dbHelper.InsertItem(newGameState);
            gameSessionId = dbHelper.GetMaxId();
            activeGame = dbHelper.GetRowWithMaxId();

            ResetUI();
        }
        private void ResetUI()
        {
            ScoreLabel.Text = "Score: 0";
            TotalLabel.Text = "Total Flips: 0";

            // Update percentage bars
            HeadsProgressBar.Progress = 0;
            TailsProgressBar.Progress = 0;

            HeadsProgressBarLabel.Text = "Heads: 0";
            TailsProgressBarLabel.Text = "Tails: 0";

            // Update tails percentage label
            HeadsPercentageLabel.Text = "0%";
            TailsPercentageLabel.Text = "0%";


            PotentialWinningLabel.Text = "Potential Winnings: N0";
            TotalPossibleWinningsLabel.Text = $"Maximum win : N{1000 * (totalResetsAllowed - resetCounter)}";

            SelectionButton.IsVisible = true;
            UserChoiceLabel.Text = "Heads or Tails";
            CoinFlipLayout.IsVisible = false;
            CoinFrontImage.IsVisible = true;
            CoinBackImage.IsVisible = true;
            ResetButton.IsVisible = false;
            InformationLayout.IsVisible = false;
            SideSelectionLayout.IsVisible = true;
            CashOutButton.IsVisible = false;

        }

        private void startGameUISetup(string username, string choice, int maxPossibleWinning, int potentialWinning)
        {
            UserChoiceLabel.Text = $"{username} : {choice}";
            TotalPossibleWinningsLabel.Text = $"Maximum Win: N{maxPossibleWinning}";
            PotentialWinningLabel.Text = $"Potential Winnings: N{potentialWinning}";
            SelectionButton.IsVisible = false;
            SideSelectionLayout.IsVisible = false;
            CoinFlipLayout.IsVisible = true;
            ResetButton.IsVisible = true;
            InformationLayout.IsVisible = true;
            Titlelayout.Orientation = StackOrientation.Horizontal;
            UserChoiceImage.IsVisible = false;
            TotalGiveawayFundsLabel.Text = $"N{totalGiveawayFunds}";
        }

        private bool isMaxWinAchieved()
        {
            if (currentWinnings == maxPossibleWinnings)
            {
                LockGame();
                return true;
            }
            return false;
        }

        private async void UpdateTotalGiveawayFundsLabelTimer()
        {
            //if update has not happened in about 15 minutes, ask for player to connect to data to avoid 
            //playing for no reason

            // Set up a timer to read the value from the Firebase Realtime Database every x seconds
            TimeSpan interval = TimeSpan.FromSeconds(5);
            while (true)
            {
                // Read the value from the Firebase Realtime Database
                totalGiveawayFunds = await GetRemainingGiveawayFundsAsync();

                //Update the label with the retrieved value
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TotalGiveawayFundsLabel.Text = $"N{totalGiveawayFunds.ToString("N0")}";
                });

                // Wait for the specified interval before reading again
                await Task.Delay(interval);
            }
        }

        private async void OnCashOutClicked(object sender, EventArgs e)
        {
            // Disable the button
            Button button = (Button)sender;
            button.IsEnabled = false;

            try
            {
                await CommonFunctions.AnimateButton((Button)sender);

                var customAlertPage = new CustomAlertPage("Thank you for playing!", $"Are you sure you want to cashout your winnings of N{currentWinnings}?", "Yes", "No", true, true);
                await Navigation.PushModalAsync(customAlertPage);
                bool cashOutNow = await customAlertPage.WaitForUserResponseAsync();

                if (cashOutNow)
                {
                    LockGame();
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

                        customAlertPage = new CustomAlertPage("Cash Out Success", "We will be in touch soon", "Ok", "", true, false);
                        await Navigation.PushModalAsync(customAlertPage);
                        bool cashedout = await customAlertPage.WaitForUserResponseAsync();

                        if (cashedout)
                        {
                            button.IsVisible = false;
                            UpdateLeftoverGiveawayFunds();
                        }
                    }
                    catch (FirebaseException ex)
                    {
                        Console.WriteLine($"Error posting data to Firebase: {ex.Message}");
                        // Handle the exception as needed. Maybe store in sqlite and try 
                        //to send to firebase later
                        customAlertPage = new CustomAlertPage("Error", "Cash out failed, please try agian", "Ok", "", true, false);
                        await Navigation.PushModalAsync(customAlertPage);
                        await customAlertPage.WaitForUserResponseAsync();
                    }
                }
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void UpdateLeftoverGiveawayFunds()
        {
            var firebaseObj = firebaseClient.Child("GiveAways/A/LeftoverGiveawayFunds");
            var leftOverGiveawayFunds = await firebaseObj.OnceSingleAsync<string>();
            await firebaseObj.PutAsync(int.Parse(leftOverGiveawayFunds) - activeGame.CurrentWinnings);
        }
        private void LockGame()
        {
            ResetButton.IsVisible = false;
            CashOutButton.IsVisible = true;
            CoinImage.IsEnabled = false;
            CoinImage.Source = "coin_locked.png";

            //Write to db signifying end of game

        }
    }
}
