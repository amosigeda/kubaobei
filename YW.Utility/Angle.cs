using System;

namespace YW.Utility
{
    public class Angle
    {
        static double Rc = 6378137;  // 赤道半径
        static double Rj = 6356725;  // 极半径 
        public static string GetStr(double angle)
        {
            string angleStr;
            //angle = (angle + 90) % 360;
            if (angle <= 22.5 || angle > 337.5)
            {
                angleStr = "正北";
            }
            else if (angle > 22.5 && angle <= 67.5)
            {
                angleStr = "东北";
            }
            else if (angle > 67.5 && angle <= 112.5)
            {
                angleStr = "正东";
            }
            else if (angle > 112.5 && angle <= 157.5)
            {
                angleStr = "东南";
            }
            else if (angle > 157.5 && angle <= 202.5)
            {
                angleStr = "正南";
            }
            else if (angle > 202.5 && angle <= 247.5)
            {
                angleStr = "西南";
            }
            else if (angle > 247.5 && angle <= 292.5)
            {
                angleStr = "正西";
            }
            else //if (angle > 292.5 && angle <= 337.5)
            {
                angleStr = "西北";
            }
            return angleStr;
        }
        /// <summary>
        /// 两点的方向，0度正北
        /// </summary>
        /// <param name="originLng">原点经度</param>
        /// <param name="originLat">原点纬度</param>
        /// <param name="lng">目标点经度</param>
        /// <param name="lat">目标点纬度</param>
        /// <returns></returns>
        public static string GetStr(double originLng, double originLat,double lng, double lat)
        {
            string angleStr = "";
            double angle;
            angle = Get(originLng, originLat, lng, lat);
            if (angle <= 22.5 || angle > 337.5)
            {
                angleStr = "正北";
            }
            else if (angle > 22.5 && angle <= 67.5)
            {
                angleStr = "东北";
            }
            else if (angle > 67.5 && angle <= 112.5)
            {
                angleStr = "正东";
            }
            else if (angle > 112.5 && angle <= 157.5)
            {
                angleStr = "东南";
            }
            else if (angle > 157.5 && angle <= 202.5)
            {
                angleStr = "正南";
            }
            else if (angle > 202.5 && angle <= 247.5)
            {
                angleStr = "西南";
            }
            else if (angle > 247.5 && angle <= 292.5)
            {
                angleStr = "正西";
            }
            else //if (angle > 292.5 && angle <= 337.5)
            {
                angleStr = "西北";
            }
            return angleStr;
        }
        /// <summary>
        /// 两点的方向，0度正北
        /// </summary>
        /// <param name="originLng">原点经度</param>
        /// <param name="originLat">原点纬度</param>
        /// <param name="lng">目标点经度</param>
        /// <param name="Lat">目标点纬度</param>
        /// <returns></returns>
        public static double Get(double originLng, double originLat,double lng, double lat)
        {
            double Am_RadLo = originLng * Math.PI / 180;
            double Am_RadLa = originLat * Math.PI / 180;
            double Bm_RadLo = lng * Math.PI / 180;
            double Bm_RadLa = lat * Math.PI / 180;
            double AEc = Rj + (Rc - Rj) * (90 - originLat) / 90;
            double AEd = AEc * Math.Cos(Am_RadLa);
            double dx = (Bm_RadLo - Am_RadLo) * AEd;
            double dy = (Bm_RadLa - Am_RadLa) * AEc;
            double angle = 0.0;
            angle = Math.Atan(Math.Abs(dx / dy)) * 180 / Math.PI;
             //判断象限
            double x = lng - originLng;
            double y = lat - originLat;
            if (x == 0 && y == 0)
            {
                angle = 0;
            }
            else if (x >= 0 && y >= 0) //第一象限
            {
                angle = 90 - angle;
            }
            else if (x < 0 && y >= 0) //第二象限
            {
                angle = angle + 90; 
            }
            else if (x <= 0 && y <= 0)//第三象限
            {
                angle = (90 - angle) + 180;
            }
            else if (x > 0 && y < 0) //第四象限
            {
                angle = angle + 270;
            }
            angle = 360 - ((angle + 270) % 360);
            return angle;
        }

    }

}
