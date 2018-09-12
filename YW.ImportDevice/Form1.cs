using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using YW.Utility;
using YW.Model.Entity;
using Newtonsoft.Json.Linq;
namespace YW.ImportDevice
{
    public partial class Form1 : Form
    {
        string loginId;
        public Form1()
        {
            InitializeComponent();
            loginId = Index.loginId;
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
            if (txtPath == "")
            {
                return;
            }
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
            string existNumList = Client.Get().ImportDevice(loginId,_dictionary);
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
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();
                message="设备导入完毕！";
            }
            else
            {
               message= jobj["Message"].ToString();
            }
            lblMessage.Text = message;
        }
    }
}
