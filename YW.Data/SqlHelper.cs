using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace YW.Data
{
    public class SqlHelper : IDBHelper
    {
        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
        public DbTransaction CreateDbTransaction(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            return CreateDbTransaction(conn);
        }
        public DbTransaction CreateDbTransaction(DbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn.BeginTransaction();
        }
        public int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                return this.ExecuteNonQuery(conn, cmdType, cmdText, commandParameters);
            }
        }
        public int ExecuteNonQuery(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
        public int ExecuteNonQuery(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }


        public DataSet ExecuteAdapter(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            { 
                return this.ExecuteAdapter(conn, cmdType, cmdText, commandParameters);
            }
        }
        public DataSet ExecuteAdapter(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            cmd.Parameters.Clear();
            return ds;
        }
        public DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }
        public DbDataReader ExecuteReader(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            return rdr;
        }
        public object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                return this.ExecuteScalar(conn, cmdType, cmdText, commandParameters);
            }
        }
        public object ExecuteScalar(DbConnection conn, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
        public object ExecuteScalar(DbTransaction trans, CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        public void BulkCopy(string connectionString, DataTable dt, string destinationTableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                BulkCopy(conn, dt, destinationTableName);
            }
        }
        public void BulkCopy(DbConnection conn, DataTable dt, string destinationTableName)
        {
            SqlConnection sqlconn=(SqlConnection)conn;
            SqlTransaction tran = sqlconn.BeginTransaction();
            using (SqlBulkCopy sbc = new SqlBulkCopy(sqlconn, SqlBulkCopyOptions.Default, tran))
            {
                try
                {
                    sbc.DestinationTableName = destinationTableName;
                    sbc.WriteToServer(dt);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
        public void BulkCopy(string connectionString, DataTable dt, string destinationTableName, string[,] columnMappings)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                BulkCopy(conn, dt, destinationTableName, columnMappings);
            }
        }
        public void BulkCopy(DbConnection conn, DataTable dt, string destinationTableName, string[,] columnMappings)
        {
            SqlConnection sqlconn = (SqlConnection)conn;
            SqlTransaction tran = sqlconn.BeginTransaction();
            using (SqlBulkCopy sbc = new SqlBulkCopy(sqlconn, SqlBulkCopyOptions.Default, tran))
            {
                for (int i = 0; i < columnMappings.Length / 2; i++)
                {
                    sbc.ColumnMappings.Add(columnMappings[i, 0], columnMappings[i, 1]);
                }
                sbc.DestinationTableName = destinationTableName;
                try
                {
                    sbc.WriteToServer(dt);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }
        }
        public void ExecuteSave(string connectionString, DataTable dt, string cmdText)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                ExecuteSave(conn, dt, cmdText);
            }
        }
        public void ExecuteSave(DbConnection conn, DataTable dt, string cmdText)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
            // SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);   
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                SqlCommandBuilder upsda = new SqlCommandBuilder(sda);
                DataTable sdt = null;
                lock (dt)
                {
                    sdt= dt.GetChanges();
                    dt.AcceptChanges();
                }
                if (sdt != null)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    sda.FillSchema(sdt, SchemaType.Mapped);
                    sda.Update(sdt);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > 1000)
                        Logger.Debug("更新" + dt.Rows.Count + "条记录花费" + sw.ElapsedMilliseconds + "毫秒");
                }
                
            }
            cmd.Parameters.Clear();
        }
        public void ExecuteSave(DbTransaction trans, DataTable dt, string cmdText)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, CommandType.Text, cmdText, null);
            // SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);   
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            SqlCommandBuilder upsda = new SqlCommandBuilder(sda);
            DataTable sdt = null;
            lock (dt)
            {
                sdt = dt.GetChanges();
                dt.AcceptChanges();
            }
            if (sdt != null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                sda.Update(sdt);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 1000)
                    Logger.Debug("更新" + dt.Rows.Count + "条记录花费" + sw.ElapsedMilliseconds + "毫秒");
            }
            cmd.Parameters.Clear();
        }
        private void PrepareCommand(SqlCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType, string cmdText, DbParameter[] cmdParms)
        {
            //判断数据库连接状态   
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = (SqlConnection)conn;
                cmd.CommandText = cmdText;
                //判断是否需要事物处理   
                if (trans != null)
                    cmd.Transaction = (SqlTransaction)trans;
                cmd.CommandType = cmdType;
                if (cmdParms != null)
                {
                    foreach (DbParameter parm in cmdParms)
                        cmd.Parameters.Add(parm);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
