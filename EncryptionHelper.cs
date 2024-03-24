using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    public class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("YourSecretKey123");

        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.GenerateIV(); // Generate a random IV

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes;
                using (MemoryStream msEncrypt = new())
                {
                    using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }
                // Concatenate IV with the encrypted data
                byte[] ivAndEncryptedData = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                Array.Copy(aesAlg.IV, 0, ivAndEncryptedData, 0, aesAlg.IV.Length);
                Array.Copy(encryptedBytes, 0, ivAndEncryptedData, aesAlg.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(ivAndEncryptedData);
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] ivAndEncryptedData = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                // Extract IV from the concatenated data
                byte[] iv = new byte[16];
                Array.Copy(ivAndEncryptedData, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes = new byte[ivAndEncryptedData.Length - aesAlg.IV.Length];
                Array.Copy(ivAndEncryptedData, aesAlg.IV.Length, encryptedBytes, 0, encryptedBytes.Length);

                using (MemoryStream msDecrypt = new(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}
