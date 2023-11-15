using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CodeLab.Bravo.Utilities
{
    public class Crypto
    {
        public static string initVector = "tu89geji340t89u2";
        private static string defaultPassPhrase = "#$%(*gHj18)%$#@R";

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string textToEncrypt, string passPhrase = null)
        {
            if (passPhrase == null)
                passPhrase = defaultPassPhrase;
            try
            {
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(textToEncrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(passPhrase);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                return "Wrong Input. " + ex.Message;
            }
        }

        public static string Decrypt(string textToDecrypt, string passPhrase = null)
        {
            if (passPhrase == null)
                passPhrase = defaultPassPhrase;
            try
            {
                byte[] inputArray = Convert.FromBase64String(textToDecrypt);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(passPhrase);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                return "Wrong Input. " + ex.Message;
            }
        }

        public static string DecryptAES(string textToDecrypt, string passPhrase = null)
        {
            if (passPhrase == null)
                passPhrase = defaultPassPhrase;
            // Check arguments.  
            if (textToDecrypt == null || textToDecrypt.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (passPhrase == null || passPhrase.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            byte[] key = Encoding.UTF8.GetBytes(passPhrase);

            // Declare the string used to hold  
            // the decrypted text.  
            string decryptedText = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = key;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(textToDecrypt)))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                decryptedText = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    decryptedText = "keyError";
                }
            }

            return decryptedText;

        }
    
        public static string EncryptAES(string plainText)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            byte[] key = Encoding.UTF8.GetBytes(defaultPassPhrase);

            string encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = key;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }

            return encrypted;
        }
    }
}