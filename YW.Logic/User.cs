using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using YW.Data;

namespace YW.Logic
{
    public class User : Base
    {
        private static User _object;
        private static readonly object LockHelper = new object();
        private readonly ConcurrentDictionary<int, Model.Entity.User> _dictionaryById;

        public static User GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new User();
                    }
                }
            }

            return _object;
        }

        public User() : base(typeof(Model.Entity.User))
        {
            var list = base.Get<Model.Entity.User>();
            _dictionaryById = new ConcurrentDictionary<int, Model.Entity.User>();
            foreach (var item in list)
            {
                if (item.Deleted)
                    continue;
                _dictionaryById.TryAdd(item.UserID, item);
            }
        }

        public ConcurrentDictionary<int, Model.Entity.User> GetDictionary()
        {
            return _dictionaryById;
        }

        public Model.Entity.User Get(int objId)
        {
            _dictionaryById.TryGetValue(objId, out var obj);
            return obj;
        }

        public Model.Entity.User Get(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            var res = _dictionaryById.FirstOrDefault(x => phoneNumber.Equals(x.Value.PhoneNumber));
            if (default(KeyValuePair<int, Model.Entity.User>).Equals(res))
            {
                return null;
            }
            var usr = res.Value;
            return (usr == null || usr.UserID == 0) ? null : usr;
        }

        public Model.Entity.User GetByBindNumber(string bindNumber)
        {
            if (string.IsNullOrEmpty(bindNumber))
            {
                return null;
            }

            var res = _dictionaryById.FirstOrDefault(x => bindNumber.Equals(x.Value.BindNumber));
            if (default(KeyValuePair<int, Model.Entity.User>).Equals(res))
            {
                return null;
            }
            var usr = res.Value;
            return (usr == null || usr.UserID == 0) ? null : usr;
        }

        public void Save(Model.Entity.User obj)
        {
                base.Save(obj);
                if (obj.UserID != 0)
                {
                    if (_dictionaryById.ContainsKey(obj.UserID))
                    {
                        obj.UpdateTime = DateTime.Now;
                        if (obj != _dictionaryById[obj.UserID])
                        {
                            base.CopyValue<Model.Entity.User>(obj, _dictionaryById[obj.UserID]);
                        }
                    }
                    else
                    {
                        obj.CreateTime = DateTime.Now;
                        obj.UpdateTime = DateTime.Now;
                        _dictionaryById.TryAdd(obj.UserID, obj);
                    }
                }
        }

        public new void Del(int objId)
        {
            if (_dictionaryById.ContainsKey(objId))
            {
                UserDevice.GetInstance().DelUser(objId);
                _dictionaryById[objId].Deleted = true;
                this.Save(_dictionaryById[objId]);
                _dictionaryById.TryRemove(objId, out var res);
            }
        }

        public void DelByBindNumber(string bindNumber)
        {
            Model.Entity.User user = GetByBindNumber(bindNumber);
            if (user != null)
            {
                _dictionaryById.TryRemove(user.UserID, out var res);
            }

            const string sqlCommond = "Delete from [User] where BindNumber=@BindNumber";
            var dp = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@BindNumber", DbType.String, bindNumber),
            };
            DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
        }

        public void DelReal(int objId)
        {
            if (_dictionaryById.ContainsKey(objId))
            {
                UserDevice.GetInstance().DelUser(objId);
                base.Del(objId);
                _dictionaryById.TryRemove(objId, out var res);
            }
        }
    }
}