using System.Collections.Generic;

namespace YW.Model.Entity
{
    public class Address
    {
        public List<string> Nearby = new List<string>();
        public string Province;
        public string City;
        public string District;
        public string Road;

        public int Code { get; set; }
    }
}