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
    public partial class ResetPassword : Form
    {
        JObject obj;
        string loginId; 
        public ResetPassword()
        {
            InitializeComponent();
            loginId = Main.loginId;
            string phoneNum= Main.phoneNum;
            string user= Client.Get().GetUserByPhone(loginId, phoneNum);
            obj= JObject.Parse(user);
            if (obj["Code"].ToString()=="1")
            {
                labUserId.Text = obj["UserID"].ToString();
                labPhone.Text = obj["PhoneNumber"].ToString();
                labName.Text=obj["Name"].ToString();
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
                this.Close();
                this.Dispose();
            }
        }

        private void btnSure_Click(object sender, EventArgs e)
        {
            string pwd=txtPassword.Text.Trim();
            if (string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            string strResult= Client.Get().UpdateUserPwd(loginId, Convert.ToInt32(obj["UserID"].ToString()), pwd);
            JObject result = JObject.Parse(strResult);
            if (result["Code"].ToString()=="1")
            {
                MessageBox.Show("修改成功！");
                this.Close();
                this.Dispose();
            }
            else
            {
                MessageBox.Show(result["Message"].ToString());
                return;
            }
            //Model.Entity.User user = new Model.Entity.User
            //{
            //    UserID=Convert.ToInt32(obj["UserID"]),
            //    PhoneNumber = obj["PhoneNumber"].ToString(),
            //    Password=pwd,
            //    LoginID = obj["LoginID"].ToString(),
            //    LoginType =Convert.ToInt32(obj["LoginType"]),
            //    UserType = Convert.ToInt32(obj["UserType"]),
            //    Name = obj["Name"].ToString(),
            //    Deleted =(obj["Deleted"].ToString()=="1"?true:false),
            //    Notification = obj["Notification"].ToString()=="1"?true:false,
            //    NotificationSound = obj["NotificationSound"].ToString()=="1"?true:false,
            //    NotificationVibration = obj["NotificationVibration"].ToString()=="1"?true:false,
            //    AppID = obj["AppID"].ToString(),
            //    CreateTime =DateTime.Parse(obj["CreateTime"].ToString())
            //};


        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
