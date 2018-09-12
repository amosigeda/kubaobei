using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace YW.Data
{
    public class DBHelper
    {
        private static DBHelper _DBHelper;
        private static object LockHelper = new object();
        private IDBHelper _IDBHelper;
        public enum DatabaseType
        {
            SqlServer, MySql, Oracle, OleDb
        }
        #region 创建数据库操作类
        public DBHelper()
        {
            switch (Config.GetInstance().DatabaseType)
            {
                case DatabaseType.SqlServer:
                    _IDBHelper = new SqlHelper();
                    break;
                default:
                    _IDBHelper = new SqlHelper();
                    break;
            }
        }
        public static DBHelper GetInstance()
        {
            if (_DBHelper == null)
            {
                lock (LockHelper)
                {
                    if (_DBHelper == null)
                    {
                        _DBHelper = new DBHelper();
                    }
                }
            }
            return _DBHelper;
        }
        #endregion
        #region 创建 DbParameter
        /// <summary>
        /// 创造输入DbParameter的实例
        /// </summary>
        public static DbParameter CreateInDbParameter(string paraName, DbType dbType, int size, object value)
        {
            return CreateDbParameter(paraName, dbType, size, value, ParameterDirection.Input);
        }
        /// <summary>
        /// 创造输入DbParameter的实例
        /// </summary>
        public static DbParameter CreateInDbParameter(string paraName, DbType dbType, object value)
        {
            return CreateDbParameter(paraName, dbType, 0, value, ParameterDirection.Input);
        }
        /// <summary>
        /// 创造输出DbParameter的实例
        /// </summary>        
        public static DbParameter CreateOutDbParameter(string paraName, DbType dbType, int size)
        {
            return CreateDbParameter(paraName, dbType, size, null, ParameterDirection.Output);
        }
        /// <summary>
        /// 创造输出DbParameter的实例
        /// </summary>        
        public static DbParameter CreateOutDbParameter(string paraName, DbType dbType)
        {
            return CreateDbParameter(paraName, dbType, 0, null, ParameterDirection.Output);
        }
        /// <summary>
        /// 创造返回DbParameter的实例
        /// </summary>        
        public static DbParameter CreateReturnDbParameter(string paraName, DbType dbType, int size)
        {
            return CreateDbParameter(paraName, dbType, size, null, ParameterDirection.ReturnValue);
        }
        /// <summary>
        /// 创造返回DbParameter的实例
        /// </summary>        
        public static DbParameter CreateReturnDbParameter(string paraName, DbType dbType)
        {
            return CreateDbParameter(paraName, dbType, 0, null, ParameterDirection.ReturnValue);
        }
        /// <summary>
        /// 创造DbParameter的实例
        /// </summary>
        public static DbParameter CreateDbParameter(string paraName, DbType dbType, int size, object value, ParameterDirection direction)
        {
            DbParameter para;
            switch (Config.GetInstance().DatabaseType)
            {
                //case DatabaseType.MySql:
                //    para = new MySqlParameter();
                //    break;
                //case DatabaseType.Oracle:
                //    para = new OracleParameter();
                //    break;
                //case DatabaseType.OleDb:
                //    para = new OleDbParameter();
                //    break;
                case DatabaseType.SqlServer:
                default:
                    para = new SqlParameter();
                    break;
            }
            para.ParameterName = paraName;
            if (size != 0)
                para.Size = size;
            para.DbType = dbType;
            if (value != null)
                para.Value = value;
            para.Direction = direction;
            return para;
        }
        #endregion
        public DbConnection CreateConnection()
        {
            return _IDBHelper.CreateConnection(Config.GetInstance().DB);
        }
        public DbConnection CreateConnection(string connectionString)
        {
            return _IDBHelper.CreateConnection(connectionString);
        }
        public DbTransaction CreateDbTransaction()
        {
            return _IDBHelper.CreateDbTransaction(Config.GetInstance().DB);
        }
        public DbTransaction CreateDbTransaction(string connectionString)
        {
            return _IDBHelper.CreateDbTransaction(connectionString);
        }
        public DbTransaction CreateDbTransaction(DbConnection conn)
        {
            return _IDBHelper.CreateDbTransaction(conn);
        }

        public int ExecuteNonQuery(string cmdText)
        {
            return _IDBHelper.ExecuteNonQuery(Config.GetInstance().DB, CommandType.Text, cmdText, null);
        }
        public int ExecuteNonQuery(string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(Config.GetInstance().DB, CommandType.Text, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(Config.GetInstance().DB, cmdType, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(string connectionString, string cmdText)
        {
            return _IDBHelper.ExecuteNonQuery(connectionString, CommandType.Text, cmdText, null);
        }
        public int ExecuteNonQuery(string connectionString, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(connectionString, CommandType.Text, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(connectionString, cmdType, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(DbConnection conn, string cmdText)
        {
            return _IDBHelper.ExecuteNonQuery(conn, CommandType.Text, cmdText, null);
        }
        public int ExecuteNonQuery(DbConnection conn, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(conn, CommandType.Text, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(conn, cmdType, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(DbTransaction trans, string cmdText)
        {
            return _IDBHelper.ExecuteNonQuery(trans, CommandType.Text, cmdText, null);
        }
        public int ExecuteNonQuery(DbTransaction trans, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(trans, CommandType.Text, cmdText, commandParameters);
        }
        public int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteNonQuery(trans, cmdType, cmdText, commandParameters);
        }

        public DataSet ExecuteAdapter(string cmdText)
        {
            return _IDBHelper.ExecuteAdapter(Config.GetInstance().DB, CommandType.Text, cmdText, null);
        }
        public DataSet ExecuteAdapter(string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(Config.GetInstance().DB, CommandType.Text, cmdText, commandParameters);
        }
        public DataSet ExecuteAdapter(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(Config.GetInstance().DB, cmdType, cmdText, commandParameters);
        }
        public DataSet ExecuteAdapter(string connectionString, string cmdText)
        {
            return _IDBHelper.ExecuteAdapter(connectionString, CommandType.Text, cmdText, null);
        }
        public DataSet ExecuteAdapter(string connectionString, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(connectionString, CommandType.Text, cmdText, commandParameters);
        }
        public DataSet ExecuteAdapter(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(connectionString, cmdType, cmdText, commandParameters);
        }
        public DataSet ExecuteAdapter(DbConnection conn, string cmdText)
        {
            return _IDBHelper.ExecuteAdapter(conn, CommandType.Text, cmdText, null);
        }
        public DataSet ExecuteAdapter(DbConnection conn, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(conn, CommandType.Text, cmdText, commandParameters);
        }
        public DataSet ExecuteAdapter(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteAdapter(conn, cmdType, cmdText, commandParameters);
        }

        public IDataReader ExecuteReader(string cmdText)
        {
            return _IDBHelper.ExecuteReader(Config.GetInstance().DB, CommandType.Text, cmdText, null);
        }
        public IDataReader ExecuteReader(string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(Config.GetInstance().DB, CommandType.Text, cmdText, commandParameters);
        }
        public IDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(Config.GetInstance().DB, cmdType, cmdText, commandParameters);
        }
        public IDataReader ExecuteReader(string connectionString, string cmdText)
        {
            return _IDBHelper.ExecuteReader(connectionString, CommandType.Text, cmdText, null);
        }
        public IDataReader ExecuteReader(string connectionString, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(connectionString, CommandType.Text, cmdText, commandParameters);
        }
        public IDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(connectionString, cmdType, cmdText, commandParameters);
        }
        public IDataReader ExecuteReader(DbConnection conn, string cmdText)
        {
            return _IDBHelper.ExecuteReader(conn, CommandType.Text, cmdText, null);
        }
        public IDataReader ExecuteReader(DbConnection conn, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(conn, CommandType.Text, cmdText, commandParameters);
        }
        public IDataReader ExecuteReader(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteReader(conn, cmdType, cmdText, commandParameters);
        }

        public object ExecuteScalar(string cmdText)
        {
            return _IDBHelper.ExecuteScalar(Config.GetInstance().DB, CommandType.Text, cmdText, null);
        }
        public object ExecuteScalar(string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(Config.GetInstance().DB, CommandType.Text, cmdText, commandParameters);
        }
        public object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(Config.GetInstance().DB, cmdType, cmdText, commandParameters);
        }
        public object ExecuteScalar(string connectionString, string cmdText)
        {
            return _IDBHelper.ExecuteScalar(connectionString, CommandType.Text, cmdText, null);
        }
        public object ExecuteScalar(string connectionString, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(connectionString, CommandType.Text, cmdText, commandParameters);
        }
        public object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(connectionString, cmdType, cmdText, commandParameters);
        }
        public object ExecuteScalar(DbConnection conn, string cmdText)
        {
            return _IDBHelper.ExecuteScalar(conn, CommandType.Text, cmdText, null);
        }
        public object ExecuteScalar(DbConnection conn, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(conn, CommandType.Text, cmdText, commandParameters);
        }
        public object ExecuteScalar(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(conn, cmdType, cmdText, commandParameters);
        }
        public object ExecuteScalar(DbTransaction trans, string cmdText)
        {
            return _IDBHelper.ExecuteScalar(trans, CommandType.Text, cmdText, null);
        }
        public object ExecuteScalar(DbTransaction trans, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(trans, CommandType.Text, cmdText, commandParameters);
        }
        public object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            return _IDBHelper.ExecuteScalar(trans, cmdType, cmdText, commandParameters);
        }

        public void ExecuteSave(DataTable dt, string cmdText)
        {
            _IDBHelper.ExecuteSave(Config.GetInstance().DB, dt, cmdText);
        }
        public void ExecuteSave(string connectionString, DataTable dt, string cmdText)
        {
            _IDBHelper.ExecuteSave(connectionString, dt, cmdText);
        }
        public void ExecuteSave(DbConnection conn, DataTable dt, string cmdText)
        {
            _IDBHelper.ExecuteSave(conn, dt, cmdText);
        }
        public void ExecuteSave(DbTransaction trans, DataTable dt, string cmdText)
        {
            _IDBHelper.ExecuteSave(trans, dt, cmdText);
        }
    }
}
