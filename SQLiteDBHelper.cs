﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    class SQLiteDBHelper
    {
        private SQLiteConnection dbConnection, dbConnection2;
        public static string DbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeadsorTailsGameData.db3");


        public SQLiteDBHelper()
        {
            dbConnection = new SQLiteConnection(DbPath);

            var dbConnectionLoc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));


            dbConnection.CreateTable<GameData>(); // Create the 'GameData' table if it doesn't exist
            dbConnection.CreateTable<UserData>(); // Create the 'UserData' table if it doesn't exist
            var t = dbConnection.DatabasePath;

            bool dbExists = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeadsorTailsGameData.db3"));
            //bool dbExists2 = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "HeadsorTailsGameData.db3"));

        }

        #region UserData db functions
        public int UserDataInsertItem(UserData data)
        {
            return dbConnection.Insert(data);
        }
        public List<UserData> GetUserData()
        {
            return dbConnection.Table<UserData>().ToList();
        }
        #endregion

        #region GameData db functions
        public List<GameData> GetGameData()
        {
            return dbConnection.Table<GameData>().ToList();
        }

        public GameData GetFirstItem()
        {
            return dbConnection.Table<GameData>().FirstOrDefault();
        }

        public int InsertItem(object item)
        {
            return dbConnection.Insert(item);
        }

        public int UpdateItem(GameData item)
        {
            return dbConnection.Update(item);
        }

        public int DeleteItem(GameData item)
        {
            return dbConnection.Delete(item);
        }

        public int GetMaxId()
        {
            try
            {
                // Execute a SQL query to select the maximum ID from the GameData table
                string query = "SELECT MAX(ID) FROM GameData;";
                var result = dbConnection.ExecuteScalar<int>(query);

                // Return the maximum ID
                return result;
            }
            catch (Exception ex)
            {
                // Handle any exceptions or errors
                Console.WriteLine("Error: " + ex.Message);
                return -1; // Return a default value or handle the error as needed
            }
        }

        public GameData GetRowWithMaxId()
        {
            try
            {
                // Execute a SQL query to select the row with the maximum ID from the GameData table
                string query = "SELECT * FROM GameData WHERE ID = (SELECT MAX(ID) FROM GameData);";
                var result = dbConnection.Query<GameData>(query).FirstOrDefault();

                // Return the row with the maximum ID
                return result;
            }
            catch (Exception ex)
            {
                // Handle any exceptions or errors
                Console.WriteLine("Error: " + ex.Message);
                return null; // Return null or handle the error as needed
            }
        }

        public bool GetHasSelectedSidesOfMaxID()
        {
            try
            {

                string query = "SELECT SelectedSides FROM GameData WHERE ID = (SELECT MAX(ID) FROM GameData)";
                var result = dbConnection.ExecuteScalar<int>(query);
                return (result == 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
