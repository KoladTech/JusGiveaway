using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    internal class GameDataRepository
    {
        private readonly SQLiteConnection _database;

        public static string DbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeadsorTailsGameData.db3");

        public GameDataRepository()
        {
            _database = new SQLiteConnection(DbPath);
            _database.CreateTable<GameData>();
        }

        public List<GameData> List()
        {
            return _database.Table<GameData>().ToList();
        }
    }
}
