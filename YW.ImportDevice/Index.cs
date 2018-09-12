using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.ServiceModel.Configuration;
namespace YW.ImportDevice
{
    public partial class Index : Form
    {
        public Index()
        {
            InitializeComponent();
            this.skinEngine1.SkinFile = "MP10.ssk";
            List<IpInfo> info = new List<IpInfo>()
            {
                new IpInfo(){name="兴韵星",Ip="http://120.24.180.38:6699/IClient"},
                new IpInfo(){name="咪咕",Ip="http://120.24.172.44:6699/IClient"},
                new IpInfo(){name="关爱通",Ip="http://112.74.130.160:6699/IClient"}
            };
            combox.DataSource = info;
            combox.DisplayMember = "name";
            combox.ValueMember = "Ip";
            string[] username= ConfigurationManager.AppSettings.GetValues("UserName");
            string[] password = ConfigurationManager.AppSettings.GetValues("Password");
            if (username!=null)
            {
                txtLogin.Text = username[0];
            }
            if (password!=null)
            {
                txtPwd.Text = password[0];
            }
        }
        public class IpInfo
        {
            public string name { get; set; }
            public string Ip { get; set; }
        }
        public static string loginId;
        private void btnLogin_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSectionGroup sct = config.SectionGroups["system.serviceModel"];
            ServiceModelSectionGroup serviceModelSectionGroup = sct as ServiceModelSectionGroup;
            ClientSection clientsection = serviceModelSectionGroup.Client;
            foreach (ChannelEndpointElement item in clientsection.Endpoints)
            {
                item.Address = new Uri(combox.SelectedValue.ToString());
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("system.serviceModel");
            string name = txtLogin.Text.Trim();
            string password = txtPwd.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("用户名不能为空");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("密码不能为空");
                return;
            }

            #region 将用户名，密码写入配置文件
            if (checkRember.Checked)
            {
                Configuration con = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (con.AppSettings.Settings["UserName"]==null)
                {
                    con.AppSettings.Settings.Add("UserName", name); 
                }
                else
                {
                    con.AppSettings.Settings["UserName"].Value = name;
                }
                if (con.AppSettings.Settings["Password"]==null)
                {
                    con.AppSettings.Settings.Add("Password", password); 
                }
                else
                {
                    con.AppSettings.Settings["Password"].Value = password;
                }
                con.Save(ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings"); 
            }
            #endregion


            string login = Client.Get().Login(3, name, password, null, "YW.Client");
            JObject obj = JObject.Parse(login);
            if ((string)obj["Code"] == "1")
            {
                if (obj["UserType"].ToString() != "2")
                {
                    MessageBox.Show("非管理员账号，登录失败！");
                    return;
                }
                loginId = (string)obj["LoginId"];
                Main main = new Main();
                if (main.IsDisposed)
                {
                    return;
                }
                main.Owner = this;
                this.Hide();
                main.ShowDialog();
                Application.ExitThread();
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
            }
        }

        private void btnRest_Click(object sender, EventArgs e)
        {
            txtLogin.Text = "";
            txtPwd.Text = "";
        }
    }
}
