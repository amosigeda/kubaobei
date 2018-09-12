using Newtonsoft.Json;
using RestSharp.Serializers;

namespace YW.Utility
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            var jSetting = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            return JsonConvert.SerializeObject(obj, Formatting.Indented, jSetting);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; }
    }
}