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
    public partial class EditDevice : Form
    {
        string loginId;
        int deviceId;
        public EditDevice()
        {
            InitializeComponent();
            deviceId = Main.deviceId;
            loginId = Main.loginId;
            string device = Client.Get().GetDeviceDetail(loginId, deviceId);
            JObject obj = JObject.Parse(device);
            if (obj["Code"].ToString() == "1")
            {
                txtBindNum.Text = obj["BindNumber"].ToString();
                txtDeviceId.Text = obj["DeviceID"].ToString();
                txtSerialNum.Text = obj["SerialNumber"].ToString();
                txtUserId.Text = obj["UserId"].ToString();
                txtSmsBalanceKey.Text = obj["SmsBalanceKey"].ToString();
                txtSmsFlowKey.Text = obj["SmsFlowKey"].ToString();
                txtSmsNumber.Text = obj["SmsNumber"].ToString();
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
            string smsNumber = txtSmsNumber.Text.Trim();
            string smsBalanceKey = txtSmsBalanceKey.Text.Trim();
            string smsFlowKey = txtSmsFlowKey.Text.Trim();
            if (string.IsNullOrEmpty(smsBalanceKey))
            {
                MessageBox.Show("查询话费指令不能为空");
                return;
            }
            if (string.IsNullOrEmpty(smsFlowKey))
            {
                MessageBox.Show("查询流量指令不能为空");
                return;
            }
            if (string.IsNullOrEmpty(smsNumber))
            {
                MessageBox.Show("运营商号码不能为空");
                return;
            }
            string str= Client.Get().UpdateSmsOrder(loginId, deviceId, smsNumber, smsBalanceKey, smsFlowKey);
            JObject obj = JObject.Parse(str);
            if (obj["Code"].ToString()=="1")
            {
                MessageBox.Show("修改成功");
                return;
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
