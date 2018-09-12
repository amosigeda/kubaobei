using System.Security.Cryptography;
using System.Text;

namespace YW.Utility.Security
{
    /// <summary>
    /// 安全帮助类
    /// </summary>
    public static class MD5Helper
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string MD5Encrypt(string value)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Default.GetBytes(value);//将字符编码为一个字节序列
            byte[] md5data = md5.ComputeHash(data);//计算data字节数组的哈希值 
            md5.Clear();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5data.Length; i++)
            {
                sb.Append(md5data[i].ToString("X").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// MD5 加密，带盐
        /// </summary>
        /// <param name="value">要加密的值</param>
        /// <param name="salt">盐值</param>
        /// <returns></returns>
        public static string MD5Encrypt(string value, string salt)
        {
            if (salt != null) {
                return MD5Encrypt(value + "@%" + salt+"*$");
            }
            return MD5Encrypt(value);
        }

    }
}
