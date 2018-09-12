using System.Reflection;

namespace YW.Model.Entity
{
    public class Weather
    {
        private string province;
        private string city;
        private string cityName;
        private string cityCode;
        private string weather;
        private string temp;
        private string wind;
        private string windGrade;
        private string pm25;
        private string humidity;

        public string Province
        {
            get => province;
            set => province = value;
        }

        public string City
        {
            get => city;
            set => city = value;
        }

        public string CityName
        {
            get => cityName;
            set => cityName = value;
        }

        public string CityCode
        {
            get => cityCode;
            set => cityCode = value;
        }

        public string WeatherCont
        {
            get => weather;
            set => weather = value;
        }

        public string Temp
        {
            get => temp;
            set => temp = value;
        }

        public string Wind
        {
            get => wind;
            set => wind = value;
        }

        public string WindGrade
        {
            get => windGrade;
            set => windGrade = value;
        }

        public string Pm25
        {
            get => pm25;
            set => pm25 = value;
        }

        public string Humidity
        {
            get => humidity;
            set => humidity = value;
        }

        public override string ToString()
        {
            PropertyInfo[] propertyInfoList = GetType().GetProperties();
            string result = "";
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                result += string.Format("{0}={1} ", propertyInfo.Name, propertyInfo.GetValue(this, null));
            }

            return result;
        }
    }
}