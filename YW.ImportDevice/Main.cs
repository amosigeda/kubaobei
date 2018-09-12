using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace YW.ImportDevice
{
    public partial class Main : Form
    {
        bool bind = true;
        public static string loginId;// = "4985274a-cb05-44bb-bee4-06820e50838e";
        string userOrDevice = "user";
        DataTable table = null;
        public static string phoneNum;
        public static int deviceId;

        public Main()
        {
            InitializeComponent();
            planePage.Location = new Point((planePage.Parent.Width-336)/2,0);
            //sc2.Panel1.ClientSize = new System.Drawing.Size(sc2.Width, 150);
            loginId = Index.loginId;
            if (string.IsNullOrEmpty(loginId))
            {
                MessageBox.Show("登录ID异常");
                this.Close();
                this.Dispose();
            }
            AddSearchCondition(userOrDevice);
            ShowUserInfo(loginId);
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Name=="菜单")
            {
                e.Node.ExpandAll();
            }
            if (e.Node.Name == "查看用户")
            {
                currentPage = 1;
                userOrDevice = "user";
                dataGV.Columns.Clear();
                gbSelect.Controls.Clear();
                AddSearchCondition("user");
                ShowUserInfo(loginId);

            }
            if (e.Node.Name == "导入设备")
            {
                ImportEquipment ie = new ImportEquipment();
                ie.ShowDialog();

            }
            if (e.Node.Name == "查看设备")
            {
                currentPage = 1;
                userOrDevice = "device";
                dataGV.Columns.Clear();
                gbSelect.Controls.Clear();
                AddSearchCondition("device");
                ShowDeivceInfo(loginId);
            }
            if (e.Node.Name=="添加设备")
            {
                AddEquipment ae = new AddEquipment();
                ae.ShowDialog();
            }
            if (e.Node.Name=="发布公告")
            {
                Announcement ann = new Announcement();
                ann.ShowDialog();
            }
        }

        void btnSelect_Click(object sender, EventArgs e)
        {
            TextBox txtPhone = gbSelect.Controls.Find("txtPhone", false)[0] as TextBox;
            if (txtPhone == null)
            {
                MessageBox.Show("Error!");
            }
            string phoneNum = txtPhone.Text.Trim();
            if (string.IsNullOrEmpty(phoneNum))
            {
                return;
            }
            dataGV.Columns.Clear();
            string userInfo = Client.Get().GetUserByPhone(loginId, phoneNum);
            JObject obj = JObject.Parse(userInfo);
            if (obj["Code"].ToString() == "1")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("用户ID");
                dt.Columns.Add("电话号码");
                dt.Columns.Add("用户名称");
                dt.Columns.Add("AppId");
                dt.Columns.Add("创建时间");
                dt.Columns.Add("更新时间");
                DataRow dr = dt.NewRow();
                dr["用户ID"] = obj["UserID"].ToString();
                dr["电话号码"] = obj["PhoneNumber"].ToString();
                dr["用户名称"] = obj["Name"].ToString();
                dr["AppId"] = obj["AppID"].ToString();
                dr["创建时间"] = obj["CreateTime"].ToString();
                dr["更新时间"] = obj["UpdateTime"].ToString();
                dt.Rows.Add(dr);
                dataGV.DataSource = dt;
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
            }
        }

        private void ShowUserInfo(string loginId)
        {
            string str = Client.Get().GetUserList(loginId);
            JObject obj = JObject.Parse(str);
            if (obj["Code"].ToString() == "1")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("用户ID");
                dt.Columns.Add("电话号码");
                dt.Columns.Add("用户名称");
                dt.Columns.Add("AppId");
                dt.Columns.Add("创建时间");
                dt.Columns.Add("更新时间");
                for (int i = 0; i < obj["UserList"].ToArray().Length; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["用户ID"] = obj["UserList"][i]["UserID"].ToString();
                    dr["电话号码"] = obj["UserList"][i]["PhoneNumber"].ToString();
                    dr["用户名称"] = obj["UserList"][i]["Name"].ToString();
                    dr["AppId"] = obj["UserList"][i]["AppID"].ToString();
                    dr["创建时间"] = obj["UserList"][i]["CreateTime"].ToString();
                    dr["更新时间"] = obj["UserList"][i]["UpdateTime"].ToString();
                    dt.Rows.Add(dr);
                }
                table = dt;
                dataGV.DataSource = BindData(dt);
                AddDelEditBtn(dataGV, "user");
                SetColumWidth();
            }

        }
        /// <summary>
        /// 增加删除和编辑/重置密码按钮
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="type"></param>
        public void AddDelEditBtn(DataGridView dgv, string type)
        {
            var delete = new DataGridViewLinkColumn
            {
                HeaderText = "删除",
                Text = "删除",
                UseColumnTextForLinkValue = true,
                Name = "delete"
            };
            dataGV.Columns.Insert(dataGV.Columns.Count, delete);

            if (type=="device")
            {
                var edit = new DataGridViewLinkColumn
                {
                    HeaderText = "编辑",
                    Text = "编辑",
                    UseColumnTextForLinkValue = true,
                    Name = "edit"
                };
                dataGV.Columns.Insert(dataGV.Columns.Count, edit);

                var reset = new DataGridViewLinkColumn
                {
                    HeaderText = "重置",
                    Text = "重置",
                    UseColumnTextForLinkValue = true,
                    Name = "reset"
                };
                dataGV.Columns.Insert(dataGV.Columns.Count, reset);

            }
            if (type=="user")
            {
                var resetPwd = new DataGridViewLinkColumn
                {
                    HeaderText = "重置密码",
                    Text = "重置密码",
                    Name = "resetPwd",
                    UseColumnTextForLinkValue = true
                };
                dataGV.Columns.Insert(dataGV.Columns.Count, resetPwd);
            }
            if (bind)
            {
                dgv.CellClick += new DataGridViewCellEventHandler(dgv_CellClick);
                bind = false;
            } 
        }

        void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            string strAction = dgv.Columns[e.ColumnIndex].Name;
            switch (strAction)
            {
                case "delete":
                    if (MessageBox.Show("确定删除吗？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        if (userOrDevice == "user")
                        {
                            int userId = Convert.ToInt32(dgv["用户ID", e.RowIndex].Value);
                            string strResult = Client.Get().DeleteByUserId(loginId, userId);
                            JObject obj = JObject.Parse(strResult);
                            if (obj["Code"].ToString() == "1")
                            {
                                MessageBox.Show("删除成功");
                                dgv.Rows.RemoveAt(dgv.CurrentRow.Index);
                            }
                            else
                            {
                                MessageBox.Show(obj["Message"].ToString());
                            }
                        }

                        if (userOrDevice == "device")
                        {
                            int deviceId = Convert.ToInt32(dgv[0,e.RowIndex].Value);
                            string strResult = Client.Get().DeleteDeviceById(loginId,deviceId);
                            JObject obj = JObject.Parse(strResult);
                            if (obj["Code"].ToString() == "1")
                            {
                                MessageBox.Show("删除成功");
                                dgv.Rows.RemoveAt(dgv.CurrentRow.Index);
                            }
                            else
                            {
                                MessageBox.Show(obj["Message"].ToString());
                            }
                        }
                    }
                    break;
                case "reset":
                    if (MessageBox.Show("确定重置吗？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {


                        if (userOrDevice == "device")
                        {
                            int deviceId = Convert.ToInt32(dgv[0, e.RowIndex].Value);
                            string strResult = Client.Get().ResetDeviceById(loginId, deviceId);
                            JObject obj = JObject.Parse(strResult);
                            if (obj["Code"].ToString() == "1")
                            {
                                MessageBox.Show("重置成功");
                                dgv.Rows.RemoveAt(dgv.CurrentRow.Index);
                            }
                            else
                            {
                                MessageBox.Show(obj["Message"].ToString());
                            }
                        }
                    }
                    break;
                case "edit":
                    if (userOrDevice == "user")
                    {
                        MessageBox.Show("UserEdit");
                    }
                    if (userOrDevice == "device")
                    {
                        deviceId = Convert.ToInt32(dgv[0, e.RowIndex].Value);
                        EditDevice ed = new EditDevice();

                        if (ed.IsDisposed)
                        {
                            return;
                        }
                        ed.ShowDialog();
                    }
                    break;
                case "resetPwd":
                    if (userOrDevice == "user")
                    {
                        phoneNum = dgv["电话号码", e.RowIndex].Value.ToString();
                        ResetPassword rp = new ResetPassword();
                        if (rp.IsDisposed)
                        {
                            return;
                        }
                        rp.ShowDialog();
                    }
                    if (userOrDevice == "device")
                    {
                        MessageBox.Show("deviceReset");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 添加查询条件
        /// </summary>
        public void AddSearchCondition(string type)
        {
            #region 添加用户查询条件
            if (type == "user")
            {
                if (gbSelect.Controls != null)
                {
                    gbSelect.Controls.Clear();
                }
                Label lblPhoneNum = new Label();
                lblPhoneNum.Text = "手机号：";
                lblPhoneNum.TextAlign = ContentAlignment.MiddleCenter;
                lblPhoneNum.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                lblPhoneNum.Height = 21;
                lblPhoneNum.Width = 60;
                lblPhoneNum.Location = new Point(50, 25);
                gbSelect.Controls.Add(lblPhoneNum);

                TextBox txtPhone = new TextBox();
                txtPhone.Name = "txtPhone";
                txtPhone.TextAlign = HorizontalAlignment.Left;
                txtPhone.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                txtPhone.Size = new Size(100, 21);
                txtPhone.Location = new Point(120, 25);
                gbSelect.Controls.Add(txtPhone);

                Button btnSelect = new Button();
                btnSelect.Name = "btnSelect";
                btnSelect.Text = "查询";
                btnSelect.TextAlign = ContentAlignment.MiddleCenter;
                btnSelect.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                btnSelect.Size = new Size(75, 23);
                btnSelect.Location = new Point(230, 25);
                btnSelect.Click += new EventHandler(btnSelect_Click);
                gbSelect.Controls.Add(btnSelect);
            }
            #endregion

            #region 添加设备查询条件
            if (type == "device")
            {
                if (gbSelect.Controls != null)
                {
                    gbSelect.Controls.Clear();
                }
                Label lblSerialNum = new Label();
                lblSerialNum.Text = "序列号：";
                lblSerialNum.TextAlign = ContentAlignment.MiddleCenter;
                lblSerialNum.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                lblSerialNum.Height = 21;
                lblSerialNum.Width = 60;
                lblSerialNum.Location = new Point(50, 25);
                gbSelect.Controls.Add(lblSerialNum);

                TextBox txtSerialNum = new TextBox();
                txtSerialNum.Name = "txtSerial";
                txtSerialNum.TextAlign = HorizontalAlignment.Left;
                txtSerialNum.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                txtSerialNum.Size = new Size(100, 21);
                txtSerialNum.Location = new Point(110, 25);
                gbSelect.Controls.Add(txtSerialNum);

                Button btnSerialNum = new Button();
                btnSerialNum.Name = "btnSerial";
                btnSerialNum.Text = "查询";
                btnSerialNum.TextAlign = ContentAlignment.MiddleCenter;
                btnSerialNum.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                btnSerialNum.Size = new Size(75, 23);
                btnSerialNum.Location = new Point(230, 25);
                btnSerialNum.Click += new EventHandler(btnSerialNum_Click);
                gbSelect.Controls.Add(btnSerialNum);
            }
            #endregion
        }

        void btnSerialNum_Click(object sender, EventArgs e)
        {
            TextBox txtSerialNum = gbSelect.Controls.Find("txtSerial", false)[0] as TextBox;
            if (txtSerialNum == null)
            {
                MessageBox.Show("Error!");
                return;
            }
            string serialNum = txtSerialNum.Text.Trim();
            if (string.IsNullOrEmpty(serialNum))
            {
                return;
            }
            dataGV.Columns.Clear();
            string strResult = Client.Get().GetDeviceBySerialNum(loginId, serialNum);
            JObject obj = JObject.Parse(strResult);
            DataTable dt = new DataTable();
            dt.Columns.Add("设备ID");
            dt.Columns.Add("序列号");
            dt.Columns.Add("设备类型");
            dt.Columns.Add("绑定号");
            dt.Columns.Add("设备电话号码");
            dt.Columns.Add("设备激活时间");
            dt.Columns.Add("设备到期时间");
            dt.Columns.Add("设备活动时间");
            if (obj["Code"].ToString() == "1")
            {
                DataRow dr = dt.NewRow();
                dr["设备ID"] = obj["DeviceID"].ToString();
                dr["序列号"] = obj["SerialNumber"].ToString();
                dr["设备类型"] = obj["DeviceModelID"].ToString();
                dr["绑定号"] = obj["BindNumber"].ToString();
                dr["设备电话号码"] = obj["PhoneNumber"].ToString();
                dr["设备激活时间"] = obj["HireStartDate"].ToString();
                dr["设备到期时间"] = obj["HireExpireDate"].ToString();
                dr["设备活动时间"] = obj["ActiveDate"].ToString();
                dt.Rows.Add(dr);
                dataGV.DataSource = dt;
                AddDelEditBtn(dataGV,userOrDevice);
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
            }
        }

        private void ShowDeivceInfo(string loginId)
        {
            string deviceInfo = Client.Get().GetAllDevice(loginId);
            JObject obj = JObject.Parse(deviceInfo);
            if (obj["Code"].ToString() == "1")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("设备ID");
                dt.Columns.Add("序列号");
                dt.Columns.Add("绑定号");
                dt.Columns.Add("设备类型");
                dt.Columns.Add("设备电话号码");
                dt.Columns.Add("设备激活时间");
                dt.Columns.Add("设备到期时间");
                dt.Columns.Add("设备活动时间");
                
                foreach (var item in obj["DeviceList"])
                {
                    DataRow dr = dt.NewRow();
                    dr["设备ID"] = item["DeviceID"].ToString();
                    dr["序列号"] = item["SerialNumber"].ToString();
                    dr["设备类型"] = item["DeviceModelID"].ToString();
                    dr["绑定号"] = item["BindNumber"].ToString();
                    dr["设备电话号码"] = item["PhoneNumber"].ToString();
                    dr["设备激活时间"] = item["HireStartDate"].ToString();
                    dr["设备到期时间"] = item["HireExpireDate"].ToString();
                    dr["设备活动时间"] = item["ActiveDate"].ToString();
                    dt.Rows.Add(dr);
                }
                table = dt;
                dataGV.DataSource = BindData(dt);
                AddDelEditBtn(dataGV, "device");
                SetColumWidth();
            }
            else
            {
                MessageBox.Show(obj["Message"].ToString());
            }
        }

        //调整列表最小列宽
        private void SetColumWidth()
        {
            if (userOrDevice=="user")
            {
                dataGV.Columns[0].MinimumWidth = 70;
                dataGV.Columns[1].MinimumWidth = 80;
                dataGV.Columns[2].MinimumWidth = 80;
                dataGV.Columns[3].MinimumWidth = 60;
                dataGV.Columns[4].MinimumWidth = 80;
                dataGV.Columns[5].MinimumWidth = 80;
                dataGV.Columns[6].MinimumWidth = 50;
                dataGV.Columns[7].MinimumWidth = 50;
            }
            if (userOrDevice=="device")
            {
                dataGV.Columns[0].MinimumWidth = 70;
                dataGV.Columns[1].MinimumWidth = 70;
                dataGV.Columns[2].MinimumWidth = 70;
                dataGV.Columns[3].MinimumWidth = 80;
                dataGV.Columns[4].MinimumWidth = 110;
                dataGV.Columns[5].MinimumWidth = 110;
                dataGV.Columns[6].MinimumWidth = 110;
                dataGV.Columns[7].MinimumWidth = 110;
                dataGV.Columns[8].MinimumWidth = 50;
                dataGV.Columns[9].MinimumWidth = 50;
                dataGV.Columns[10].MinimumWidth = 50;
            }
        }

        #region 分页功能
        int currentPage = 1;
        int pageSize = 10;
        int totalPage = 0;
        int totalRow = 0;
        int startRow = 0;
        int endRow = 0;
        public void InitDataSet(DataTable dt)
        {
            totalRow = dt.Rows.Count;
            if (totalRow <= 0)
            {
                return;
            }
            if (totalRow % pageSize == 0)
            {
                totalPage = totalRow / pageSize;
            }
            else
            {
                totalPage = totalRow / pageSize + 1;
            }
            if (currentPage <= 1)
            {
                currentPage = 1;
            }
            if (currentPage >= totalPage)
            {
                currentPage = totalPage;
            }
            startRow = (currentPage - 1) * pageSize;
            if (currentPage == totalPage)
            {
                endRow = totalRow;
            }
            else
            {
                endRow = currentPage * pageSize;
            }

        }

        public DataTable BindData(DataTable dt)
        {

            InitDataSet(dt);
            if (totalPage <= 1)
            {
                llFirstPage.Enabled = false;
                llPreviousPage.Enabled = false;
                llNextPage.Enabled = false;
                llLastPage.Enabled = false;
                txtGoPage.Enabled = false;
                llGoPage.Enabled = false;
            }
            else
            {
                llFirstPage.Enabled = true;
                llPreviousPage.Enabled = true;
                llNextPage.Enabled = true;
                llLastPage.Enabled = true;
                txtGoPage.Enabled = true;
                llGoPage.Enabled = true;
            }
            DataTable temp = dt.Clone();
            for (int i = startRow; i < endRow; i++)
            {
                temp.ImportRow(dt.Rows[i]);
            }
            return temp;
        }

        private void llFirstPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (table == null)
            {
                return;
            }
            currentPage = 1;
            dataGV.Columns.Clear();
            dataGV.DataSource = BindData(table);
            AddDelEditBtn(dataGV, userOrDevice);
            SetColumWidth();
            llFirstPage.Enabled = false;
            llPreviousPage.Enabled = false;
            llNextPage.Enabled = true;
            llLastPage.Enabled = true;
        }

        private void llPreviousPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (table == null)
            {
                return;
            }
            currentPage = currentPage - 1;
            dataGV.Columns.Clear();
            dataGV.DataSource = BindData(table);
            AddDelEditBtn(dataGV, userOrDevice);
            SetColumWidth();
            if (currentPage <= 1)
            {
                llFirstPage.Enabled = false;
                llPreviousPage.Enabled = false;
                llNextPage.Enabled = true;
                llLastPage.Enabled = true;
            }
        }

        private void llNextPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (table == null)
            {
                return;
            }
            currentPage = currentPage + 1;
            dataGV.Columns.Clear();
            dataGV.DataSource = BindData(table);
            AddDelEditBtn(dataGV, userOrDevice);
            SetColumWidth();
            if (currentPage >= totalPage)
            {
                llFirstPage.Enabled = true;
                llPreviousPage.Enabled = true;
                llNextPage.Enabled = false;
                llLastPage.Enabled = false;
            }
        }

        private void llLastPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (table == null)
            {
                return;
            }
            currentPage = totalPage;
            dataGV.Columns.Clear();
            dataGV.DataSource = BindData(table);
            AddDelEditBtn(dataGV, userOrDevice);
            SetColumWidth();
            llFirstPage.Enabled = true;
            llPreviousPage.Enabled = true;
            llNextPage.Enabled = false;
            llLastPage.Enabled = false;
        }

        private void llGoPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (table == null)
            {
                return;
            }
            string value = txtGoPage.Text.Trim();
            if (string.IsNullOrEmpty(value))
            {
                MessageBox.Show("请输入跳转页");
                return;
            }
            int result;
            if (!int.TryParse(value, out result))
            {
                MessageBox.Show("请输入正整数");
                return;
            }
            if (result <= 0 || result > totalPage)
            {
                MessageBox.Show("输入页数超出范围");
                return;
            }

            currentPage = result;
            dataGV.Columns.Clear();
            dataGV.DataSource = BindData(table);
            AddDelEditBtn(dataGV, userOrDevice);
            SetColumWidth();
            if (currentPage == 1)
            {
                llFirstPage.Enabled = false;
                llPreviousPage.Enabled = false;
                llNextPage.Enabled = true;
                llLastPage.Enabled = true;
            }
            if (currentPage == totalPage)
            {
                llFirstPage.Enabled = true;
                llPreviousPage.Enabled = true;
                llNextPage.Enabled = false;
                llLastPage.Enabled = false;
            }
        } 
        #endregion

    }
}
