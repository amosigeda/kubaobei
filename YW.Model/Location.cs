using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace YW.Model
{
    public class Location
    {
        [BsonIdAttribute]
        [MongoId]
        public Guid Id { get; set; }

        public Location(int deviceId, DateTime? time, DateTime create)
        {
            this.Id = GenerateId(deviceId, time, create);
        }

        public static Guid GenerateId(int deviceId, DateTime? time, DateTime? create)
        {
            byte[] bytes1 = null;
            if (time != null)
            {
                long timeL = time.Value.Ticks / 100000;
                bytes1 = BitConverter.GetBytes(timeL); //低位在前，高位在后
                Array.Reverse(bytes1); //反转
            }
            else
            {
                bytes1 = new byte[8];
            }

            byte[] bytes2 = null;
            if (create == null)
            {
                bytes2 = new byte[8];
            }
            else
            {
                bytes2 = BitConverter.GetBytes(create.Value.Ticks / 100000);
                Array.Reverse(bytes2); //反转
            }
            bytes2[0] = bytes1[6];
            bytes2[1] = bytes1[7];
            return new System.Guid(deviceId, BitConverter.ToInt16(bytes1, 2), BitConverter.ToInt16(bytes1, 4), bytes2);
        }

        public int DeviceId
        {
            get
            {
                var bytes = this.Id.ToByteArray();
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public DateTime Time
        {
            get
            {
                var bytes = this.Id.ToByteArray();
                var bytes2 = new byte[8];
                Array.Copy(bytes, 4, bytes2, 2, 6);
                Array.Reverse(bytes2); //反转
                return DateTime.FromBinary(BitConverter.ToInt64(bytes2, 0)*100000);
            }
        }

        public DateTime CreateTime
        {
            get
            {
                var bytes = Id.ToByteArray();
                var bytes2 = new byte[8];
                Array.Copy(bytes, 10, bytes2, 2, 6);
                Array.Reverse(bytes2); //反转
                return DateTime.FromBinary(BitConverter.ToInt64(bytes2, 0)*100000);
            }
        }



        /// <summary>
        /// 返回guid1是否大于guid2
        /// </summary>
        /// <param name="guid1"></param>
        /// <param name="guid2"></param>
        /// <returns></returns>
        public static bool GreaterThan(System.Guid guid1, System.Guid guid2)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            for (int i = 0; i < 16; i++)
            {
                if (bytes1[i] < bytes2[i])
                {
                    return false;
                }
            }
            return true;
        }
        [BsonElementAttribute("la")]
        [MongoAliasAttribute("la")]
        public double? Lat { get; set; }
        [BsonElementAttribute("ln")]
        [MongoAliasAttribute("ln")]
        public double? Lng { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("ra")]
        [MongoAliasAttribute("ra")]
        public int? Radius { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("sp")]
        [MongoAliasAttribute("sp")]
        public double? Speed { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("c")]
        [MongoAliasAttribute("c")]
        public double? Course { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("a")]
        [MongoAliasAttribute("a")]
        public double? Altitude { get; set; }

        /// <summary>
        ///0未定位 1 GPS 定位，2LBS 定位 3Wifi
        /// </summary>
        [BsonElementAttribute("lt")]
        [MongoAliasAttribute("lt")]
        public int LocationType { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("w")]
        [MongoAliasAttribute("w")]
        public string WIFI { get; set; }
        //[MongoIgnore]
        //[BsonIgnoreAttribute]
        [BsonElementAttribute("lb")]
        [MongoAliasAttribute("lb")]
        public string LBS { get; set; }
        [BsonElementAttribute("s")]
        [MongoAliasAttribute("s")]
        public int Status { get; set; }
        [BsonElementAttribute("u")]
        [MongoAliasAttribute("u")]
        public DateTime UpdateTime { get; set; }
    }
}
