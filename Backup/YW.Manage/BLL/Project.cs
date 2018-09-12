using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace YW.Manage.BLL
{
    public class Project
    {
        private static Project _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<string, Models.Project> _dictionaryById;
        private readonly Dictionary<string, Models.Project> _dictionaryByServer;
        public static Project GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Project();
                    }
                }
            }
            return _object;
        }
        public Project()
        {
            _dictionaryById = new Dictionary<string, Models.Project>();
            _dictionaryByServer = new Dictionary<string, Models.Project>();
            StreamReader sr = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Server.txt"), Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] item = line.Split(',');
                var server = new Models.Project {Id = item[0], Name = item[2], Server = item[1]};
                _dictionaryById.Add(server.Id.ToLower(), server);
                _dictionaryByServer.Add(server.Server.ToLower(), server);
            }
            sr.Close();
            sr.Dispose();
        }

        public Models.Project GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            id = id.ToLower();
            if (_dictionaryById.ContainsKey(id))
                return _dictionaryById[id];
            else
                return null;
        }
        public Models.Project GetByServer(string server)
        {
            if (string.IsNullOrEmpty(server))
                return null;
            server = server.ToLower();
            if (_dictionaryByServer.ContainsKey(server))
                return _dictionaryByServer[server];
            else
                return null;
        }
       
    }
}