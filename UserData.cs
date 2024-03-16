using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsOrTails
{
    internal class UserData
    {
        // Constructor with no parameters
        public UserData()
        {
            Name = "";
            UID = "";
            EmailAddress = "";
        }

        //start game with register button on screen and maybe spinning coin
        //registration stores userdata in sqlite as well as firebase
        //after registering, we go to the select sides screen
        //now we always check if there is a user in userdata table. If not create one
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [NotNull]
        public string Name { get; set; }

        public string? PhoneNumber { get; set; }

        [NotNull]
        public string UID { get; set; }

        [NotNull]
        public string EmailAddress { get; set; }

        public string? DeviceInfo { get; set; }
    }
}
