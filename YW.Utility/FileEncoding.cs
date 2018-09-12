using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YW.Utility
{
    /// <summary> 
    /// 获取文件的编码格式 
    /// </summary> 
    public class EncodingType
    {
        private static int CountGbk(byte[] bytes, int start, int length)
        {
            int len = start+length;
            int counter = 0;
            const byte head = 0x80;
            for (int i = start; i < len - 1; ++i)
            {
                byte firstChar = bytes[i];
                if ((firstChar & head) == 0) continue;
                byte secondChar = bytes[i];
                if (firstChar >= 161 && firstChar <= 247 && secondChar >= 161 && secondChar <= 254)
                {
                    counter += 2;
                    ++i;
                }
            }
            return counter;
        }

        private static int CountUtf8(byte[] bytes, int start, int length)
        {
            int len = start + length;
            int counter = 0;
            const byte head = 0x80;
            for (int i = start; i < len; ++i)
            {
                byte firstChar = bytes[i];
                if ((firstChar & head)==0) continue;
                byte tmpHead = head;
                int wordLen = 0, tPos = 0;
                while ((firstChar & tmpHead)>0)
                {
                    ++ wordLen;
                    tmpHead >>= 1;
                }
                if (wordLen <= 1) continue; //utf8最小长度为2  
                wordLen --;
                if (wordLen + i >= len) break;
                for (tPos = 1; tPos <= wordLen; ++tPos)
                {
                    byte secondChar = bytes[i + tPos];
                    if ((secondChar & head)==0) break;
                }
                if (tPos > wordLen)
                {
                    counter += wordLen + 1;
                    i += wordLen;
                }
            }
            return counter;
        }

        public static bool BeUtf8(byte[] bytes,int start,int length)
        {
            int iGbk = CountGbk(bytes, start,length);
            int iUtf8 = CountUtf8(bytes, start, length);
            if (iUtf8 >= iGbk) return true;
            return false;
        }

    }


}
