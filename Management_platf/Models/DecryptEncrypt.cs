using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WeiXinApi.Models
{
    public class DecryptEncrypt
    {
        private static readonly byte[] IV = new byte[] { 0x7f, 0xbb, 0xa3, 0x9a, 0xc8, 0x87, 0xbd, 0xb0 };
        private static readonly byte[] Key = new byte[] { 0x82, 0x81, 0xa0, 0x4a, 0xa7, 0xd0, 0x49, 0xa0 };

        public void EncryptFile(string inFileName, string outFileName)
        {
            using (var inStream = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
            using (var outStream = new FileStream(outFileName, FileMode.Create, FileAccess.Write))
            using (var des = new DESCryptoServiceProvider { IV = IV, Key = Key })
            using (var encryptor = des.CreateEncryptor())
            {
                var buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, buffer.Length);
                var bytes = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        public void DecryptFile(string inFileName, string outFileName)
        {
            using (var inStream = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
            using (var outStream = new FileStream(outFileName, FileMode.Create, FileAccess.Write))
            using (var des = new DESCryptoServiceProvider { IV = IV, Key = Key })
            using (var decryptor = des.CreateDecryptor())
            {
                var buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, buffer.Length);
                var bytes = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        public string EncryptString(string inString)
        {
            var cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            var ms = new MemoryStream();
            var cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(Key, IV), CryptoStreamMode.Write);
            var sw = new StreamWriter(cst);
            sw.Write(inString);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        public string DecryptString(string inBase64String)
        {
            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(inBase64String);
            }
            catch
            {
                return null;
            }
            var cryptoProvider = new DESCryptoServiceProvider();
            var ms = new MemoryStream(byEnc);
            var cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read);
            var sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }

        public void StringEncryptToFile(string inString, string filePath)
        {
            using (var inStream = new MemoryStream(Encoding.UTF8.GetBytes(inString)))
            using (var outStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var des = new DESCryptoServiceProvider { IV = IV, Key = Key })
            using (var encryptor = des.CreateEncryptor())
            {
                var buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, buffer.Length);
                var bytes = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                outStream.Write(bytes, 0, bytes.Length);

            }
        }

        public string FileDecryptToString(string inFileName)
        {

            using (var inStream = new FileStream(inFileName, FileMode.Open, FileAccess.Read))
            using (var outStream = new MemoryStream())
            using (var des = new DESCryptoServiceProvider { IV = IV, Key = Key })
            using (var decryptor = des.CreateDecryptor())
            {
                var buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, buffer.Length);
                var bytes = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                return UTF8Encoding.UTF8.GetString(bytes);

            }
        }
    }
}