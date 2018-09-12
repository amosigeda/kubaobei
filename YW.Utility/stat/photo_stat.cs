using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace YW.Utility.Stat
{

    public struct RECORD_INFO
    {
        public char stat_key;               //- 统计集合的key
        public string op;               //- 操作名
        public string ip;               //- ip
        public uint uin;                //- uin
        public int cost_t;              //- 耗时
        public int errcode;             //- 错误码
        public int size;                    //- 文件大小
        public string biz_name;          //-业务名

        public void set(char key, string _op, string _ip, uint _uin, int t, int err, int s = 0, string _bizname = "")
        {
            stat_key = key;
            op = _op;
            ip = _ip;

            uin = _uin;
            cost_t = t;
            errcode = err;
            size = s;
            if ("" == _bizname)
            {
                biz_name = CPhotoStat.UNKNOWN_BIZ;
            }
            else
            {
                biz_name = _bizname;
            }
        }
    };

    public class STAT_INFO
    {
        //	char op[40];				//-  操作名
        public int succ_num;                //-  成功次数
        public int err_num;             //-  总错误次数
        public int err_normal;          //-  正常错误数
        public int err_fail;                //-  失败错误数
        public long size;              //-  文件大小
        public Dictionary<int, int> map_err;        //-  错误码分布
        public long suc_total_t;        //-  总成功耗时
        public long err_total_t;        //-  总失败耗时
        public long all_total_t;      //-  所有耗时
        public Dictionary<int, int> map_time;      //-  延时分布 (>100 >300 >1000之类)

        public Dictionary<string, int> map_ip;      //-  ip分布 
        public Dictionary<uint, int> map_uin;   //-  uin分布


        public STAT_INFO(int init = 0)
        {
            map_err = new Dictionary<int, int>();
            map_time = new Dictionary<int, int>();
            map_ip = new Dictionary<string, int>();
            map_uin = new Dictionary<uint, int>();

            succ_num = err_num = err_normal = err_fail = 0;
            suc_total_t = err_total_t = all_total_t = size = 0;
        }
    };

    public class CPhotoStat
    {
        private static CPhotoStat _object;
        private static readonly object LockHelper = new object();
        protected string _log_file;
        protected int _log_time;
        protected System.DateTime _cur_time;
        protected int _print_err_num;
        private const string DateFormat = "yyyyMMdd";
        private const string TimeFormat = "yyyy/MM/dd HH:mm:ss";

        protected Dictionary<int, int> _map_normal_err;             //- 正常错误码集合
        protected List<int> _vec_normal_err;              //- 正常错误码集合
        protected List<int> _vec_cost_time;                   //- 统计的延时范围

        protected Dictionary<string, STAT_INFO> _map_stat_info;

        protected Dictionary<string, Dictionary<string, STAT_INFO>> _map_biz_stat;

        public static string UNKNOWN_BIZ = "unknow_biz";

        public CPhotoStat(int stat_time, string log_file, string normal_err, string cost_time, int print_err_num = 6)
        {
            _cur_time = System.DateTime.Now;
            _log_time = stat_time;
            _log_file = log_file;
            _print_err_num = print_err_num;

            _vec_normal_err = new List<int>();
            _vec_cost_time = new List<int>();
            str_2_int(normal_err, ref _vec_normal_err);
            str_2_int(cost_time, ref _vec_cost_time);
            _map_stat_info = new Dictionary<string, STAT_INFO>();
            _map_biz_stat = new Dictionary<string, Dictionary<string, STAT_INFO>>();
            _map_normal_err = new Dictionary<int, int>();

            foreach (var item in _vec_normal_err)
            {
                _map_normal_err.Add(item, 1);
            }
        }

        public static CPhotoStat GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new CPhotoStat(58, System.AppDomain.CurrentDomain.BaseDirectory + "stat\\stat_0", "0;","100;300;500;", 5);
                    }
                }
            }
            return _object;
        }


        private void str_2_int(string str, ref List<int> vec_int)
        {
            if (str.Length > 0 && str[0] != '\0')
            {
                int pos = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == ';')
                    {
                        if ((i - pos) > 0)
                        {
                            string tmp = str.Substring(pos, i - pos);
                            vec_int.Add(int.Parse(tmp));
                        }
                        pos = i + 1;
                    }
                }
            }
        }


        public void add_info(RECORD_INFO p)
        {
            lock (_map_stat_info)
            {
                STAT_INFO info;
                if (_map_stat_info.TryGetValue(p.op, out info))
                {
                    add_stat_2_info(p, ref info);
                }
                else
                {
                    info = new STAT_INFO();
                    add_stat_2_info(p, ref info);
                    _map_stat_info.Add(p.op, info);
                }
            }
            /* has problem
            if(p.errcode != -30000)   //业务不存在的错误码，如果业务不存在，不添加业务统计和创建相关的文件
            {
                Dictionary<string, STAT_INFO> busi_stat;
                if (_map_biz_stat.TryGetValue(p.biz_name, out busi_stat))
                { }
                else
                {
                    busi_stat = new Dictionary<string, STAT_INFO>();
                    _map_biz_stat.Add(p.biz_name, busi_stat);
                }


                STAT_INFO busi_stat_info;
                if (busi_stat.TryGetValue(p.op, out busi_stat_info))
                {
                    add_stat_2_info(p, ref busi_stat_info);
                }
                else
                {
                    busi_stat_info = new STAT_INFO();
                    add_stat_2_info(p, ref busi_stat_info);
                    busi_stat.Add(p.op, info);
                }               
            }*/
            return;
        }

        public int add_stat_2_info(RECORD_INFO p, ref STAT_INFO info)
        {
            //STAT_INFO &info = _map_stat_info[p->op];

            info.all_total_t += p.cost_t;

		    for (int i = _vec_cost_time.Count -1; i >= 0; i--)
		    {
			    if (p.cost_t > _vec_cost_time[i])
			    {
                    int num;
                    if (info.map_time.TryGetValue(_vec_cost_time[i], out num))
                    {
                        info.map_time[_vec_cost_time[i]]++;
                    }
                    else
                    {
                        info.map_time.Add(_vec_cost_time[i], 1);
                    }
				    break;
			    }
		    }

            if (p.errcode == 1)
            {
                info.succ_num++;
                info.suc_total_t += p.cost_t;
                info.size += p.size;
            }
            else
            {
                int count = 0;
                if (info.map_ip.TryGetValue(p.ip, out count))
                {
                    info.map_ip[p.ip]++;
                }
                else
                {
                    info.map_ip.Add(p.ip, 1);
                }

                if (info.map_uin.TryGetValue(p.uin, out count))
                {
                    info.map_uin[p.uin]++;
                }
                else
                {
                    info.map_uin.Add(p.uin, 1);
                }
                info.err_total_t += p.cost_t;

                if (info.map_err.TryGetValue(p.errcode, out count))
                {
                    info.map_err[p.errcode]++;
                }
                else
                {
                    info.map_err.Add(p.errcode, 1);
                }

                info.err_num++;
                if (_map_normal_err.ContainsKey(p.errcode))
                {
                    info.err_normal++;
                }
                else
                {
                    info.err_fail++;
                }
            }
            return 0;
        }

        public void myprintf(List<FileStream> vec_fp, string data)
        {
            foreach (var item in vec_fp)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                item.Write(bytes, 0, bytes.Length);
                item.Flush();
            }
        }

        public void print_info(System.DateTime now_time)
        {
            if (!((int)(now_time - _cur_time).TotalSeconds > _log_time))
                return;

            _cur_time = now_time;

            lock (_map_stat_info)
            {
                write_stat_info_2_file(_map_stat_info, "", _cur_time);

                foreach (var item in _map_biz_stat)
                {
                    if (item.Key != UNKNOWN_BIZ)
                    {
                        write_stat_info_2_file(item.Value, item.Key, _cur_time);
                    }
                }

                _map_stat_info.Clear();
                _map_biz_stat.Clear();
            }
            return;

        }


        public void write_stat_info_2_file(Dictionary<string, STAT_INFO> map_stat_info, string biz_id, System.DateTime _cur_time)
        {

            List<FileStream> vec_fp = new List<FileStream>();
            //_cur_time = now_time;
            string file; ;

            try
            {
                if (biz_id.Length == 0)
                {
                    file = String.Format("{0}.{1}.log", _log_file, _cur_time.ToString(DateFormat));
                    checkAndCreateFile(file);
                    FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    if (fs != null)
                    {
                        vec_fp.Add(fs);
                    }
                    file = String.Format("{0}.report", _log_file);
                    checkAndCreateFile(file);
                    FileStream fs2 = new FileStream(file, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                    if (fs2 != null)
                    {
                        vec_fp.Add(fs2);
                    }
                }
                else
                {
                    file = String.Format("{0}_{1}.{2}.log", _log_file, biz_id, _cur_time.ToString(DateFormat));
                    checkAndCreateFile(file);
                    FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    if (fs != null)
                    {
                        vec_fp.Add(fs);
                    }

                    file = String.Format(" {0}_{1}.report", _log_file, biz_id);
                    checkAndCreateFile(file);
                    FileStream fs2 = new FileStream(file, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                    if (fs2 != null)
                    {
                        vec_fp.Add(fs2);
                    }
                }

                if (vec_fp.Count == 0)
                {
                    return;
                }

                if (map_stat_info.Count == 0)
                {
                    foreach (var item in vec_fp)
                    {
                        item.Close();
                    }
                    return;
                }

                //- print stat info
                string temp = string.Format("{0}\r\n", _cur_time.ToString(TimeFormat));
                myprintf(vec_fp, temp);

                temp = string.Format(" ===============================================================================================\r\n");
                myprintf(vec_fp, temp);

                temp = string.Format("     Operation| {0,7} {1,7} {2,7} {3,7} {4,7} {5,7} {6,7}", "Success", "Error", "Err_nor", "Err_err", "Cost_t", "Suc_t", "Fail_t");
                myprintf(vec_fp, temp);

                foreach (var item in _vec_cost_time)
                {
                    string tmp = string.Format(">{0}", item);

                    tmp = string.Format(" {0,7}", tmp);
                    myprintf(vec_fp, tmp);

                }
                myprintf(vec_fp, "\r\n");
                myprintf(vec_fp, " -----------------------------------------------------------------------------------------------\r\n");
                int total_suc = 0;
                int total_err = 0;
                int total_err_nor = 0;
                int total_err_fail = 0;
                int[] cost_t_num = new int[50];


                foreach (var it in map_stat_info)
                {
                    STAT_INFO info = it.Value;
                    int cost_t = 0;
                    int suc_c_t = 0;
                    int fail_c_t = 0;
                    if (info.succ_num + info.err_num > 0)
                    {
                        cost_t = (int)info.all_total_t / (info.succ_num + info.err_num);
                    }
                    if (info.succ_num > 0)
                    {
                        suc_c_t = (int)info.suc_total_t / info.succ_num;
                    }
                    if (info.err_num > 0)
                    {
                        fail_c_t = (int)info.err_total_t / info.err_num;
                    }
                    temp = string.Format("{0,14}| {1,7} {2,7} {3,7} {4,7} {5,7} {6,7} {7,7}",
                            it.Key, info.succ_num, info.err_num, info.err_normal, info.err_fail,
                            cost_t, suc_c_t, fail_c_t);

                    myprintf(vec_fp, temp);

                    int i = 0;
                    foreach (var ii in _vec_cost_time)
                    {
                        int num = 0;
                        info.map_time.TryGetValue(ii, out num);
                        temp = string.Format(" {0,7}", num);
                        myprintf(vec_fp, temp);

                        cost_t_num[i] += num;
                        i++;
                    }
                    myprintf(vec_fp, "\r\n");
                    total_suc += info.succ_num;
                    total_err += info.err_num;
                    total_err_nor += info.err_normal;
                    total_err_fail += info.err_fail;
                }
                myprintf(vec_fp, " -----------------------------------------------------------------------------------------------\r\n");

                temp = string.Format("{0,14}  {1,7} {2,7} {3,7} {4,7} {5,7} {6,7} {7,7}", "total", total_suc, total_err, total_err_nor, total_err_fail, "/", "/", "/");
                myprintf(vec_fp, temp);

                for (int i = 0; i < _vec_cost_time.Count; i++)
                {
                    temp = string.Format(" {0,7}", cost_t_num[i]);
                    myprintf(vec_fp, temp);
                }
                myprintf(vec_fp, "\r\n");

                //- print size
                myprintf(vec_fp, "\r\n");
                bool is_has_size = false;
                foreach (var it in map_stat_info)
                {
                    STAT_INFO info = it.Value;
                    if (info.size > 0)
                    {
                        is_has_size = true;
                        temp = string.Format("{0}_size: {1}\r\n", it.Key, info.size);
                        myprintf(vec_fp, temp);
                    }
                }
                //- print err distributed
                if (is_has_size)
                {
                    myprintf(vec_fp, "\r\n");
                }

                myprintf(vec_fp, "\r\n\r\n\r\n\r\n");
                //- end

                map_stat_info.Clear();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally {
                if (vec_fp != null && vec_fp.Count != 0) {
                    foreach (var item in vec_fp)
                    {
                        if (item != null) {
                            item.Close();
                        }
                    }
                }
            }
            
        }

        public void checkAndCreateFile(String path) {
            if (path == null) {
                return;
            }
            int idx = path.LastIndexOf("\\");
            String dir = path.Substring(0,idx);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(path)) {
                File.Create(path).Close();
            }
        }

    }
}

