using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using static JusGiveaway.CustomAlertPage;

namespace JusGiveaway;

public partial class HeadsOrTails : ContentPage
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
    private int maxPossibleWinnings = 0; //change this to be set to the lesser of db value or left over winnings
    private double totalGiveawayFunds = 0;

    // Define a counter to keep track of the number of resets
    private int resetCounter = 0;
    private List<ImageButton> _heartImages = new List<ImageButton>(); // List to hold heart image references
    private int gameSessionId = 0;
    private readonly SQLiteDBHelper dbHelper;
    private readonly UserData userData;
    private GameData activeGame;
    private readonly FirebaseClient firebaseClient;// = FirebaseClientHelper.Instance.GetFirebaseClient();
    private Dictionary<string, string> firebaseGiveawayData;

    public HeadsOrTails(Dictionary<string,string> firebaseGiveawayData)
	{
		InitializeComponent();

        dbHelper = new();
        userData = dbHelper.GetUserData()[0];
        activeGame = dbHelper.GetRowWithMaxId();
        firebaseClient = FirebaseClientHelper.Instance.GetFirebaseClient();
        this.firebaseGiveawayData = firebaseGiveawayData;

        maxPossibleWinnings = CommonFunctions.GetLeftoverGiveawayFunds(firebaseGiveawayData) < activeGame.MaxPossibleWinnings ? CommonFunctions.GetLeftoverGiveawayFunds(firebaseGiveawayData) : activeGame.MaxPossibleWinnings;

        gameSessionId = dbHelper.GetMaxId();

        List<GameData> items = dbHelper.GetGameData();
        foreach (GameData item in items)
        {
            //Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Price: {item.Price}");
            isHeadsChosen = true;
        }

        //GameData lastGame = new GameData();
        //lastGame = dbHelper.GetRowWithMaxId();

        if (activeGame != null && activeGame.SelectedSides == 1)
        {
            uID = activeGame.UID;
            hasSelectedSides = activeGame.SelectedSides == 1;
            isHeadsChosen = activeGame.PlayingHeads == 1;
            headsCount = activeGame.HeadsCount;
            tailsCount = activeGame.TailsCount;
            totalFlips = headsCount + tailsCount;
            //maxPossibleWinnings = CommonFunctions.GetLeftoverGiveawayFunds(firebaseGiveawayData) < activeGame.MaxPossibleWinnings ? CommonFunctions.GetLeftoverGiveawayFunds(firebaseGiveawayData) : activeGame.MaxPossibleWinnings;
            currentWinnings = activeGame.CurrentWinnings;
            resetCounter = activeGame.TotalResetsUsed;

            // Set UI with DB values
            UpdateCounts();

            //since we already have a game saved, setup the game from where we left off
            startGameUISetup(userData.Name, hasSelectedSides, (isHeadsChosen ? "Heads" : "Tails"), maxPossibleWinnings, currentWinnings);

            SaveDataToProperties();
        }
        else if (activeGame != null)
        {
            hasSelectedSides = false;
            //maxPossibleWinnings = int.Parse(firebaseGiveawayData["LeftoverGiveawayFunds"]) < activeGame.MaxPossibleWinnings ? int.Parse(firebaseGiveawayData["LeftoverGiveawayFunds"]) : activeGame.MaxPossibleWinnings;
            resetCounter = activeGame.TotalResetsUsed;

            startGameUISetup(userData.Name, hasSelectedSides, null, maxPossibleWinnings, 0);

            //SaveDataToProperties();
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
        AddHeartImages();
    }

    //OBSOLETE
    //public Task<double> GetRemainingGiveawayFundsAsync(Dictionary<string, string> giveawayData)
    //{
    //    if (giveawayData == null)
    //    {
    //        return Task.FromResult<double>(0);
    //    }
        
    //    return Task.FromResult(double.Parse(giveawayData["LeftoverGiveawayFunds"]));
    //}

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
        bool isConfirmed = await CommonFunctions.DisplayCustomAlertPage(
            "Confirm Selection?", 
            $"{userData.Name} : {choice}", 
            "Yes", "No", 
            true, true, 
            AlertType.Info, Navigation);

        if (isConfirmed)
        {
            //uID = EncryptionHelper.Encrypt(playerPhoneNumber);

            hasSelectedSides = true;
            startGameUISetup(userData.Name, hasSelectedSides, choice, maxPossibleWinnings, 0);

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
        }
    }

    private async void OnCoinTapped(object sender, EventArgs e)
    {
        CashOutButton.IsVisible = false;
        ResetButton.IsVisible = true;
        //CoinImage.IsEnabled = false;

        Random random = new Random();

        // Rotate the coin animation
        for (int i = 0; i < 15; i++)
        {
            // Alternate between front and back images during rotation
            CoinImage.Source = i % 2 == 0 ? CommonFunctions.coinFrontImage : CommonFunctions.coinBackImage;

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
            headsCount += 20;
            CoinImage.Source = CommonFunctions.coinFrontImage;
        }
        else
        {
            //tailsCount++;
            tailsCount += 20;
            CoinImage.Source = CommonFunctions.coinBackImage;
        }
        //score = (headsCount - tailsCount) * scoreAdjuster;

        totalFlips = headsCount + tailsCount;

        // Update UI
        UpdateCounts();
        CoinImage.IsEnabled = true;

        CheckForWin();

        SaveDataToProperties();
    }

    private void UpdateCounts()
    {
        scoreAdjuster = isHeadsChosen ? 1 : -1;
        score = (headsCount - tailsCount) * scoreAdjuster;
        ScoreLabel.Text = $"{score}";
        ScoreLabel.TextColor = (score >= 0) ? Color.Parse("Green") : Color.Parse("Red");

        TotalLabel.Text = $"Total Flips: {totalFlips}";

        // Calculate percentages
        double headsPercentage = (double)headsCount / totalFlips;
        double tailsPercentage = (double)tailsCount / totalFlips;

        // Set the heights of the bars relative to the fixed container height (maxBarHeight)
        HeadsBar.HeightRequest = headsPercentage * HeadsBarFrame.HeightRequest;
        TailsBar.HeightRequest = tailsPercentage * TailsBarFrame.HeightRequest;

        HeadsCountLabel.Text = $"{headsCount}";
        TailsCountLabel.Text = $"{tailsCount}";
    }

    private async void CheckForWin()
    {
        bool playerWon = false;
        bool canCashOut = currentWinnings >= activeGame.MinCashOut;
        string roundCompleteMsg = $"Round {(totalFlips / 100)} complete!";
        string winMsg = string.Empty;
        string lossMsg = "You did not win this time.";
        string additionalMsg = string.Empty;

        // Check for win only when total flips reach a multiple of 100
        if (totalFlips >= 100 && totalFlips % 100 == 0)
        {
            if (score >= 0)
            {
                int winning = 0;
                playerWon = true;

                if (score == 0)     //A draw (BINGO)
                {
                    winning = CommonFunctions.GetRoundDrawMonetaryValue(firebaseGiveawayData);
                }
                else if(score >= 10)    //A big win
                {
                    winning = CommonFunctions.GetRoundBigWinMonetaryValue(firebaseGiveawayData);
                }
                else    //A small win
                {
                    winning = CommonFunctions.GetRoundSmallWinMonetaryValue(firebaseGiveawayData);
                }

                currentWinnings += winning;
                //check if current round winning puts you above max possible win
                if (currentWinnings > maxPossibleWinnings)
                {
                    currentWinnings -= winning;
                    winning = maxPossibleWinnings - currentWinnings;
                    currentWinnings = maxPossibleWinnings;
                }

                winMsg = $"You won N{winning.ToString("N0")}! ";

                CommonFunctions.AnimateLabelChange(PotentialWinningLabel, currentWinnings - winning, currentWinnings);
            }
            else
            {
                int loss = 0;
                playerWon = false;

                if (currentWinnings > 0)
                {
                    if (score <= -10)    //A big loss
                    {
                        loss = CommonFunctions.GetRoundBigLossMonetaryValue(firebaseGiveawayData);
                    }
                    else    //A small loss
                    {
                        loss = CommonFunctions.GetRoundSmallLossMonetaryValue(firebaseGiveawayData);
                    }

                    lossMsg = $"You lost N{loss.ToString("N0")}!";
                    currentWinnings -= loss;

                    CommonFunctions.AnimateLabelChange(PotentialWinningLabel, currentWinnings + loss, currentWinnings);
                }
            }

            canCashOut = currentWinnings >= activeGame.MinCashOut;
            winMsg += canCashOut ? "\nYou can cashout or Play for more" : "";
            CashOutButton.IsVisible = canCashOut;
            ResetButton.IsVisible = !canCashOut;

            if (playerWon)
            {
                if (isMaxWinAchieved())
                {
                    winMsg = $"You can now cashout your winnings of N{currentWinnings.ToString("N0")}";
                }
                additionalMsg = winMsg;
            }
            else
            {
                // Player did not meet winning criteria
                additionalMsg = lossMsg;
            }

            await CommonFunctions.DisplayCustomAlertPage(roundCompleteMsg, additionalMsg, "Ok", "", true, false, AlertType.Info, Navigation);

            try
            {
                //save progress to db
                GameData currentGameState = dbHelper.GetRowWithMaxId();
                currentGameState.HeadsCount = headsCount;
                currentGameState.TailsCount = tailsCount;
                currentGameState.CurrentWinnings = currentWinnings;
                dbHelper.UpdateItem(currentGameState);
            }
            catch (Exception)
            {
                await CommonFunctions.DisplayCustomAlertPage("Error", "Error saving game state to DB, please contact app devs", "Close", "", true, false, AlertType.Error, Navigation);
            }
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
            int monetaryResetPenalty = CommonFunctions.GetMonetaryResetPenalty(firebaseGiveawayData);
            int newMaxPossibleWinnings = maxPossibleWinnings - monetaryResetPenalty;
            newMaxPossibleWinnings = Math.Max(newMaxPossibleWinnings, CommonFunctions.GetMinCashoutPerPerson(firebaseGiveawayData));

            bool reset = await CommonFunctions.DisplayCustomAlertPage(
                "Reset Game?",
                $"Your maximum possible winnings will be reduced to N{newMaxPossibleWinnings.ToString("N0")}! " +
                $"\n\nYour current winnings of N{currentWinnings.ToString("N0")} will be reset to N0!", 
                "Yes", "No", 
                true, true, 
                AlertType.Info, Navigation);

            int totalResetsAllowed = CommonFunctions.GetTotalResetsAllowed(firebaseGiveawayData);

            if (reset)
            {
                if (resetCounter < totalResetsAllowed)
                {
                    // Calculate the current heart number (each heart has two halves)
                    int heartNumber = CommonFunctions.GetTotalResetsAllowed(firebaseGiveawayData) - activeGame.TotalResetsUsed;

                    // Find and update the left and right heart halves for the current heart
                    var leftHeart = _heartImages.FirstOrDefault(img => img.ClassId == $"Heart{heartNumber}_Left");
                    var rightHeart = _heartImages.FirstOrDefault(img => img.ClassId == $"Heart{heartNumber}_Right");


                    // Change their sources to grey heart images to indicate used reset
                    if (leftHeart != null)
                    {
                        AnimateHeartTransition(leftHeart, "grey_heart_left.png");
                    }
                    if (rightHeart != null)
                    {
                        AnimateHeartTransition(rightHeart, "grey_heart_right.png");
                    }

                    maxPossibleWinnings = newMaxPossibleWinnings;

                    // Increment the reset counter
                    resetCounter++;

                    // Reset game state and UI
                    ResetGame();

                    SaveDataToProperties();
                }
            }
            // disable reset button if we used up all resets
            ResetButton.IsEnabled = !(resetCounter == totalResetsAllowed);
            hasSelectedSides = false;
        }
        finally
        {
            //button.IsEnabled = true;
        }
    }

    // Function to reset the game state and UI
    private void ResetGame()
    {
        ResetUI();

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
    }
    private void ResetUI()
    {
        ScoreLabel.Text = "0";
        TotalLabel.Text = "Total Flips: 0";

        HeadsCountLabel.Text = "0";
        TailsCountLabel.Text = "0";
        HeadsBar.HeightRequest = 0;
        TailsBar.HeightRequest = 0;

        CommonFunctions.AnimateLabelChange(PotentialWinningLabel, currentWinnings, 0);
        CommonFunctions.AnimateLabelChange(TotalPossibleWinningsLabel, maxPossibleWinnings + CommonFunctions.GetMonetaryResetPenalty(firebaseGiveawayData), maxPossibleWinnings);

        //Header.IsVisible = false;
        //Titlelayout.IsVisible = false;
        SelectionButton.IsVisible = true;
        PlayerNameLabel.Text = userData.Name;
        PlayerChoiceLabel.Text = "Heads/Tails";
        CoinFlipLayout.IsVisible = false;
        CoinFrontImage.IsVisible = true;
        CoinBackImage.IsVisible = true;
        ResetButton.IsVisible = false;
        InformationLayout.IsVisible = false;
        SideSelectionLayout.IsVisible = true;
        CashOutButton.IsVisible = false;
    }

    private void startGameUISetup(string username, bool selectedSides, string? choice, int maxPossibleWinning, int potentialWinning)
    {
        //Header.IsVisible = true;
        PlayerNameLabel.Text = username;
        PlayerChoiceLabel.Text = choice ?? "Heads/Tails";
        TotalPossibleWinningsLabel.Text = $"N{maxPossibleWinning.ToString("N0")}";
        PotentialWinningLabel.Text = $"N{potentialWinning.ToString("N0")}";
        SelectionButton.IsVisible = !selectedSides;
        SideSelectionLayout.IsVisible = !selectedSides;
        CoinFlipLayout.IsVisible = selectedSides;
        ResetButton.IsVisible = selectedSides;
        InformationLayout.IsVisible = selectedSides;
        //Titlelayout.IsVisible = true;
        //Titlelayout.Orientation = StackOrientation.Horizontal;
        TotalGiveawayFundsLabel.Text = $"N{totalGiveawayFunds.ToString("N0")}";
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
        TimeSpan interval = TimeSpan.FromSeconds(10);
        while (true)
        {
            var prevTotalGiveawayFunds = totalGiveawayFunds;
            totalGiveawayFunds = await CommonFunctions.GetRemainingGiveawayFundsAsync(firebaseClient);
            var totalNumOfPlayers = int.Parse(firebaseGiveawayData["NumberOfPlayers"]);

            //Update the label with the retrieved value
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CommonFunctions.AnimateLabelChange(TotalGiveawayFundsLabel, (int)prevTotalGiveawayFunds, (int)totalGiveawayFunds);
                //TotalRegisteredPlayers.Text = $"# of Players - {totalNumOfPlayers}";
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

            bool cashOutNow = await CommonFunctions.DisplayCustomAlertPage(
                "Cashout!", 
                $"Are you sure you want to cashout your winnings of N{currentWinnings.ToString("N0")}?", 
                "Yes", "No", 
                true, true, 
                AlertType.Info, Navigation);

            if (cashOutNow)
            {
                await Navigation.PushAsync(new CashoutForm(userData, activeGame));

                //LockGame();
                //CashOutDetails cashOutDetails = new()
                //{
                //    Name = userData.Name,
                //    UID = userData.UID,
                //    EmailAddress = userData.EmailAddress,
                //    Sex = null,
                //    DeviceInfo = userData.DeviceInfo ?? "",
                //    BankAccountNumber = "0123456789",
                //    CashoutAmount = activeGame.CurrentWinnings
                //};

                //try
                //{
                //    //Post the dictionary to Firebase under the specified parent and child keys
                //    await firebaseClient.Child("Cashouts").Child(userData.UID).PatchAsync(cashOutDetails);

                //    customAlertPage = new CustomAlertPage("Cash Out Success", "We will be in touch soon", "Ok", "", true, false);
                //    await Navigation.PushModalAsync(customAlertPage);
                //    bool cashedout = await customAlertPage.WaitForUserResponseAsync();

                //    if (cashedout)
                //    {
                //        button.IsVisible = false;
                //        UpdateLeftoverGiveawayFunds();
                //    }
                //}
                //catch (FirebaseException ex)
                //{
                //    Console.WriteLine($"Error posting data to Firebase: {ex.Message}");
                //    // Handle the exception as needed. Maybe store in sqlite and try 
                //    //to send to firebase later
                //    customAlertPage = new CustomAlertPage("Error", "Cash out failed, please try agian", "Ok", "", true, false);
                //    await Navigation.PushModalAsync(customAlertPage);
                //    await customAlertPage.WaitForUserResponseAsync();
                //}
            }
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    private void LockGame()
    {
        ResetButton.IsVisible = false;
        CashOutButton.IsVisible = true;
        CoinImage.IsEnabled = false;
        CoinImage.Source = "coin_locked.png";

        //Write to db signifying end of game

    }

    private async void OnSponsoredByTapped(object sender, EventArgs e)
    {
        await CommonFunctions.AnimateSpan((Label)sender);

        var sponsorsPage = new SponsorsPage(firebaseGiveawayData["Sponsor"], firebaseGiveawayData["SponsorInstagramAccount"]);
        await Navigation.PushModalAsync(sponsorsPage);
    }

    // Method to add heart images to the StackLayout dynamically
    private void AddHeartImages()
    {
        var resetsLeft = CommonFunctions.GetTotalResetsAllowed(firebaseGiveawayData) - activeGame.TotalResetsUsed;
        int i = 0;

        // green hearts for number of resets remaining
        for (i=1; i <= resetsLeft; i++)
        {
            if (i % 2 == 1)
            {
                // Create the left half of the heart
                var leftHeart = new ImageButton
                {
                    Source = "green_heart_left.png",
                    WidthRequest = 10,
                    HeightRequest = 20,
                };
                leftHeart.ClassId = $"Heart{i}_Left";  // Assign unique identifier
                leftHeart.Clicked += OnHeartButtonClicked; // Assign the Clicked event handler
                _heartImages.Add(leftHeart);
                ResetCountVisualization.Children.Add(leftHeart);  // Add to StackLayout
            }
            else
            {
                // Create the right half of the heart
                var rightHeart = new ImageButton
                {
                    Source = "green_heart_right.png",
                    WidthRequest = 10,
                    HeightRequest = 20
                };
                rightHeart.ClassId = $"Heart{i}_Right";  // Assign unique identifier
                rightHeart.Clicked += OnHeartButtonClicked; // Assign the Clicked event handler
                _heartImages.Add(rightHeart);
                ResetCountVisualization.Children.Add(rightHeart);  // Add to StackLayout
            }
        }

        // grey hearts for the number of used resets
        for (int j = i; j < i + activeGame.TotalResetsUsed; j++)
        {
            if (j % 2 == 1)
            {
                // Create the left half of the heart
                var leftHeart = new ImageButton
                {
                    Source = "grey_heart_left.png",
                    WidthRequest = 10,
                    HeightRequest = 20
                };
                leftHeart.ClassId = $"Heart{j}_Left";  // Assign unique identifier
                leftHeart.Clicked += OnHeartButtonClicked; // Assign the Clicked event handler
                _heartImages.Add(leftHeart);
                ResetCountVisualization.Children.Add(leftHeart);  // Add to StackLayout
            }
            else
            {
                // Create the right half of the heart
                var rightHeart = new ImageButton
                {
                    Source = "grey_heart_right.png",
                    WidthRequest = 10,
                    HeightRequest = 20
                };
                rightHeart.ClassId = $"Heart{j}_Right";  // Assign unique identifier
                rightHeart.Clicked += OnHeartButtonClicked; // Assign the Clicked event handler
                _heartImages.Add(rightHeart);
                ResetCountVisualization.Children.Add(rightHeart);  // Add to StackLayout
            }
        }
    }
    private async void OnHeartButtonClicked(object sender, EventArgs e)
    {
        var resetsLeft = CommonFunctions.GetTotalResetsAllowed(firebaseGiveawayData) - activeGame.TotalResetsUsed;

        ResetCountHintLabel.Text = resetsLeft.ToString() + " resets left";

        // Animate the text color to green
        var fadeInAnimation = new Animation(v => ResetCountHintLabel.TextColor = Color.FromRgba(0, 0, 0, v), 0, 1);
        fadeInAnimation.Commit(this, "FadeInAnimation", 16, 500, Easing.Linear);

        // Wait for a short duration
        await Task.Delay(2000);

        // Animate the text color back to transparent
        var fadeOutAnimation = new Animation(v => ResetCountHintLabel.TextColor = Color.FromRgba(0, 0, 0, v), 1, 0);
        fadeOutAnimation.Commit(this, "FadeOutAnimation", 16, 2000, Easing.Linear);
    }

    async void AnimateHeartTransition(ImageButton heartImage, string newImageName)
    {
        // Start multiple animations concurrently
        await Task.WhenAll(
            heartImage.TranslateTo(0, 50, 1000, Easing.SinInOut),  // Drop the heart downwards
            heartImage.RotateTo(30, 1000, Easing.SinInOut),       // Rotate the heart slightly
            heartImage.FadeTo(0, 2000, Easing.SinOut)             // Fade the heart out
        );

        // Change the source once the animation completes
        heartImage.Source = newImageName;

        // Reset position and rotation for future animations
        await heartImage.TranslateTo(0, 0, 0);
        await heartImage.RotateTo(0, 0);

        // Fade back in with the new heart image
        await heartImage.FadeTo(1, 1000, Easing.SinInOut);
    }
}