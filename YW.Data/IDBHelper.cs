using System.Data;
using System.Data.Common;

namespace YW.Data
{
    public interface IDBHelper
    {
        DbConnection CreateConnection(string connectionString);
        DbTransaction CreateDbTransaction(string connectionString);
        DbTransaction CreateDbTransaction(DbConnection conn);
        int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        int ExecuteNonQuery(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        DataSet ExecuteAdapter(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        DataSet ExecuteAdapter(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        DbDataReader ExecuteReader(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        object ExecuteScalar(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters);
        void ExecuteSave(string connectionString, DataTable dt, string cmdText);
        void ExecuteSave(DbConnection conn, DataTable dt, string cmdText);
        void ExecuteSave(DbTransaction trans, DataTable dt, string cmdText);
    }
}
