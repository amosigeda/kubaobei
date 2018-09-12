using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class DealerNotification : Base
    {
        private static DealerNotification _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DealerNotification> _dictionaryById;
        public static DealerNotification GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DealerNotification();
                    }
                }
            }
            return _object;
        }
        public DealerNotification()
            : base(typeof(Model.Entity.DealerNotification))
        {
            var list = base.Get<Model.Entity.DealerNotification>();
            _dictionaryById = new Dictionary<int, Model.Entity.DealerNotification>();
            foreach (var item in list)
            {
                _dictionaryById.Add(item.DealerId, item);
            }
        }

        public List<Model.Entity.DealerNotification> GetList()
        {
            return _dictionaryById.Values.ToList();
        }

        public Model.Entity.DealerNotification Get(int objId)
        {
            Model.Entity.DealerNotification obj;
            _dictionaryById.TryGetValue(objId, out obj);
            return obj;
        }
        public void Save(Model.Entity.DealerNotification obj)
        {
            base.Save(obj);
            if (obj.DealerId != 0)
            {
                if (_dictionaryById.ContainsKey(obj.DealerId))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryById[obj.DealerId])
                        base.CopyValue<Model.Entity.Dealer>(obj, _dictionaryById[obj.DealerId]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryById.Add(obj.DealerId, obj);
                }
            }
        }
        public new void Del(int objId)
        {
            if (_dictionaryById.ContainsKey(objId))
            {
                _dictionaryById[objId].Status = 0;
                this.Save(_dictionaryById[objId]);
                _dictionaryById.Remove(objId);
            }
        }
    }
}
