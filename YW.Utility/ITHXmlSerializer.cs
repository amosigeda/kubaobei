using System.IO;
using System.Text;
using RestSharp.Serializers;

namespace YW.Utility
{
    public class ITHXmlSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            string res = null;
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, obj);
                res = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            }
            return res;
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; }
    }
}