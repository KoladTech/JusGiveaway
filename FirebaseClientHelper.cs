using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadsOrTails
{
    internal class FirebaseClientHelper
    {
        private static FirebaseClientHelper instance;
        private FirebaseClient firebaseClient;

        private FirebaseClientHelper()
        {
            firebaseClient = new FirebaseClient("https://headsortails-c1e0b-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        public static FirebaseClientHelper Instance
        {
            get
            {
                instance ??= new FirebaseClientHelper();
                return instance;
            }
        }

        public FirebaseClient GetFirebaseClient()
        {
            return firebaseClient;
        }
    }
}
