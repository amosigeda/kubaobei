using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using YW.Model.Entity;
using Newtonsoft.Json.Linq;
namespace YW.ImportDevice
{
    public partial class ImportEquipment : Form
    {
        string loginId;
        public ImportEquipment()
        {
            InitializeComponent();
            loginId = Main.loginId;

            List<info> model = new List<info>();
            Array arr = Enum.GetValues(typeof(Model.DeviceModel));
            foreach (var item in arr)
            {
                model.Add(new info() { Id = (int)(Model.DeviceModel)item, Name = item.ToString()});
            }

            ModelList.DataSource = model;
            ModelList.ValueMember = "Id";
            ModelList.DisplayMember = "Name";
        }
        public class info
        {
            public int Id{get;set;}
            public string Name { get; set; }
        }
        string txtPath = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "文本文件(*.txt)|*.txt";
            openFile.Title = "请选择要导入的设备文件";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFile.FileName;
                txtPath = openFile.FileName;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            int type = int.Parse(ModelList.SelectedValue.ToString());
            if (txtPath == "")
            {
                txtLog.Text = "文件路径不能为空！\r\n";
                return;
            }
            txtLog.Text = "开始导入···\r\n";
            string message = "";
            Dictionary<string, string> _dictionary = new Dictionary<string, string>();
            StreamReader sr = new StreamReader(txtPath);
            string line = sr.ReadLine();
            while (line != null)
            {
                try
                {
                    string[] str = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    _dictionary.Add(str[0].Trim(), str[1].Trim());
                    line = sr.ReadLine();
                }
                catch (Exception)
                {

                }
            }
            string existNumList = Client.Get().ImportDevice(loginId, _dictionary,type);
            JObject jobj = JObject.Parse(existNumList);
            if (jobj["Code"].ToString() == "1")
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "Exixt\\";
                string path = filePath + fileName;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                StreamWriter sw = new StreamWriter(path);
                JArray arr = JArray.Parse(jobj["existNumList"].ToString());
                foreach (var item in arr)
                {
                    sw.WriteLine(item["NumList"]);
                    txtLog.AppendText(item["NumList"].ToString() + "\r\n");
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();
                message = "设备导入完毕！\r\n";
            }
            else
            {
                message = jobj["Message"].ToString() + "\r\n";
            }
            txtLog.AppendText(message);
        }
    }
}
