// QuickStart/Utilities/CryptionEngine.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuickStart.Utilities
{
    public static class CryptionEngine
    {
        private static readonly string privateKey = "rjV6hpZ9";
        private static readonly string keyAddress = "8Ybz6L9V";
        private static readonly string keyValue = "1vJz6Kr6";

        public static string EncryptAddress(this string input) => Encrypt(input, keyAddress);
        public static string DecryptAddress(this string input) => Decrypt(input, keyAddress);
        public static string EncryptValue(this string input) => Encrypt(input, keyValue);
        public static string DecryptValue(this string input) => Decrypt(input, keyValue);

        public static string Encrypt(this string input, string key)
        {
            var privateKeyByte = Encoding.UTF8.GetBytes(privateKey);
            var keyByte = Encoding.UTF8.GetBytes(key);
            byte[] inputTextByteArray = Encoding.UTF8.GetBytes(input);
            using (var dsp = DES.Create())
            {
                var memStr = new MemoryStream();
                var cryStr = new CryptoStream(memStr, dsp.CreateEncryptor(keyByte, privateKeyByte), CryptoStreamMode.Write);
                cryStr.Write(inputTextByteArray, 0, inputTextByteArray.Length);
                cryStr.FlushFinalBlock();
                return Convert.ToBase64String(memStr.ToArray());
            }
        }

        public static string Decrypt(this string input, string key)
        {
            var privateKeyByte = Encoding.UTF8.GetBytes(privateKey);
            var keyByte = Encoding.UTF8.GetBytes(key);
            var inputTextByteArray = Convert.FromBase64String(input.Replace(" ", "+"));
            using (var des = DES.Create())
            {
                var memStr = new MemoryStream();
                var cryStr = new CryptoStream(memStr, des.CreateDecryptor(keyByte, privateKeyByte), CryptoStreamMode.Write);
                cryStr.Write(inputTextByteArray, 0, inputTextByteArray.Length);
                cryStr.FlushFinalBlock();
                return Encoding.UTF8.GetString(memStr.ToArray());
            }
        }
    }
}