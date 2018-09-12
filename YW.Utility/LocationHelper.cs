using System;

namespace YW.Utility
{
    public class LocationHelper
    {
        static double pi = 3.14159265358979324;
        static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;

        /**
         * GCJ-02 坐标转换成 BD-09
         * 
         * @param gg_lat
         * @param gg_lon
         * @param coord
         * @return
         */
        public static void GCJToBD09(double gg_lat, double gg_lon,out double bd_lat,out double bd_lon)
        {
            double x = gg_lon, y = gg_lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            bd_lon = z * Math.Cos(theta) + 0.0065;
            bd_lat = z * Math.Sin(theta) + 0.006;
        }

        public static void BD09ToGCJ(double bd_lat, double bd_lon,out double gg_lat,out double gg_lon)
        {
            double x = bd_lon - 0.0065, y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            gg_lon = z * Math.Cos(theta);
            gg_lat = z * Math.Sin(theta);
        }

        //
        // Krasovsky 1940
        //
        // a = 6378245.0, 1/f = 298.3
        // b = a * (1 - f)
        // ee = (a^2 - b^2) / a^2;
        private const double A = 6378245.0;
        private const double Ee = 0.00669342162296594323;

        // GPS转火星坐标
        // World Geodetic System ==> Mars Geodetic System
        /**
         * WGS-84 转 GCJ-02
         * 
         * @param wgLat
         * @param wgLon
         * @param coord
         * @return
         */
        public static void WGS84ToGCJ(double wgLat, double wgLon,out double gcjLat,out double gcjLon)
        {
            if (OutOfChina(wgLat, wgLon))
            {
                gcjLat = wgLat;
                gcjLon = wgLon;
                return;
            }
            double dLat = transformLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = transformLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - Ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((A * (1 - Ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * pi);
            gcjLat = wgLat + dLat;
            gcjLon = wgLon + dLon;
        }
        public static void GCJToWGS84(double gcjLat, double gcjLon, out double wgLat, out double wgLon)
        {
            if (OutOfChina(gcjLat, gcjLon))
            {
                wgLat = gcjLat;
                wgLon = gcjLon;
                return;
            }
            double dLat = transformLat(gcjLon - 105.0, gcjLat - 35.0);
            double dLon = transformLon(gcjLon - 105.0, gcjLat - 35.0);
            double radLat = gcjLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - Ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((A * (1 - Ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * pi);
            wgLat = gcjLat - dLat;
            wgLon = gcjLon - dLon;
        }
        public static bool OutOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0
                    * pi)) * 2.0 / 3.0;
            return ret;
        }

        public static void WGS84ToBD09(double wgLat, double wgLon,out double bdLat,out double bdLon)
        {
            double gcjLat;
            double gcjLon;
            WGS84ToGCJ(wgLat, wgLon, out gcjLat, out gcjLon);
            GCJToBD09(gcjLat, gcjLon, out bdLat, out bdLon);
        }

        private const double EARTH_RADIUS = 6378.137;//地球半径
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 亮点间距离计算
        /// </summary>
        /// <param name="lat1">开始纬度</param>
        /// <param name="lng1">开始经度</param>
        /// <param name="lat2">结束纬度</param>
        /// <param name="lng2">结束经度</param>
        /// <returns>单位米</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 100000) / 100;
            return s;
        }

        /// <summary>
        /// 判断是否在区域内
        /// </summary>
        /// <param name="Lat">经度</param>
        /// <param name="Lng">纬度</param>
        /// <param name="Pts">区域点数组</param>
        /// <returns></returns>
        public static bool PolygonIsInZone(double Lat, double Lng, string Pts)
        {
            bool inZone = false;
            int j = 0;

            double x = Lng;
            double y = Lat;
            string[] Points = Pts.Split('|');

            for (int i = 0; i < Points.Length; i++)
            {
                j++;
                if (j == Points.Length) { j = 0; }

                string[] pti = Points[i].Split(',');
                string[] ptj = Points[j].Split(',');
                double ptiLat = System.Convert.ToDouble(pti[0]);
                double ptiLng = System.Convert.ToDouble(pti[1]);
                double ptjLat = System.Convert.ToDouble(ptj[0]);
                double ptjLng = System.Convert.ToDouble(ptj[1]);

                if (((ptiLat < y) && (ptjLat >= y)) || ((ptjLat < y) && (ptiLat >= y)))
                {
                    if (ptiLng + (y - ptiLat) / (ptjLat - ptiLat) * (ptjLng - ptiLng) < x)
                    {
                        inZone = !inZone;
                    }
                }

            }

            return inZone;

        }
    }
}
