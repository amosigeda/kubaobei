using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace YW.Utility
{
    public class Convert
    {
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = System.Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public static string HexByteToStr(byte[] bytes)
        {
            return HexByteToStr(bytes, 0, bytes.Length);
        }
        public static string HexByteToStr(byte[] bytes, int index, int length)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    str.Append(bytes[index + i].ToString("X2"));
                }
                else
                {
                    str.Append(" " + bytes[index + i].ToString("X2"));
                }
            }
            return str.ToString();
        }
        public static int BytesToInt32(byte[] bytes, int startOffset)
        {
            int length = bytes[startOffset] * 256 * 256 * 256 + bytes[startOffset + 1] * 256 * 256 + bytes[startOffset + 2] * 256 + bytes[startOffset + 3];
            return length;
        }
        public static byte[] GetByteArray(byte[] from, int start, int length)
        {
            byte[] n = new byte[length];
            Array.Copy(from, start, n, 0, length);
            return n;
        }
        public static int StringToInt32(string str)
        {
            try
            {
                return int.Parse(str);
            }
            catch
            {
                return 0;
            }
        }
        public static int HexToInt(string hex)
        {
            try
            {
                return Int32.Parse(hex, NumberStyles.HexNumber);
            }
            catch
            {
                return 0;
            }
        }
        public static string HexToFullBinary(string hex)
        {
            string b;
            try
            {

                b = System.Convert.ToString(Int32.Parse(hex, NumberStyles.HexNumber), 2);
                int len = 4 * hex.Length;
                for (int j = 0; j < len; j++)
                {
                    if (b.Length == len)
                        break;

                    b = "0" + b;
                }
            }
            catch
            {
                b = "";
            }
            return b;
        }
        public static string HexToBinary(string hex)
        {
            string b;
            try
            {
                b = System.Convert.ToString(Int32.Parse(hex, NumberStyles.HexNumber), 2);
            }
            catch
            {
                b = "";
            }
            return b;
        }

        //二进制转十六进制
        public static string BinaryToHex(string binary)
        {
            int intVal = System.Convert.ToInt32(binary, 2);
            string str = System.Convert.ToString(intVal, 16).PadLeft(2, '0');
            return str;
        }
    }
}
