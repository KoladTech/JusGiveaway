using Firebase.Auth.Providers;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAuthException = Firebase.Auth.FirebaseAuthException;

namespace HeadsOrTails
{
    internal class FirebaseAuthHelper
    {

        private static FirebaseAuthHelper instance;
        private static FirebaseAuthClient client;

        public string webApiKey = "AIzaSyCFViNHQ81CR89Jcag01BOMVODjKYbXLxE";

        private FirebaseAuthHelper()
        {

            // Configure...
            var config = new FirebaseAuthConfig
            {
                ApiKey = webApiKey,
                AuthDomain = "headsortails-c1e0b.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    // Add and configure individual providers
                    new EmailProvider()
                },
            };

            // ...and create your FirebaseAuthClient
            client = new FirebaseAuthClient(config);

            //var userCredential = await client.CreateUserWithEmailAndPasswordAsync("email", "pwd", "Display Name");
        }

        public static FirebaseAuthHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FirebaseAuthHelper();
                }
                return instance;
            }
        }

        public async Task<UserCredential?> RegisterUserWithEmailAndPasswordAsync(string email, string password, string displayName)
        {
            try
            {
                // Create user with email and password
                var auth = await client.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
                return auth;
            }
            catch (FirebaseAuthException ex)
            {
                // Handle any authentication errors
                Console.WriteLine($"Error registering user: {ex.Message}");
                return null;
            }
        }

        public async Task<UserCredential?> SignInUserWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                // Sign in user with email and password
                var userCredential = await client.SignInWithEmailAndPasswordAsync(email, password);

                //return userCredential.AuthCredential.ToString(); // Return the user's unique ID (UID)
                return userCredential;
            }
            catch (FirebaseAuthException ex)
            {
                // Handle any authentication errors
                Console.WriteLine($"Error registering user: {ex.Message}");
                return null;
            }
        }

        public User? GetCurrentUserAsync()
        {
            try
            {
                var cl = client.User;
                return cl;
            }
            catch (FirebaseAuthException ex)
            {
                // Handle any authentication errors
                Console.WriteLine($"Error retrieving user: {ex.Message}");
                return null;
            }
        }
    }
}
