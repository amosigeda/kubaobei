using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace YW.Utility
{
    public class Utils
    {
        public static bool IsNum(String str)
        {
            for(int i=0;i<str.Length;i++)
            {
                if(!Char.IsNumber(str,i))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Find the position in the specified byte array by sub byte array.
        /// </summary>
        /// <param name="bts">Specified byte array</param>
        /// <param name="sub">Sub byte array</param>
        /// <param name="offset">offset</param>
        /// <returns>-1 Not found else return the found position</returns>
        public static int indexOfSubBytes(byte[] bts, byte[] sub, int offset) {
            if (bts.Length < sub.Length) {
                return -1;
            }
            for (int i = offset, j = bts.Length - sub.Length; i < j; i++) {
                if (bts[i] == sub[0]) {
                    bool isSame = true;
                    for (int k = 1; k < sub.Length; k++) {
                        if (bts[i + k] != sub[k]) {
                            isSame = false;
                            i += k;
                            break;
                        }
                    }
                    if (isSame) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static string createNewSalt()
        {
            string guid = Guid.NewGuid().ToString("N");
            return guid.Substring(guid.Length - 8).ToUpper();
        }

        public static string ExecuteCommand(string command, string param)
        {
            // ProcessStartInfo info = new ProcessStartInfo(strExePath,传给EXE 的参数);
            ProcessStartInfo info = new ProcessStartInfo(command, param);
            info.UseShellExecute = false;
            //隐藏exe窗口状态
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardOutput = true;
            //运行exe
            Process proc = Process.Start(info);
            proc.WaitForExit();
            string res;
            using (StreamReader sr = proc.StandardOutput)
            {
                res = sr.ReadLine();
                while (null != res)
                {
                    Console.WriteLine(res);
                    res = sr.ReadLine();
                }
            }

            if (proc.HasExited)
            {
                proc.Close();
            }

            // 取得EXE运行后的返回值，返回值只能是整型
            return res;
        }

        public static string CaptureFromVideo(string source, string target, int frame)
        {
            int idx = target.LastIndexOfAny(new[] {'/', '\\'});
            if (idx == -1)
            {
                return null;
            }

            DirectoryInfo di = new DirectoryInfo(target.Substring(0, idx + 1));
            if (!di.Exists)
            {
                di.Create();
            }

            string param = "-i " + source + " -r 1 -frames:v " + frame + " -f image2 " + target;
            try
            {
                return ExecuteCommand("ffmpeg", param);
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                //
            }

            return null;
        }

        public static string GetJObjVal(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            string res = "";
            if (obj.GetType() == typeof(JValue))
            {
                res = ((JValue) obj).Value<string>();
            }
            else if (obj.GetType() == typeof(JToken))
            {
                res = ((JToken) obj).Value<string>();
            }
            else if (obj.GetType() == typeof(JObject))
            {
                res = ((JObject) obj).ToString();
            }
            else if (obj.GetType() == typeof(JArray))
            {
                JArray ary = (JArray) obj;
                if (ary.Count > 0 && ary[0] != null)
                {
                    res = ary[0].ToString();
                }
            }
            else
            {
                res = obj.ToString();
            }

            return Regex.Replace(res, "[\\{\\}\\[\\] ,\"\']+", "");
        }
    }
}