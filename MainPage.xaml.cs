using SQLite;
using System.Text;
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;


namespace HeadsOrTails
{
    public partial class MainPage : ContentPage
    {
        private string playerName = string.Empty;
        private string playerPhoneNumber = string.Empty;
        private string uniqueID = string.Empty;
        private bool showTextBoxes = true;
        private int headsCount = 0;
        private int tailsCount = 0;
        private int totalFlips = 0; // Total number of coin flips
        // Define a boolean variable to hold the player's choice
        private bool hasSelectedSides = false;
        private bool isHeadsChosen = true; // Default choice is heads
        private int score = 0;
        private int scoreAdjuster = 1;
        private int currentWinnings = 0;
        // Define the maxPossibleWinnings variable with an initial value of 20000
        private int maxPossibleWinnings = 20000;

        // Define the maximum number of resets allowed
        private const int MaxResets = 20;
        private const string coinFrontImage = "kobo_front.png";
        private const string coinBackImage = "kobo_back.png";

        // Define a counter to keep track of the number of resets
        private int resetCounter = 0;
        private int gameSessionId = 0;
        SQLiteDBHelper dbHelper = new();



        public MainPage()
        {
            InitializeComponent();
            //MySQLHelper.ConnectAndQuery();

            // Check if the code has been run before
            if (!Preferences.ContainsKey("CodeExecuted"))
            {
                // Code to run only once goes here
                //TODO Only do this when app first runs
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("HeadsOrTails.HeadsorTailsGameData.db3"))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);

                        File.WriteAllBytes(SQLiteDBHelper.DbPath, memoryStream.ToArray());
                    }
                }
                // Set a flag indicating that the code has been executed
                Preferences.Set("CodeExecuted", true);
            }

            gameSessionId = dbHelper.GetMaxId();

            List<GameData> items = dbHelper.GetItems();
            foreach (GameData item in items)
            {
                //Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Price: {item.Price}");
                isHeadsChosen = true;
            }

            GameData lastGame = new GameData();
            lastGame = dbHelper.GetRowWithMaxId();
            

            if (lastGame == null)
            {
                dbHelper.InsertItem(new GameData());
            }
            else if (lastGame != null && lastGame.SelectedSides == 1)
            {
                playerName = lastGame.PlayerName;
                playerPhoneNumber = lastGame.PlayerNumber;
                uniqueID = lastGame.UniqueID;
                hasSelectedSides = lastGame.SelectedSides == 1 ? true : false;
                isHeadsChosen = lastGame.PlayingHeads == 1 ? true : false;
                headsCount = lastGame.HeadsCount;
                tailsCount = lastGame.TailsCount;
                totalFlips = headsCount + tailsCount;
                maxPossibleWinnings = lastGame.MaxPossibleWinnings;
                currentWinnings = lastGame.CurrentWinnings;
                resetCounter = lastGame.TotalResetsUsed;
                
                // Set UI with DB values
                UpdateCounts();
                //PotentialWinningLabel.Text = $"Potential Winnings: N{currentWinnings}";
                //TotalPossibleWinningsLabel.Text = $"Maximum win : N{1000 * (MaxResets - resetCounter)}";

                //since we already have a game saved, setup the game from where we left off
                startGameUISetup(playerName, (isHeadsChosen ? "Heads" : "Tails"), maxPossibleWinnings, currentWinnings);
            }
            else if (lastGame != null)
            {
                hasSelectedSides = false;
                playerName = lastGame.PlayerName;
                playerPhoneNumber = lastGame.PlayerNumber;
                maxPossibleWinnings = lastGame.MaxPossibleWinnings;
                resetCounter = lastGame.TotalResetsUsed;
                showTextBoxes = playerName == "" ? true : false;
                NameEntryFrame.IsVisible = showTextBoxes;
                PhoneNumberEntryFrame.IsVisible = showTextBoxes;
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

        }
        private void SaveDataToProperties()
        {
            // Store data in Application Properties
            if (Application.Current != null)
            {
                Application.Current.Resources["ID"] = gameSessionId;
                Application.Current.Resources["PlayerName"] = playerName;
                Application.Current.Resources["PlayerNumber"] = playerPhoneNumber;
                Application.Current.Resources["UniqueID"] = uniqueID;
                Application.Current.Resources["PlayingHeads"] = isHeadsChosen ? 1 : 0;
                Application.Current.Resources["SelectedSides"] = hasSelectedSides ? 1 : 0;
                Application.Current.Resources["HeadsCount"] = headsCount;
                Application.Current.Resources["TailsCount"] = tailsCount;
                Application.Current.Resources["MaxPossibleWinnings"] = maxPossibleWinnings;
                Application.Current.Resources["CurrentWinnings"] = currentWinnings;
                Application.Current.Resources["TotalResetsUsed"] = resetCounter;
            }
        }

        private async void OnChooseHeadsTapped(object sender, EventArgs e)
        {
            isHeadsChosen = true;
            await CoinBackImage.ScaleTo(0.8, 100);
            await CoinFrontImage.ScaleTo(1, 100);
            scoreAdjuster = 1;
            SelectionButtonFrame.IsVisible = true;
        }

        private async void OnChooseTailsTapped(object sender, EventArgs e)
        {
            isHeadsChosen = false;
            await CoinFrontImage.ScaleTo(0.8, 100);
            await CoinBackImage.ScaleTo(1, 100);
            scoreAdjuster = -1;
            SelectionButtonFrame.IsVisible = true;
        }


        private async void OnLockInSelectionClicked(object sender, EventArgs e)
        {
            await SelectionButtonFrame.ScaleTo(1.3, 100);
            await SelectionButtonFrame.ScaleTo(1, 100);

            string choice = isHeadsChosen ? "Heads" : "Tails";
            // Prompt the user to confirm their selection
            var customAlertPage = new CustomAlertPage($"Confirm?", $"Username - {playerName}\nLast Digits of # - {playerPhoneNumber}\nPlaying : {choice}");
            await Navigation.PushModalAsync(customAlertPage);
            bool isConfirmed = await customAlertPage.WaitForUserResponseAsync();
            //bool isConfirmed = await DisplayAlert($"Confirm UserName and Choice : {choice}?", $"Are you sure you want to lock in {playerName} as your Username and {choice} as your choice?", "Yes", "No");

            if (isConfirmed)
            {
                uniqueID = EncryptionHelper.Encrypt(playerPhoneNumber);

                hasSelectedSides = true;
                startGameUISetup(playerName, choice, maxPossibleWinnings, 0);

                SaveDataToProperties();
                GameData gameData = new()
                {
                    PlayerName = playerName,
                    PlayerNumber = playerPhoneNumber,
                    UniqueID = uniqueID,
                    ID = dbHelper.GetMaxId(),
                    SelectedSides = 1,
                    PlayingHeads = isHeadsChosen ? 1 : 0,
                    MaxPossibleWinnings = maxPossibleWinnings,
                    TotalResetsUsed = resetCounter,
                    //store more stuff here...
                };
                dbHelper.UpdateItem(gameData);
            }
        }

        private async void OnCoinTapped(object sender, EventArgs e)
        {
            CashOutButtonFrame.IsVisible = false;
            //CoinImage.IsEnabled = false;
            totalFlips += 1;

            Random random = new Random();

            // Rotate the coin animation
            for (int i = 0; i < 15; i++)
            {
                // Alternate between front and back images during rotation
                CoinImage.Source = i % 2 == 0 ? "kobo_front.png" : "kobo_back.png";

                // Rotate the coin around the X-axis
                await CoinImage.RotateXTo(180, 100, Easing.SinInOut); // Rotate the coin around the X-axis by 180 degrees in 200 milliseconds
                CoinImage.RotationX = 0; // Reset rotation to 0
            }

            // Generate a random number to determine the outcome of the flip
            int result = random.Next(2);

            // Update heads or tails count based on the random result
            if (result == 0)
            {
                headsCount++;
                //headsCount += 10;
                CoinImage.Source = coinFrontImage;
            }
            else
            {
                tailsCount++;
                //tailsCount += 10;
                CoinImage.Source = coinBackImage;
            }
            //score = (headsCount - tailsCount) * scoreAdjuster;

            // Update UI
            UpdateCounts();
            //CoinImage.IsEnabled = true;

            CheckForWin();

            SaveDataToProperties();
        }

        private void OnUserNameTextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the Entry control
            Entry entry = (Entry)sender;

            // Remove any spaces from the entered text
            entry.Text = entry.Text.Replace(" ", string.Empty);
            playerName = entry.Text;
        }

        private void OnPhoneNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the Entry control
            Entry entry = (Entry)sender;

            // Get the current text from the Entry
            string text = entry.Text;

            // Remove any non-numeric characters from the text
            string newText = string.Concat(text.Where(char.IsDigit));

            // If the new text is different from the current text,
            // update the Entry's text to the new filtered text
            if (newText != text)
            {
                entry.Text = newText;
            }
            playerPhoneNumber = entry.Text;
        }
        private void UpdateCounts()
        {
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
            bool canCashOut = currentWinnings >= 2000;
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

                canCashOut = currentWinnings >= 1000;
                cashOutMsg = canCashOut ? "\nYou can cashout or Play for more" : "";
                winMsg = $"You win N1000! {cashOutMsg}";
                CashOutButtonFrame.IsVisible = canCashOut;

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
                SQLiteDBHelper dbHelper = new();
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
            //bool reset = false;

            await ResetButtonFrame.ScaleTo(1.3, 100);
            await ResetButtonFrame.ScaleTo(1, 100);
            //ResetButtonFrame.BackgroundColor = Color.FromRgb(169, 169, 169);
            //ResetButton.IsEnabled = false;

            var customAlertPage = new CustomAlertPage("Reset Game?", $"Your total maximum winnings will be reduced to N{1000 * (MaxResets - resetCounter - 1)}!");
            await Navigation.PushModalAsync(customAlertPage);
            bool reset = await customAlertPage.WaitForUserResponseAsync();

            //bool reset = await DisplayAlert("Reset Game?", $"Your total maximum winnings will be reduced to N{1000 * (MaxResets-resetCounter-1)}!", "Yes", "No");
            if (reset)
            {
                if (resetCounter < MaxResets)
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
            if (resetCounter == MaxResets - 1)
            {
                ResetButton.IsEnabled = false;
            }

            ResetButton.IsEnabled = true;
            hasSelectedSides = false;
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
                PlayerName = playerName,
                PlayerNumber = playerPhoneNumber,
                UniqueID = uniqueID,
                SelectedSides = hasSelectedSides ? 0 : 1,
                MaxPossibleWinnings = maxPossibleWinnings,
                TotalResetsUsed = resetCounter
            };
            dbHelper.InsertItem(newGameState);
            gameSessionId = dbHelper.GetMaxId();

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
            TotalPossibleWinningsLabel.Text = $"Maximum win : N{1000 * (MaxResets - resetCounter)}";

            SelectionButtonFrame.IsVisible = true;
            UserChoiceLabel.Text = "Heads or Tails";
            CoinFlipLayout.IsVisible = false;
            CoinFrontImage.IsVisible = true;
            CoinBackImage.IsVisible = true;
            ResetButton.IsVisible = false;
            InformationLayout.IsVisible = false;
            SideSelectionLayout.IsVisible = true;

        }

        private void startGameUISetup(string username, string choice, int maxPossibleWinning, int potentialWinning)
        {
            UserChoiceLabel.Text = $"{username} : {choice}";
            TotalPossibleWinningsLabel.Text = $"Maximum Win: N{maxPossibleWinning}";
            PotentialWinningLabel.Text = $"Potential Winnings: N{potentialWinning}";
            NameEntryFrame.IsVisible = false;
            PhoneNumberEntryFrame.IsVisible = false;
            SelectionButtonFrame.IsVisible = false;
            SideSelectionLayout.IsVisible = false;
            CoinFlipLayout.IsVisible = true;
            ResetButton.IsVisible = true;
            InformationLayout.IsVisible = true;
            Titlelayout.Orientation = StackOrientation.Horizontal;
            UserChoiceImage.IsVisible = false;
        }

        private bool isMaxWinAchieved()
        {
            if (currentWinnings == maxPossibleWinnings)
            {
                ResetButton.IsVisible = false;
                CashOutButtonFrame.IsVisible = true;
                CoinImage.IsEnabled = false;
                CoinImage.Source = "coin_locked.png";
                return true;
            }
            return false;
        }

        private async void OnCashOutClicked(object sender, EventArgs e)
        {
            await CashOutButtonFrame.ScaleTo(1.3, 100);
            await CashOutButtonFrame.ScaleTo(1, 100);
            await DisplayAlert("Thank you for playing!", $"Are you sure you want to cashout your winnings of {currentWinnings}?", "Yes", "No");

            //SendEmail("recipient@example.com", "Cashout Request", "Please process my cashout request.");

        }

        //private async void SendEmail(string recipient, string subject, string body)
        //{
        //    try
        //    {
        //        var message = new EmailMessage
        //        {
        //            To = new List<string> { recipient },
        //            Subject = subject,
        //            Body = body
        //        };
        //        await Email.ComposeAsync(message);
        //    }
        //    catch (FeatureNotSupportedException)
        //    {
        //        // Email is not supported on this device
        //    }
        //    catch (Exception ex)
        //    {
        //        // Other error occurred
        //    }
        //}


    }
}
