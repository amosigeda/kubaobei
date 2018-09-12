namespace YW.ImportDevice
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("查看用户");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("人员管理", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("导入设备");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("查看设备");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("添加设备");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("设备管理", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5});
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("发布公告");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("菜单", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode6,
            treeNode7});
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.sc1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.sc2 = new System.Windows.Forms.SplitContainer();
            this.gbSelect = new System.Windows.Forms.GroupBox();
            this.sc3 = new System.Windows.Forms.SplitContainer();
            this.dataGV = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.planePage = new System.Windows.Forms.Panel();
            this.llPreviousPage = new System.Windows.Forms.LinkLabel();
            this.llGoPage = new System.Windows.Forms.LinkLabel();
            this.llFirstPage = new System.Windows.Forms.LinkLabel();
            this.txtGoPage = new System.Windows.Forms.TextBox();
            this.llNextPage = new System.Windows.Forms.LinkLabel();
            this.llLastPage = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.sc1)).BeginInit();
            this.sc1.Panel1.SuspendLayout();
            this.sc1.Panel2.SuspendLayout();
            this.sc1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sc2)).BeginInit();
            this.sc2.Panel1.SuspendLayout();
            this.sc2.Panel2.SuspendLayout();
            this.sc2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sc3)).BeginInit();
            this.sc3.Panel1.SuspendLayout();
            this.sc3.Panel2.SuspendLayout();
            this.sc3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGV)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.planePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // sc1
            // 
            this.sc1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sc1.Location = new System.Drawing.Point(0, 0);
            this.sc1.Margin = new System.Windows.Forms.Padding(0);
            this.sc1.Name = "sc1";
            // 
            // sc1.Panel1
            // 
            this.sc1.Panel1.Controls.Add(this.treeView1);
            // 
            // sc1.Panel2
            // 
            this.sc1.Panel2.Controls.Add(this.sc2);
            this.sc1.Size = new System.Drawing.Size(784, 461);
            this.sc1.SplitterDistance = 137;
            this.sc1.TabIndex = 0;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeView1.ItemHeight = 30;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            treeNode1.Name = "查看用户";
            treeNode1.Text = "查看用户";
            treeNode2.Name = "人员管理";
            treeNode2.Text = "人员管理";
            treeNode3.Name = "导入设备";
            treeNode3.Text = "导入设备";
            treeNode4.Name = "查看设备";
            treeNode4.Text = "查看设备";
            treeNode5.Name = "添加设备";
            treeNode5.Text = "添加设备";
            treeNode6.Name = "设备管理";
            treeNode6.Text = "设备管理";
            treeNode7.Name = "发布公告";
            treeNode7.Text = "发布公告";
            treeNode8.Name = "菜单";
            treeNode8.NodeFont = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            treeNode8.Text = "菜单";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8});
            this.treeView1.Size = new System.Drawing.Size(137, 461);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // sc2
            // 
            this.sc2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sc2.Location = new System.Drawing.Point(0, 0);
            this.sc2.Name = "sc2";
            this.sc2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sc2.Panel1
            // 
            this.sc2.Panel1.AutoScroll = true;
            this.sc2.Panel1.Controls.Add(this.gbSelect);
            // 
            // sc2.Panel2
            // 
            this.sc2.Panel2.Controls.Add(this.sc3);
            this.sc2.Size = new System.Drawing.Size(643, 461);
            this.sc2.SplitterDistance = 70;
            this.sc2.TabIndex = 0;
            // 
            // gbSelect
            // 
            this.gbSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSelect.Location = new System.Drawing.Point(0, 0);
            this.gbSelect.Name = "gbSelect";
            this.gbSelect.Size = new System.Drawing.Size(643, 70);
            this.gbSelect.TabIndex = 0;
            this.gbSelect.TabStop = false;
            this.gbSelect.Text = "查询条件";
            // 
            // sc3
            // 
            this.sc3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sc3.Location = new System.Drawing.Point(0, 0);
            this.sc3.Name = "sc3";
            this.sc3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sc3.Panel1
            // 
            this.sc3.Panel1.Controls.Add(this.dataGV);
            // 
            // sc3.Panel2
            // 
            this.sc3.Panel2.Controls.Add(this.groupBox1);
            this.sc3.Size = new System.Drawing.Size(643, 387);
            this.sc3.SplitterDistance = 321;
            this.sc3.TabIndex = 2;
            // 
            // dataGV
            // 
            this.dataGV.AllowUserToAddRows = false;
            this.dataGV.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.dataGV.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGV.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGV.Location = new System.Drawing.Point(0, 0);
            this.dataGV.Name = "dataGV";
            this.dataGV.ReadOnly = true;
            this.dataGV.RowHeadersVisible = false;
            this.dataGV.RowTemplate.Height = 23;
            this.dataGV.Size = new System.Drawing.Size(643, 321);
            this.dataGV.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.planePage);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(643, 62);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // planePage
            // 
            this.planePage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.planePage.Controls.Add(this.llPreviousPage);
            this.planePage.Controls.Add(this.llGoPage);
            this.planePage.Controls.Add(this.llFirstPage);
            this.planePage.Controls.Add(this.txtGoPage);
            this.planePage.Controls.Add(this.llNextPage);
            this.planePage.Controls.Add(this.llLastPage);
            this.planePage.Location = new System.Drawing.Point(130, 1);
            this.planePage.Name = "planePage";
            this.planePage.Size = new System.Drawing.Size(336, 60);
            this.planePage.TabIndex = 6;
            // 
            // llPreviousPage
            // 
            this.llPreviousPage.AutoSize = true;
            this.llPreviousPage.Location = new System.Drawing.Point(50, 25);
            this.llPreviousPage.Name = "llPreviousPage";
            this.llPreviousPage.Size = new System.Drawing.Size(41, 12);
            this.llPreviousPage.TabIndex = 1;
            this.llPreviousPage.TabStop = true;
            this.llPreviousPage.Text = "上一页";
            this.llPreviousPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llPreviousPage_LinkClicked);
            // 
            // llGoPage
            // 
            this.llGoPage.AutoSize = true;
            this.llGoPage.Location = new System.Drawing.Point(307, 25);
            this.llGoPage.Name = "llGoPage";
            this.llGoPage.Size = new System.Drawing.Size(29, 12);
            this.llGoPage.TabIndex = 5;
            this.llGoPage.TabStop = true;
            this.llGoPage.Text = "跳转";
            this.llGoPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llGoPage_LinkClicked);
            // 
            // llFirstPage
            // 
            this.llFirstPage.AutoSize = true;
            this.llFirstPage.Location = new System.Drawing.Point(0, 25);
            this.llFirstPage.Name = "llFirstPage";
            this.llFirstPage.Size = new System.Drawing.Size(29, 12);
            this.llFirstPage.TabIndex = 0;
            this.llFirstPage.TabStop = true;
            this.llFirstPage.Text = "首页";
            this.llFirstPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llFirstPage_LinkClicked);
            // 
            // txtGoPage
            // 
            this.txtGoPage.Location = new System.Drawing.Point(224, 21);
            this.txtGoPage.Name = "txtGoPage";
            this.txtGoPage.Size = new System.Drawing.Size(62, 21);
            this.txtGoPage.TabIndex = 4;
            // 
            // llNextPage
            // 
            this.llNextPage.AutoSize = true;
            this.llNextPage.Location = new System.Drawing.Point(112, 25);
            this.llNextPage.Name = "llNextPage";
            this.llNextPage.Size = new System.Drawing.Size(41, 12);
            this.llNextPage.TabIndex = 2;
            this.llNextPage.TabStop = true;
            this.llNextPage.Text = "下一页";
            this.llNextPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llNextPage_LinkClicked);
            // 
            // llLastPage
            // 
            this.llLastPage.AutoSize = true;
            this.llLastPage.Location = new System.Drawing.Point(174, 25);
            this.llLastPage.Name = "llLastPage";
            this.llLastPage.Size = new System.Drawing.Size(29, 12);
            this.llLastPage.TabIndex = 3;
            this.llLastPage.TabStop = true;
            this.llLastPage.Text = "尾页";
            this.llLastPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llLastPage_LinkClicked);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.sc1);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.sc1.Panel1.ResumeLayout(false);
            this.sc1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sc1)).EndInit();
            this.sc1.ResumeLayout(false);
            this.sc2.Panel1.ResumeLayout(false);
            this.sc2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sc2)).EndInit();
            this.sc2.ResumeLayout(false);
            this.sc3.Panel1.ResumeLayout(false);
            this.sc3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sc3)).EndInit();
            this.sc3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGV)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.planePage.ResumeLayout(false);
            this.planePage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer sc1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.SplitContainer sc2;
        private System.Windows.Forms.DataGridView dataGV;
        private System.Windows.Forms.GroupBox gbSelect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel llLastPage;
        private System.Windows.Forms.LinkLabel llNextPage;
        private System.Windows.Forms.LinkLabel llPreviousPage;
        private System.Windows.Forms.LinkLabel llFirstPage;
        private System.Windows.Forms.LinkLabel llGoPage;
        private System.Windows.Forms.TextBox txtGoPage;
        private System.Windows.Forms.Panel planePage;
        private System.Windows.Forms.SplitContainer sc3;
    }
}