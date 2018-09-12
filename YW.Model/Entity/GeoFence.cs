using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Entity
{
    public class GeoFence
    {
        private int _GeofenceID;
        private string _FenceName;
        private bool _Entry;
        private bool _Exit;
        private DateTime _CreateTime;
        private DateTime _UpdateTime;
        private bool _Enable;
        private int _DeviceID;
        private string _Description;
        private string _LatAndLng;

        public int GeofenceID
        {
            get { return _GeofenceID; }
            set { _GeofenceID = value; }
        }
        public DateTime UpdateTime
        {
            get { return _UpdateTime; }
            set { _UpdateTime = value; }
        }
        public string LatAndLng
        {
            get { return _LatAndLng; }
            set { _LatAndLng = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public int DeviceID
        {
            get { return _DeviceID; }
            set { _DeviceID = value; }
        }

        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; }
        }

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        public bool Exit
        {
            get { return _Exit; }
            set { _Exit = value; }
        }

        public bool Entry
        {
            get { return _Entry; }
            set { _Entry = value; }
        }

        public string FenceName
        {
            get { return _FenceName; }
            set { _FenceName = value; }
        }

    }
}
