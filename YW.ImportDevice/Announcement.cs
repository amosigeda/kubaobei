using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
namespace YW.ImportDevice
{
    public partial class Announcement : Form
    {
        string loginId;
        public Announcement()
        {
            InitializeComponent();
            loginId = Main.loginId;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string content = txtAnnouncement.Text.Trim();
            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("公告内容不能为空");
                return;
            }
            string rest= Client.Get().Announcement(loginId, content);
            JObject obj = JObject.Parse(rest);
            if (obj["Code"].ToString()=="1")
            {
                MessageBox.Show(obj["Message"].ToString());
                this.Close();
                this.Dispose();

            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
                this.Close();
                this.Dispose();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
