namespace YW.Model.Entity
{
    public class CloudPlatformInfo
    {
        private string _vccId;
        private string _key;
        private string _urlDial;
        private string _urlNumberAnalysis;

        public string VccId
        {
            get => _vccId;
            set => _vccId = value;
        }

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public string UrlDial
        {
            get => _urlDial;
            set => _urlDial = value;
        }

        public string UrlNumberAnalysis
        {
            get => _urlNumberAnalysis;
            set => _urlNumberAnalysis = value;
        }
    }
}