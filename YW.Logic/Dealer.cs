using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class Dealer : Base
    {
        private static Dealer _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.Dealer> _dictionaryById;
        public static Dealer GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Dealer();
                    }
                }
            }
            return _object;
        }
        public Dealer()
            : base(typeof(Model.Entity.Dealer))
        {
            var list = base.Get<Model.Entity.Dealer>();
            _dictionaryById = new Dictionary<int, Model.Entity.Dealer>();
            foreach (var item in list)
            {
                _dictionaryById.Add(item.DealerId, item);
            }
        }

        public List<Model.Entity.Dealer> GetList()
        {
            return _dictionaryById.Values.ToList();
        }

        public Model.Entity.Dealer Get(int objId)
        {
            Model.Entity.Dealer obj;
            _dictionaryById.TryGetValue(objId, out obj);
            return obj;
        }
        public void Save(Model.Entity.Dealer obj)
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
