using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsOrTails
{
    class GameData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string PlayerName { get; set; }

        [NotNull]
        public string PlayerNumber { get; set; }

        [NotNull]
        public string UniqueID { get; set; }

        [NotNull]
        public int SelectedSides { get; set; }

        [NotNull]
        public int PlayingHeads { get; set; }

        [NotNull]
        public int HeadsCount { get; set; }

        [NotNull]
        public int TailsCount { get; set; }

        [NotNull]
        public int MaxPossibleWinnings { get; set; }

        [NotNull]
        public int CurrentWinnings { get; set; }

        [NotNull]
        public int TotalResetsUsed { get; set; }

        // Add other properties as needed
        //users device name, location to try to ensure its 1 person per win

        // Default constructor
        public GameData()
        {
            // Initialize properties to zero
            ID = 0;
            PlayerName = "";
            PlayerNumber = "";
            UniqueID = "";
            SelectedSides = 0;
            PlayingHeads = 0;
            HeadsCount = 0;
            TailsCount = 0;
            MaxPossibleWinnings = 0;
            CurrentWinnings = 0;
            TotalResetsUsed = 0;
        }
    }

    //public GameData(
    //        int id = 0,
    //        string playerName = "",
    //        int playingHeads = 0,
    //        int headsCount = 0,
    //        int tailsCount = 0,
    //        int maxPossibleWinnings = 0,
    //        int currentWinnings = 0,
    //        int totalResetsUsed = 0)
    //{
    //    // Initialize properties to zero
    //    ID = id;
    //    PlayerName = playerName;
    //    PlayingHeads = playingHeads;
    //    HeadsCount = headsCount;
    //    TailsCount = tailsCount;
    //    MaxPossibleWinnings = maxPossibleWinnings;
    //    CurrentWinnings = currentWinnings;
    //    TotalResetsUsed = totalResetsUsed;
    //}
}
