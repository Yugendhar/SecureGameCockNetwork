using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SecureGameCockNetwork.Providers
{
    public class EncryptDecryptProvider
    {
        // Get the shared key (Passphrase)  from config file
         AppSettingsReader settingsReader = new AppSettingsReader();
       // string key = (string)settingsReader.GetValue("sharedkey", typeof(String));
         
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText)
        {
            byte[] byteText = System.Text.Encoding.Unicode.GetBytes(plainText);
            return Convert.ToBase64String(byteText);
        }
        public static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            return System.Text.Encoding.Unicode.GetString(cipherBytes);
        }
    }
}