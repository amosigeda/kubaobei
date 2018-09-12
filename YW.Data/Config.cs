using System.Reflection;
using YW.Utility;

namespace YW.Data
{
    public class Config
    {
        public DBHelper.DatabaseType DatabaseType;
        public string DB;
        public string DB_LBS;
        public string Path;
        private static Config _Object;
        private static object _LockHelper = new object();
        public static Config GetInstance()
        {
            if (_Object == null)
            {
                lock (_LockHelper)
                {
                    if (_Object == null)
                    {
                        _Object = new Config();
                    }
                }
            }
            return _Object;
        }
        public Config()
        {
            try
            {
                string stmp = Assembly.GetExecutingAssembly().Location;
                Path = stmp.Substring(0, stmp.LastIndexOf('\\'));//删除文件名
            }
            catch { }
          
            try
            {
                DB = AppConfig.GetValue("DB");
                DB_LBS = AppConfig.GetValue("DB_LBS");
            }
            catch { }
            
        }
    }

}
