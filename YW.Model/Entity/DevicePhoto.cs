using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace YW.Model.Entity
{
    //DeviceVoice
    [Serializable]
    public class DevicePhoto
    {
        private int _devicephotoid;
        /// <summary>
        /// 设备照片编号
        /// </summary>		
        public int DevicePhotoId
        {
            get { return _devicephotoid; }
            set { _devicephotoid = value; }
        }
        private int _deviceid;
        /// <summary>
        /// 设备编号
        /// </summary>		
        public int DeviceID
        {
            get { return _deviceid; }
            set { _deviceid = value; }
        }

        private string _source;
        /// <summary>
        /// 来源
        /// </summary>		
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }
        private DateTime? _devicetime;
        /// <summary>
        /// 设备时间
        /// </summary>		
        public DateTime? DeviceTime
        {
            get { return _devicetime; }
            set { _devicetime = value; }
        }
        private double? _latitude;
        /// <summary>
        /// 发生异常的纬度
        /// </summary>		
        public double? Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }
        private double? _longitude;
        /// <summary>
        /// 发生异常的精度
        /// </summary>		
        public double? Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
        private int _state;
        /// <summary>
        /// 状态 0 正在接收 1接收完成
        /// </summary>		
        public int State
        {
            get { return _state; }
            set { _state = value; }
        }
        private int _totalpackage;
        /// <summary>
        /// 总包数
        /// </summary>		
        public int TotalPackage
        {
            get { return _totalpackage; }
            set { _totalpackage = value; }
        }
        private int _currentpackage;
        /// <summary>
        /// 当前包
        /// </summary>		
        public int CurrentPackage
        {
            get { return _currentpackage; }
            set { _currentpackage = value; }
        }

        private string _mark;
        /// <summary>
        /// 语音标识
        /// </summary>		
        public string Mark
        {
            get { return _mark; }
            set { _mark = value; }
        }
        private string _path;
        /// <summary>
        /// 保存路径
        /// </summary>		
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        private DateTime _createtime;
        /// <summary>
        /// CreateTime
        /// </summary>		
        public DateTime CreateTime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }
        private DateTime _updatetime;
        /// <summary>
        /// UpdateTime
        /// </summary>		
        public DateTime UpdateTime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }

        private string _thumb;

        public string Thumb
        {
            get => _thumb;
            set => _thumb = value;
        }
    }
}

