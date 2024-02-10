namespace HeadsOrTails
{
    public partial class MainPage : ContentPage
    {
        private int headsCount = 0;
        private int tailsCount = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void FlipCoinButton_Clicked(object sender, EventArgs e)
        {
            Random random = new Random();
            int result = random.Next(2); // Generates either 0 or 1
            string flipResult = result == 0 ? "Heads" : "Tails";

            // Update UI
            if (flipResult == "Heads")
            {
                headsCount++;
            }
            else
            {
                tailsCount++;
            }
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            HeadsLabel.Text = $"Heads: {headsCount}";
            TailsLabel.Text = $"Tails: {tailsCount}";
        }
    }
}
