using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SecureGameCockNetwork.Providers
{
    public class EncryptDecryptProvider
    {
        // Get the shared key (Passphrase)  from config file
        private static string Sharedkey = ConfigurationManager.AppSettings["SharedKey"];

        private const int Keysize = 256; // can be 192 or 128

        private const int BlockSize = 256; // AES uses only 128 bit block size (FIXED BLOCK SIZE) where as Rinjdael can use 256.

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Encrypt(string plainText)
        {
            byte[] byteText = Encoding.UTF8.GetBytes(plainText);

            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var initvectorStringBytes = Generate256BitsOfRandomEntropy();
            using (var KeyBytes = new Rfc2898DeriveBytes(Sharedkey, saltStringBytes, DerivationIterations))
            {
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = BlockSize;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    // Use the password to generate pseudo - random bytes for the encryption
                    // key. Specify the size of the key in bytes (instead of bits).
                    byte[] keyBytes = KeyBytes.GetBytes(Keysize / 8);

                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initvectorStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(byteText, 0, byteText.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(initvectorStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                
                                //Assemble encrypted message and add authentication
                                using (var hmac = new HMACSHA256(keyBytes))
                                {
                                    //Authenticate all data
                                    var tag = hmac.ComputeHash(cipherTextBytes);
                                    cipherTextBytes = cipherTextBytes.Concat(tag).ToArray();
                                }
                                memoryStream.Close();
                                cryptoStream.Close();

                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            byte[] cipherTextBytesWithSaltAndIVAndTag = Convert.FromBase64String(cipherText);

            //byte[] cipherTextBytesWithSaltAndIv =Encoding.UTF8.GetBytes(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIVAndTag.Take(Keysize / 8).ToArray();

            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIVAndTag.Skip(Keysize / 8).Take(Keysize / 8).ToArray();

            //Get actual cipher text after substracting tag length and initial (salt and IV) lengths respectively.

            using (var password = new Rfc2898DeriveBytes(Sharedkey, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);


                //Integrity Verificatoin using MAC Hashing using SHA256
                using (var hmac = new HMACSHA256(keyBytes))
                {
                    var sentTag = new byte[hmac.HashSize / 8];

                    // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                    var cipherTextBytes = cipherTextBytesWithSaltAndIVAndTag.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIVAndTag.Length - ((Keysize / 8) * 2) - sentTag.Length).ToArray();

                    //Calculate Tag
                    var calcTag = hmac.ComputeHash(cipherTextBytesWithSaltAndIVAndTag, 0, cipherTextBytesWithSaltAndIVAndTag.Length - sentTag.Length);
                    var ivLength = (BlockSize / 8);

                    //if message length is to small just return null
                    if (cipherTextBytesWithSaltAndIVAndTag.Length < sentTag.Length + ivLength)
                        return null;

                    //Grab Sent Tag and later use this for comparision
                    Array.Copy(cipherTextBytesWithSaltAndIVAndTag, cipherTextBytesWithSaltAndIVAndTag.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                    //Integrity Verificatoin using MAC Hashing using SHA256, Comparision of HashCode bytes
                    //Compare Tag with constant time comparison
                    var compare = 0;
                    for (var i = 0; i < sentTag.Length; i++)
                        compare |= sentTag[i] ^ calcTag[i];

                    // if message doesn't authenticate return null
                    if (compare != 0)
                        return null;

                    //Symmetric Key Decryption
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = BlockSize;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the randomBytes array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}