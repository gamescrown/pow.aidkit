using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace pow.aidkit
{
    public static class TextEncryption
    {
        private static TripleDESCryptoServiceProvider GetCryptoProvider(string keyStr)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes(keyStr));
            return new TripleDESCryptoServiceProvider {Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};
        }

        public static string Encrypt(string plainString, string keyStr)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainString);
            TripleDESCryptoServiceProvider tripleDes = GetCryptoProvider(keyStr);
            ICryptoTransform transform = tripleDes.CreateEncryptor();
            byte[] resultsByteArray = transform.TransformFinalBlock(data, 0, data.Length);
            return RemoveNonAlphanumericChars(Convert.ToBase64String(resultsByteArray));
        }

        public static string Decrypt(string encryptedString, string keyStr)
        {
            byte[] data = Convert.FromBase64String(encryptedString);
            TripleDESCryptoServiceProvider tripleDes = GetCryptoProvider(keyStr);
            ICryptoTransform transform = tripleDes.CreateDecryptor();
            byte[] resultsByteArray = transform.TransformFinalBlock(data, 0, data.Length);
            return Encoding.UTF8.GetString(resultsByteArray);
        }

        private static string RemoveNonAlphanumericChars(string str)
        {
            char[] arr = str.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray();
            return new string(arr);
        }
    }
}