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
    public partial class AddEquipment : Form
    {
        string loginId;
        public AddEquipment()
        {
            InitializeComponent();
            loginId = Main.loginId;
            Array arr= Enum.GetValues(typeof(Model.DeviceModel));
            List<info> model = new List<info>();
            foreach (var item in arr)
            {
                model.Add(new info() {Id=(int)(Model.DeviceModel)item,Name=item.ToString() });
            }
            DeviceModel.DataSource = model;
            DeviceModel.ValueMember = "Id";
            DeviceModel.DisplayMember = "Name";
        }

        public class info
        {
            public int Id{get;set;}
            public string Name { get; set; }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int type = int.Parse(DeviceModel.SelectedValue.ToString());
            string serialNum = txtSerialNum.Text.Trim();
            string bindNum = txtBindNum.Text.Trim();
            if (string.IsNullOrEmpty(serialNum))
            {
                MessageBox.Show("序列号不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(bindNum))
            {
                MessageBox.Show("绑定号不能为空！");
                return;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(serialNum.Trim(), bindNum.Trim());
            string strResult= Client.Get().ImportDevice(loginId, dic,type);
            JObject obj = JObject.Parse(strResult);
            if (obj["Code"].ToString()=="1")
            {
                if (obj["existNumList"].HasValues)
                {
                    txtLog.Text = obj["existNumList"][0]["NumList"].ToString()+"\r\n";
                    txtLog.AppendText("添加失败！");
                    return;
                }
                txtLog.Text = "添加成功！";
                return;
            }
            else
            {
                txtLog.Text = obj["Message"].ToString() + "\r\n";
                txtLog.AppendText("添加失败！");
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
