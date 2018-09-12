using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using YW.Model.Entity;
using YW.Data;
namespace YW.Logic
{
    public class Project
    {
        private static Project _project = null;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<string, Model.Entity.Project> _dictionaryById;

        public static Project GetInstance()
        {
            if (_project == null)
            {
                lock (LockHelper)
                {
                    if (_project == null)
                    {
                        _project = new Project();
                    }
                }
            }
            return _project;
        }

        public Project()
        {
            _dictionaryById = new Dictionary<string, Model.Entity.Project>();
            const string sql ="select [Project].* from [Project]";
            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var item = new Model.Entity.Project
                    {
                        ProjectId = ds.Tables[0].Rows[i]["ProjectId"].ToString(),
                        Name= ds.Tables[0].Rows[i]["Name"].ToString(),
                        SMSKey = ds.Tables[0].Rows[i]["SMSKey"].ToString(),
                        SMSReg = ds.Tables[0].Rows[i]["SMSReg"].ToString(),
                        SMSForgot = ds.Tables[0].Rows[i]["SMSForgot"].ToString(),
                        AppleVersion = int.Parse(ds.Tables[0].Rows[i]["AppleVersion"].ToString()),
                        AppleUrl = ds.Tables[0].Rows[i]["AppleUrl"].ToString(),
                        AppleDescription = ds.Tables[0].Rows[i]["AppleDescription"].ToString(),
                        AndroidVersion = int.Parse(ds.Tables[0].Rows[i]["AndroidVersion"].ToString()),
                        AndroidUrl = ds.Tables[0].Rows[i]["AndroidUrl"].ToString(),
                        AndroidDescription = ds.Tables[0].Rows[i]["AndroidDescription"].ToString(),
                        AD = (ds.Tables[0].Rows[i]["AD"]!=DBNull.Value? ds.Tables[0].Rows[i]["AD"].ToString():null)

                    };
                    _dictionaryById.Add(item.ProjectId, item);
                }
            }
           
        }

        public List<Model.Entity.Project> GetList()
        {
            return _dictionaryById.Values.ToList();
        }

        public Model.Entity.Project Get(string objId)
        {
            Model.Entity.Project obj;
            _dictionaryById.TryGetValue(objId, out obj);
            return obj;
        }

        public void Del(string objId)
        {
            lock (_dictionaryById)
            {
                if (_dictionaryById.ContainsKey(objId))
                {
                    const string sql = "Delete from [Project] where [Project].[ProjectId]=@ProjectId";
                    var commandParameters = new DbParameter[]
                    {
                        Data.DBHelper.CreateInDbParameter("@ProjectId", DbType.String, objId)
                    };
                    Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
                    _dictionaryById.Remove(objId);
                }
            }
        }

        /// <summary>
        /// 此方法判断要传递的参数是否为null， 如果为Null, 则返回值DBNLL.Value,主要用户网数据库添加或者更新数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object CheckIsNull(object obj)
        {
            if(obj==null)
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }


        public void Save(Model.Entity.Project project)
        {

            lock (_dictionaryById)
            {
                string sql;
                if (_dictionaryById.ContainsKey(project.ProjectId))
                {
                    sql =
                        "update [Project] set [Name]=@Name,[SMSKey]=@SMSKey,[SMSReg]=@SMSReg,[SMSForgot]=@SMSForgot,[AppleVersion]=@AppleVersion,[AppleUrl]=@AppleUrl,[AppleDescription]=@AppleDescription,[AndroidVersion]=@AndroidVersion,[AndroidUrl]=@AndroidUrl,[AndroidDescription]=@AndroidDescription,[AD]=@AD where [Project].[ProjectId]=@ProjectId";
                }
                else
                {
                    sql =
                        "insert into [Project] (ProjectId,Name,SMSKey,SMSReg,SMSForgot,AppleVersion,AppleUrl,AppleDescription,AndroidVersion,AndroidUrl,AndroidDescription,AD) values (@ProjectId,@Name,@SMSKey,@SMSReg,@SMSForgot,@AppleVersion,@AppleUrl,@AppleDescription,@AndroidVersion,@AndroidUrl,@AndroidDescription,@AD)";
                }
                var commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@ProjectId", DbType.String, CheckIsNull(project.ProjectId)),
                    Data.DBHelper.CreateInDbParameter("@Name", DbType.String, CheckIsNull(project.Name)),
                    Data.DBHelper.CreateInDbParameter("@SMSKey", DbType.String, CheckIsNull(project.SMSKey)),
                    Data.DBHelper.CreateInDbParameter("@SMSReg", DbType.String, CheckIsNull(project.SMSReg)),
                    Data.DBHelper.CreateInDbParameter("@SMSForgot", DbType.String, CheckIsNull(project.SMSForgot)),
                    Data.DBHelper.CreateInDbParameter("@AppleVersion", DbType.Int32, CheckIsNull(project.AppleVersion)),
                    Data.DBHelper.CreateInDbParameter("@AppleUrl", DbType.String, CheckIsNull(project.AppleUrl)),
                    Data.DBHelper.CreateInDbParameter("@AppleDescription", DbType.String, CheckIsNull(project.AppleDescription)),
                    Data.DBHelper.CreateInDbParameter("@AndroidVersion", DbType.Int32, CheckIsNull(project.AndroidVersion)),
                    Data.DBHelper.CreateInDbParameter("@AndroidUrl", DbType.String, CheckIsNull(project.AndroidUrl)),
                    Data.DBHelper.CreateInDbParameter("@AndroidDescription", DbType.String, CheckIsNull(project.AndroidDescription)),
                    Data.DBHelper.CreateInDbParameter("@AD", DbType.String,GetItemValue(CheckIsNull(project.AD))),
                };
                Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
                if (!_dictionaryById.ContainsKey(project.ProjectId))
                {
                    _dictionaryById.Add(project.ProjectId, project);
                }
                else
                {
                    _dictionaryById[project.ProjectId].Name = project.Name;
                    _dictionaryById[project.ProjectId].SMSKey = project.SMSKey;
                    _dictionaryById[project.ProjectId].SMSReg = project.SMSReg;
                    _dictionaryById[project.ProjectId].SMSForgot = project.SMSForgot;
                    _dictionaryById[project.ProjectId].AppleVersion = project.AppleVersion;
                    _dictionaryById[project.ProjectId].AppleUrl = project.AppleUrl;
                    _dictionaryById[project.ProjectId].AppleDescription = project.AppleDescription;
                    _dictionaryById[project.ProjectId].AndroidVersion = project.AndroidVersion;
                    _dictionaryById[project.ProjectId].AndroidUrl = project.AndroidUrl;
                    _dictionaryById[project.ProjectId].AndroidDescription = project.AndroidDescription;
                    _dictionaryById[project.ProjectId].AD = project.AD;
                }

            }
        }
        private object GetItemValue(Object obj)
        {
            if (obj == null)
                return DBNull.Value;
            else
                return obj;
        }
    }
}
