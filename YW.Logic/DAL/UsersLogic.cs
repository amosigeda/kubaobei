using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Model.Entity;
using YW.Model;
using YW.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
namespace YW.Logic.DAL
{
    public class UsersLogic
    {
        public static volatile UsersLogic _UsersLogic = null;

        /// <summary>
        /// get UsersLogic Instance
        /// </summary>
        /// <returns></returns>
        public static UsersLogic GetInstance()
        {
            if (_UsersLogic==null)
            {
                lock (typeof(UsersLogic))
                {
                    if (_UsersLogic==null)
                    {
                        _UsersLogic = new UsersLogic();
                    }
                }
            }
            return _UsersLogic;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int InsertUser(User user)
        {
            string sqlCommand = "Insert into User(PhoneNumber,Password,LoginID,UserType,Name,Deleted,Notification,NotificationSound,NotificationVibration,CreateTime,UpdateTime)";
            sqlCommand += "Values(@PhoneNumber,@Password,@LoginID,@UserType,@Name,@Deleted,@Notification,@NotificationSound,@NotificationVibration,@CreateTime,@UpdateTime)";
            //DbParameter[] parameter =null;
            //parameter[0].DbType = DbType.String;
            //parameter[0].ParameterName = "@PhoneNumber";
            //parameter[0].Value = user.PhoneNumber;
            DbParameter[] parame = new SqlParameter[]{
               new SqlParameter("@PhoneNumber",user.PhoneNumber),
               new SqlParameter("@Password",user.Password),
               new SqlParameter("@LoginID",user.LoginID),
               new SqlParameter("@UserType",user.UserType),
               new SqlParameter("@Name",user.Name),
               new SqlParameter("@Deleted",user.Deleted),
               new SqlParameter("@Notification",user.Notification),
               new SqlParameter("@NotificationSound",user.NotificationSound),
               new SqlParameter("@NotificationVibration",user.NotificationVibration),
               new SqlParameter("@CreateTime",user.CreateTime),
               new SqlParameter("@UpdateTime",user.UpdateTime)
            };
            //List<DbParameter> ldp = new List<DbParameter>();
            //ldp.Add();
            //ldp.ToArray();
            int res=DBHelper.GetInstance().ExecuteNonQuery(sqlCommand,parame);
            return res;
        }


        public User GetUsers(string userName,string passWord)
        {
            User _user = null;
            string sqlCommand = "select * from User where Deleted=0 and LoginID="+userName+" and Password="+passWord;
            IDataReader dr= DBHelper.GetInstance().ExecuteReader(sqlCommand);
            while (dr.Read())
            {
                _user.PhoneNumber = DataReaderHelper.GetString(dr,"PhoneNumber");
                _user.Password = DataReaderHelper.GetString(dr,"Password");
                _user.LoginID = DataReaderHelper.GetString(dr,"LoginID");
                _user.UserType = DataReaderHelper.GetInt16(dr,"UserType");
                _user.Name = DataReaderHelper.GetString(dr,"Name");
                _user.Deleted = DataReaderHelper.GetBoolean(dr,"Deleted");
                _user.Notification = DataReaderHelper.GetBoolean(dr,"Notification");
                _user.NotificationSound = DataReaderHelper.GetBoolean(dr, "NotificationSound");
                _user.NotificationVibration = DataReaderHelper.GetBoolean(dr, "NotificationVibration");
                _user.CreateTime=DataReaderHelper.GetDateTime(dr,"CreateTime");
                _user.UpdateTime = DataReaderHelper.GetDateTime(dr,"UpdateTime");
            }
            return _user;

        }


    }
}
