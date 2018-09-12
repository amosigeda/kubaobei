using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class DealerUser : Base
    {
        private static DealerUser _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DealerUser> _dictionaryById;
        private readonly Dictionary<string, Model.Entity.DealerUser> _dictionaryByUserName;
        public static DealerUser GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DealerUser();
                    }
                }
            }
            return _object;
        }
        public DealerUser()
            : base(typeof(Model.Entity.DealerUser))
        {
            var list = base.Get<Model.Entity.DealerUser>();
            _dictionaryById = new Dictionary<int, Model.Entity.DealerUser>();
            _dictionaryByUserName = new Dictionary<string, Model.Entity.DealerUser>();
            foreach (var item in list)
            {
                if (item.Status == 1)
                {
                    _dictionaryById.Add(item.DealerUserId, item);
                    _dictionaryByUserName.Add(item.UserName.ToLower(), item);
                }
            }
        }

        public List<Model.Entity.DealerUser> GetList()
        {
            return _dictionaryById.Values.ToList();
        }

        public Model.Entity.DealerUser Get(int objId)
        {
            Model.Entity.DealerUser obj;
            _dictionaryById.TryGetValue(objId, out obj);
            return obj;
        }
        public Model.Entity.DealerUser Get(string userName)
        {
            Model.Entity.DealerUser obj;
            _dictionaryByUserName.TryGetValue(userName.ToLower(), out obj);
            return obj;
        }
        public void Save(Model.Entity.DealerUser obj)
        {
            base.Save(obj);
            if (obj.DealerUserId != 0)
            {
                if (_dictionaryById.ContainsKey(obj.DealerUserId))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryById[obj.DealerUserId])
                        base.CopyValue<Model.Entity.Dealer>(obj, _dictionaryById[obj.DealerUserId]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryById.Add(obj.DealerUserId, obj);
                    _dictionaryByUserName.Add(obj.UserName.ToLower(), obj);
                }
            }
        }
        public new void Del(int objId)
        {
            if (_dictionaryById.ContainsKey(objId))
            {
                _dictionaryById[objId].Status = 0;
                this.Save(_dictionaryById[objId]);
                _dictionaryByUserName.Remove(_dictionaryById[objId].UserName.ToLower());
                _dictionaryById.Remove(objId);
            }
        }
    }
}
