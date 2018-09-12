using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
namespace YW.Logic
{
    public class FeedBack:Base
    {
        public static FeedBack _object;
        public static readonly object LockHelper=new object();
        public static FeedBack GetInstance()
        {
            if (_object==null)
            {
                lock (LockHelper)
                {
                    if (_object==null)
                    {
                        _object = new FeedBack();
                    }
                }
            }
            return _object;
        }
        public FeedBack()
            : base(typeof(Model.Entity.Feedback))
        {

        }
        public List<Model.Entity.Feedback> GetList(int userId)
        {
            const string sqlCommond = "select * from Feedback where QuestionUserID=@userId and Deleted=0";
            DbParameter[] dp=new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@userId",DbType.Int32,userId)
            };
            DataSet ds= Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond, dp);
            return base.TableToList<Model.Entity.Feedback>(ds);
        }
        public Model.Entity.Feedback GetByFeedbackId(int feedbackId)
        {
            const string sqlCommond = "select * from Feedback where FeedbackID=@feedbackId";
            DbParameter[] dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@feedbackId",DbType.Int32,feedbackId)
            };
            DataSet ds= Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond, dp);
            return base.TableToList<Model.Entity.Feedback>(ds)[0];
        }

        public void Save(Model.Entity.Feedback feedBack)
        {
            base.Save(feedBack);
        }
        public void Delete(int feedbackId)
        {
            base.Del(feedbackId);
        }
    }
}
