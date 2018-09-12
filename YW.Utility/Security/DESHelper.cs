using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace YW.Utility.Security
{
    public sealed class DESHelper
    {

        /// <summary>
        /// DES加密偏移量，必须是>=8位长的字符串
        /// </summary>
        byte[] _iv;
        /// <summary>
        /// DES加密的私钥，必须是8位长的字符串
        /// </summary>
        byte[] _key;

        public DESHelper()
        {
            _iv = Encoding.Default.GetBytes("");
            _key = Encoding.Default.GetBytes("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iv">DES加密向量，必须是>=8位长的字符串</param>
        /// <param name="key">DES加密的私钥，必须是8位长的字符串</param>
        public DESHelper(string iv, string key)
        {
            _iv = Encoding.Default.GetBytes(iv);
            _key = Encoding.Default.GetBytes(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ivBytes">DES加密向量,长度为8的字节数组</param>
        /// <param name="key">DES加密的私钥，必须是8位长的字符串</param>
        public DESHelper(byte[] ivBytes, string key)
        {
            _iv = ivBytes;
            _key = Encoding.Default.GetBytes(key);
        }

        /// <summary>
        /// 对字符串进行DES加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的BASE64编码的字符串</returns>
        public string Encrypt(string sourceString)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Encoding.Default.GetBytes(sourceString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(_key, _iv), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return System.Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 对DES加密后的字符串进行解密
        /// </summary>
        /// <param name="encryptedString">待解密的字符串</param>
        /// <returns>解密后的字符串</returns>
        public string Decrypt(string encryptedString)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = System.Convert.FromBase64String(encryptedString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(_key, _iv), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.Default.GetString(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 对文件内容进行DES加密
        /// </summary>
        /// <param name="sourceFile">待加密的文件绝对路径</param>
        /// <param name="destFile">加密后的文件保存的绝对路径</param>
        public void EncryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateEncryptor(_key, _iv), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// 对文件内容进行DES加密，加密后覆盖掉原来的文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件的绝对路径</param>
        public void EncryptFile(string sourceFile)
        {
            EncryptFile(sourceFile, sourceFile);
        }
        /// <summary>
        /// 对文件内容进行DES解密
        /// </summary>
        /// <param name="sourceFile">待解密的文件绝对路径</param>
        /// <param name="destFile">解密后的文件保存的绝对路径</param>
        public void DecryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateDecryptor(_key, _iv), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// 对文件内容进行DES解密，加密后覆盖掉原来的文件
        /// </summary>
        /// <param name="sourceFile">待解密的文件的绝对路径</param>
        public void DecryptFile(string sourceFile)
        {
            DecryptFile(sourceFile, sourceFile);
        }
    }
}
