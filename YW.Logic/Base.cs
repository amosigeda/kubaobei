using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;


namespace YW.Logic
{
    public class Base
    {
        private readonly Type _type;
        private readonly List<System.Reflection.PropertyInfo> _propertyInfo;
        private readonly System.Reflection.PropertyInfo[] _propertyInfoAll;
        private readonly int[] _propertyInfoAllDataType;
        private readonly int[] _propertyInfoDataType;
        private readonly System.Data.DbType[] _propertyInfoDbType;
        private readonly string _dbProcedureGetList;
        private readonly string _dbProcedureSaveObject;
        private readonly string _dbProcedureDelObject;

        protected Base(Type type)
        {
            _type = type;
            _propertyInfoAll = type.GetProperties();
            _propertyInfoAllDataType = new int[_propertyInfoAll.Count()];
            var sbSqlInsertItem = new StringBuilder();
            var sbSqlInsertValue = new StringBuilder();
            var sbSqlUpdate = new StringBuilder();
            for (int i = 0; i < _propertyInfoAll.Count(); i++)
            {
                if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.Int32", System.StringComparison.Ordinal) !=
                    -1)
                    _propertyInfoAllDataType[i] = 1;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.DateTime",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 2;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.Double",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 3;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.Decimal",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 4;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.Single",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 5;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.String",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 6;
                else if (
                    _propertyInfoAll[i].PropertyType.FullName.IndexOf("System.Guid",
                        System.StringComparison.Ordinal) != -1)
                    _propertyInfoAllDataType[i] = 7;
                else
                    _propertyInfoAllDataType[i] = 0;
                if (i != 0)
                {
                    sbSqlInsertItem.Append("[" + _propertyInfoAll[i].Name + "],");
                    if (_propertyInfoAll[i].Name.ToLower().Equals("createtime"))
                    {
                        sbSqlInsertValue.Append("GETDATE(),");
                    }
                    else if (_propertyInfoAll[i].Name.ToLower().Equals("updatetime"))
                    {
                        sbSqlInsertValue.Append("GETDATE(),");
                        sbSqlUpdate.Append("[" + _propertyInfoAll[i].Name + "]=GETDATE(),");
                    }
                    else
                    {
                        sbSqlInsertValue.Append("@" + _propertyInfoAll[i].Name + ",");
                        sbSqlUpdate.Append("[" + _propertyInfoAll[i].Name + "]=@" + _propertyInfoAll[i].Name + ",");
                    }
                }
            }

            sbSqlInsertItem.Remove(sbSqlInsertItem.Length - 1, 1);
            sbSqlInsertValue.Remove(sbSqlInsertValue.Length - 1, 1);
            sbSqlUpdate.Remove(sbSqlUpdate.Length - 1, 1);
            _propertyInfo = type.GetProperties().ToList();
            for (int i = 0; i < _propertyInfo.Count(); i++)
            {
                if (_propertyInfo[i].Name == "CreateTime" || _propertyInfo[i].Name == "UpdateTime")
                {
                    _propertyInfo.RemoveAt(i);
                    i--;
                }
            }

            string dbName = type.Name;
            _propertyInfoDataType = new int[_propertyInfo.Count()];
            _propertyInfoDbType = new DbType[_propertyInfo.Count()];
            for (int i = 0; i < _propertyInfo.Count(); i++)
            {
                if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.DateTime", System.StringComparison.Ordinal) !=
                    -1)
                {
                    _propertyInfoDataType[i] = 1;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Decimal", System.StringComparison.Ordinal) !=
                    -1)
                {
                    _propertyInfoDataType[i] = 2;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Single",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 3;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Double",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 4;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Int32",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 5;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Byte[]",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 6;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Byte",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 7;
                }
                else if (
                    _propertyInfo[i].PropertyType.FullName.IndexOf("System.Guid",
                        System.StringComparison.Ordinal) != -1)
                {
                    _propertyInfoDataType[i] = 8;
                }
                else
                {
                    _propertyInfoDataType[i] = 0;
                }

                _propertyInfoDbType[i] = GetDbType(_propertyInfo[i].PropertyType);
            }

            _dbProcedureGetList = "SELECT * FROM [" + dbName + "] order by [" + _propertyInfo[0].Name + "] asc";
            StringBuilder sbSql = new StringBuilder();
            if ((_propertyInfo[0].Name).ToLower().Equals((type.Name + "id").ToLower()))
            {
                sbSql.Append("if @" + _propertyInfo[0].Name + " =0\n");
                sbSql.Append("begin\n");
                sbSql.Append("	INSERT INTO [" + dbName + "] (" + sbSqlInsertItem.ToString() + ")VALUES(" +
                             sbSqlInsertValue.ToString() + "\n)");
                sbSql.Append("	SET @" + _propertyInfo[0].Name + " = @@IDENTITY\n");
                sbSql.Append("end\n");
            }
            else
            {
                sbSql.Append("if @" + _propertyInfo[0].Name + " =0 or not exists(select top 1 1 from [" + dbName + "] where [" + _propertyInfo[0].Name +
                             "]=@" + _propertyInfo[0].Name + ")\n");
                sbSql.Append("begin\n");
                sbSql.Append("	INSERT INTO [" + dbName + "] ([" + _propertyInfo[0].Name + "]," + sbSqlInsertItem.ToString() + ")VALUES( @" + _propertyInfo[0].Name + "," + sbSqlInsertValue.ToString() +
                             "\n)");
                sbSql.Append("end\n");
            }

            sbSql.Append("else\n");
            sbSql.Append("begin\n");
            sbSql.Append("	UPDATE [" + dbName + "] SET " + sbSqlUpdate.ToString() + " WHERE [" + _propertyInfo[0].Name +
                         "]=@" + _propertyInfo[0].Name + "\n");
            sbSql.Append("end\n");
            sbSql.Append("select @" + _propertyInfo[0].Name + " as " + _propertyInfo[0].Name);
            _dbProcedureSaveObject = sbSql.ToString();
            _dbProcedureDelObject = "DELETE FROM [" + dbName + "] WHERE [" + _propertyInfo[0].Name + "]=@" +
                                    _propertyInfo[0].Name;
        }

        protected List<T> Get<T>()
        {
            DataSet ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, _dbProcedureGetList);
            if (ds.Tables.Count > 0)
                return TableToList<T>(ds.Tables[0]);
            else
                return
                    null;
        }

        protected List<T> TableToList<T>(DataSet ds)
        {
            if (ds.Tables.Count > 0)
            {
                return this.TableToList<T>(ds.Tables[0]);
            }
            else
            {
                return null;
            }
        }

        protected bool ProcessSqlStr(string inputString)
        {
            const string sqlStr =
                @"and|or|exec|execute|insert|select|delete|update|alter|create|drop|count|\*|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|net +localgroup +administrators";
            try
            {
                if (!string.IsNullOrEmpty(inputString))
                {
                    const string strRegex = @"\b(" + sqlStr + @")\b";
                    Regex regex = new Regex(strRegex, RegexOptions.IgnoreCase);
                    if (true == regex.IsMatch(inputString))
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected List<T> TableToList<T>(DataTable dt)
        {
            List<T> list = new List<T>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    object _object = Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < _propertyInfoAll.Count(); i++)
                    {
                        if (dr[_propertyInfoAll[i].Name] != DBNull.Value)
                        {
                            switch (_propertyInfoAllDataType[i])
                            {
                                case 0:
                                    _propertyInfoAll[i].SetValue(_object, dr[_propertyInfoAll[i].Name], null);
                                    break;
                                case 1:
                                    _propertyInfoAll[i].SetValue(_object,
                                        int.Parse(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                case 2:
                                    _propertyInfoAll[i].SetValue(_object,
                                        DateTime.Parse(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                case 3:
                                    _propertyInfoAll[i].SetValue(_object,
                                        double.Parse(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                case 4:
                                    _propertyInfoAll[i].SetValue(_object,
                                        decimal.Parse(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                case 5:
                                    _propertyInfoAll[i].SetValue(_object,
                                        float.Parse(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                case 6:
                                    _propertyInfoAll[i].SetValue(_object, dr[_propertyInfoAll[i].Name].ToString(), null);
                                    break;
                                case 7:
                                    if (!string.IsNullOrEmpty(dr[_propertyInfoAll[i].Name].ToString()))
                                        _propertyInfoAll[i].SetValue(_object, new Guid(dr[_propertyInfoAll[i].Name].ToString()), null);
                                    break;
                                default:
                                    _propertyInfoAll[i].SetValue(_object, dr[_propertyInfoAll[i].Name], null);
                                    break;
                            }
                        }
                    }

                    list.Add((T) _object);
                }
            }

            return list;
        }

        protected virtual void Save(object obj)
        {
            DbParameter[] commandParameters = new DbParameter[_propertyInfo.Count()];
            object aa = _propertyInfo[0].GetValue(obj, null);
            commandParameters[0] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[0].Name, DbType.Int32, _propertyInfo[0].GetValue(obj, null));
            for (int i = 1; i < _propertyInfo.Count(); i++)
            {
                switch (_propertyInfoDataType[i])
                {
                    case 0:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, _propertyInfoDbType[i], GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 1:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.DateTime, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 2:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Decimal, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 3:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Single, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 4:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Double, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 5:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Int32, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 6:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Binary, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 7:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.Byte, GetItemValue(_propertyInfo[i], obj));
                        break;
                    case 8:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, DbType.String, GetItemValue(_propertyInfo[i], obj).ToString());
                        break;
                    default:
                        commandParameters[i] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[i].Name, _propertyInfoDbType[i], GetItemValue(_propertyInfo[i], obj));
                        break;
                }
            }
            int id = int.Parse(Data.DBHelper.GetInstance().ExecuteScalar(CommandType.Text, _dbProcedureSaveObject, commandParameters).ToString());
            _propertyInfo[0].SetValue(obj, id, null);
            
        }

        private object GetItemValue(System.Reflection.PropertyInfo pi, Object obj)
        {
            var item = pi.GetValue(obj, null);
            if (item == null)
                return DBNull.Value;
            else
                return item;
        }

        protected virtual void Del(int objId)
        {
            DbParameter[] commandParameters = new DbParameter[1];
            commandParameters[0] = Data.DBHelper.CreateInDbParameter("@" + _propertyInfo[0].Name, DbType.Int32, objId);
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, _dbProcedureDelObject, commandParameters);
        }

        protected void CopyValue<T>(object obj1, object obj2)
        {
            for (int i = 0; i < _propertyInfoAll.Count(); i++)
            {
                _propertyInfoAll[i].SetValue(obj2, _propertyInfoAll[i].GetValue(obj1, null), null);
            }
        }

        private DbType GetDbType(Type type)
        {
            DbType dbt;
            try
            {
                dbt = (DbType) Enum.Parse(typeof(DbType), type.Name);
            }
            catch
            {
                dbt = DbType.Object;
            }

            return dbt;
        }
    }
}