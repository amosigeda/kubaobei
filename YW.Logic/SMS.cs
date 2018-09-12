using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using YW.Data;

namespace YW.Logic
{
    public class SMS
    {
        private static SMS _object;
        private static readonly object LockHelper = new object();
        public static SMS GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new SMS();
                    }
                }
            }
            return _object;
        }

        public bool SendRegCheckNumber(string project, string phoneNumber, string content)
        {
            var pro = Logic.Project.GetInstance().Get(project);
            if (pro == null)
                return false;
            string sms = "#code#=" + content;
            sms = System.Web.HttpUtility.UrlEncode(sms);
            try
            {
                string url = "http://v.juhe.cn/sms/send?mobile=" + phoneNumber + "&tpl_id=" + pro.SMSReg + "&tpl_value=" + sms +
                             "&key=" + pro.SMSKey + "&dtype=xml";
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string strXml = null;
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    strXml = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
                response.Close();
                if (strXml != null)
                {
                    var xml = new XmlDocument();
                    xml.LoadXml(strXml);
                    XmlNode root = xml.SelectSingleNode("root");
                    if (root != null)
                    {
                        if (int.Parse(root.SelectSingleNode("error_code").InnerXml) == 0)
                        {
                            Count.GetInstance().SMS();
                            return true;
                        }
                        else
                        {
                            Logger.Info(strXml);
                        }
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }


        public bool SendForgotCheckNumber(string project,string phoneNumber, string content)
        {
            var pro = Logic.Project.GetInstance().Get(project);
            if (pro == null)
                return false;
            string sms = "#code#=" + content;
            sms = System.Web.HttpUtility.UrlEncode(sms);
            try
            {
                string url = "http://v.juhe.cn/sms/send?mobile=" + phoneNumber + "&tpl_id=" + pro.SMSForgot + "&tpl_value=" + sms +
                             "&key=" + pro.SMSKey + "&dtype=xml";
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                string strXml = null;
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    strXml = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                }
                response.Close();
                if (strXml != null)
                {
                    var xml = new XmlDocument();
                    xml.LoadXml(strXml);
                    XmlNode root = xml.SelectSingleNode("root");
                    if (root != null)
                    {
                        if (int.Parse(root.SelectSingleNode("error_code").InnerXml) == 0)
                        {
                            Count.GetInstance().SMS();
                            return true;
                        }
                        else
                        {
                            Logger.Error(strXml);
                        }
                    }
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
        
    }
}
