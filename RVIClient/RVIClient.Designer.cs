namespace RVIClient
{
    partial class RVIClient
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RVIClient));
            this.label1 = new System.Windows.Forms.Label();
            this.tb_IP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_UserName = new System.Windows.Forms.TextBox();
            this.tb_Port = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_connect = new System.Windows.Forms.Button();
            this.tb_log = new System.Windows.Forms.TextBox();
            this.tb_message = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_UserList = new System.Windows.Forms.ListBox();
            this.btn_sendMessage = new System.Windows.Forms.Button();
            this.btn_takePic = new System.Windows.Forms.Button();
            this.btn_disconnect = new System.Windows.Forms.Button();
            this.btn_exportLog = new System.Windows.Forms.Button();
            this.lb_dayShift = new System.Windows.Forms.Label();
            this.tb_DayShift = new System.Windows.Forms.TextBox();
            this.btn_Cleartb_log = new System.Windows.Forms.Button();
            this.cb_debug = new System.Windows.Forms.CheckBox();
            this.btn_cancel_connect = new System.Windows.Forms.Button();
            this.lable1 = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RecoverMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(186, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server IP";
            // 
            // tb_IP
            // 
            this.tb_IP.Location = new System.Drawing.Point(240, 4);
            this.tb_IP.Name = "tb_IP";
            this.tb_IP.Size = new System.Drawing.Size(100, 22);
            this.tb_IP.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(169, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "使用者名稱";
            // 
            // tb_UserName
            // 
            this.tb_UserName.Location = new System.Drawing.Point(240, 66);
            this.tb_UserName.Name = "tb_UserName";
            this.tb_UserName.Size = new System.Drawing.Size(100, 22);
            this.tb_UserName.TabIndex = 3;
            // 
            // tb_Port
            // 
            this.tb_Port.Location = new System.Drawing.Point(240, 33);
            this.tb_Port.Name = "tb_Port";
            this.tb_Port.Size = new System.Drawing.Size(100, 22);
            this.tb_Port.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(177, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Server Port";
            // 
            // btn_connect
            // 
            this.btn_connect.Location = new System.Drawing.Point(505, 146);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(42, 23);
            this.btn_connect.TabIndex = 6;
            this.btn_connect.Text = "連線";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Visible = false;
            this.btn_connect.Click += new System.EventHandler(this.btn_connect_Click);
            // 
            // tb_log
            // 
            this.tb_log.Location = new System.Drawing.Point(15, 91);
            this.tb_log.Multiline = true;
            this.tb_log.Name = "tb_log";
            this.tb_log.ReadOnly = true;
            this.tb_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_log.Size = new System.Drawing.Size(358, 177);
            this.tb_log.TabIndex = 8;
            // 
            // tb_message
            // 
            this.tb_message.Location = new System.Drawing.Point(74, 274);
            this.tb_message.Name = "tb_message";
            this.tb_message.Size = new System.Drawing.Size(299, 22);
            this.tb_message.TabIndex = 9;
            this.tb_message.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 277);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "發言內容";
            this.label4.Visible = false;
            // 
            // lb_UserList
            // 
            this.lb_UserList.FormattingEnabled = true;
            this.lb_UserList.ItemHeight = 12;
            this.lb_UserList.Location = new System.Drawing.Point(15, 33);
            this.lb_UserList.Name = "lb_UserList";
            this.lb_UserList.Size = new System.Drawing.Size(119, 40);
            this.lb_UserList.TabIndex = 11;
            // 
            // btn_sendMessage
            // 
            this.btn_sendMessage.Enabled = false;
            this.btn_sendMessage.Location = new System.Drawing.Point(379, 274);
            this.btn_sendMessage.Name = "btn_sendMessage";
            this.btn_sendMessage.Size = new System.Drawing.Size(75, 23);
            this.btn_sendMessage.TabIndex = 12;
            this.btn_sendMessage.Text = "送出";
            this.btn_sendMessage.UseVisualStyleBackColor = true;
            this.btn_sendMessage.Visible = false;
            this.btn_sendMessage.Click += new System.EventHandler(this.btn_sendMessage_Click);
            // 
            // btn_takePic
            // 
            this.btn_takePic.Enabled = false;
            this.btn_takePic.Location = new System.Drawing.Point(505, 202);
            this.btn_takePic.Name = "btn_takePic";
            this.btn_takePic.Size = new System.Drawing.Size(75, 23);
            this.btn_takePic.TabIndex = 13;
            this.btn_takePic.Text = "即時拍照";
            this.btn_takePic.UseVisualStyleBackColor = true;
            this.btn_takePic.Visible = false;
            this.btn_takePic.Click += new System.EventHandler(this.btn_takePic_Click);
            // 
            // btn_disconnect
            // 
            this.btn_disconnect.Enabled = false;
            this.btn_disconnect.Location = new System.Drawing.Point(552, 146);
            this.btn_disconnect.Name = "btn_disconnect";
            this.btn_disconnect.Size = new System.Drawing.Size(52, 23);
            this.btn_disconnect.TabIndex = 16;
            this.btn_disconnect.Text = "離線";
            this.btn_disconnect.UseVisualStyleBackColor = true;
            this.btn_disconnect.Visible = false;
            this.btn_disconnect.Click += new System.EventHandler(this.btn_disconnect_Click);
            // 
            // btn_exportLog
            // 
            this.btn_exportLog.Location = new System.Drawing.Point(504, 89);
            this.btn_exportLog.Name = "btn_exportLog";
            this.btn_exportLog.Size = new System.Drawing.Size(100, 23);
            this.btn_exportLog.TabIndex = 17;
            this.btn_exportLog.Text = "匯出作業log";
            this.btn_exportLog.UseVisualStyleBackColor = true;
            this.btn_exportLog.Visible = false;
            this.btn_exportLog.Click += new System.EventHandler(this.btn_exportLog_Click);
            // 
            // lb_dayShift
            // 
            this.lb_dayShift.AutoSize = true;
            this.lb_dayShift.Location = new System.Drawing.Point(508, 13);
            this.lb_dayShift.Name = "lb_dayShift";
            this.lb_dayShift.Size = new System.Drawing.Size(53, 12);
            this.lb_dayShift.TabIndex = 18;
            this.lb_dayShift.Text = "日期偏移";
            this.lb_dayShift.Visible = false;
            // 
            // tb_DayShift
            // 
            this.tb_DayShift.Location = new System.Drawing.Point(510, 28);
            this.tb_DayShift.Name = "tb_DayShift";
            this.tb_DayShift.Size = new System.Drawing.Size(100, 22);
            this.tb_DayShift.TabIndex = 19;
            this.tb_DayShift.Text = "0";
            this.tb_DayShift.Visible = false;
            // 
            // btn_Cleartb_log
            // 
            this.btn_Cleartb_log.Location = new System.Drawing.Point(504, 118);
            this.btn_Cleartb_log.Name = "btn_Cleartb_log";
            this.btn_Cleartb_log.Size = new System.Drawing.Size(75, 23);
            this.btn_Cleartb_log.TabIndex = 20;
            this.btn_Cleartb_log.Text = "清除tb_log";
            this.btn_Cleartb_log.UseVisualStyleBackColor = true;
            this.btn_Cleartb_log.Visible = false;
            this.btn_Cleartb_log.Click += new System.EventHandler(this.btn_Cleartb_log_Click);
            // 
            // cb_debug
            // 
            this.cb_debug.AutoSize = true;
            this.cb_debug.Location = new System.Drawing.Point(510, 57);
            this.cb_debug.Name = "cb_debug";
            this.cb_debug.Size = new System.Drawing.Size(82, 16);
            this.cb_debug.TabIndex = 21;
            this.cb_debug.Text = "DebugMode";
            this.cb_debug.UseVisualStyleBackColor = true;
            this.cb_debug.Visible = false;
            // 
            // btn_cancel_connect
            // 
            this.btn_cancel_connect.Location = new System.Drawing.Point(505, 174);
            this.btn_cancel_connect.Name = "btn_cancel_connect";
            this.btn_cancel_connect.Size = new System.Drawing.Size(65, 23);
            this.btn_cancel_connect.TabIndex = 22;
            this.btn_cancel_connect.Text = "停止倒數";
            this.btn_cancel_connect.UseVisualStyleBackColor = true;
            this.btn_cancel_connect.Visible = false;
            this.btn_cancel_connect.Click += new System.EventHandler(this.btn_cancel_connect_Click);
            // 
            // lable1
            // 
            this.lable1.AutoSize = true;
            this.lable1.Location = new System.Drawing.Point(15, 13);
            this.lable1.Name = "lable1";
            this.lable1.Size = new System.Drawing.Size(65, 12);
            this.lable1.TabIndex = 23;
            this.lable1.Text = "線上使用者";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "RVIClient";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExitMenuItem,
            this.RecoverMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(99, 48);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(98, 22);
            this.ExitMenuItem.Text = "關閉";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // RecoverMenuItem
            // 
            this.RecoverMenuItem.Name = "RecoverMenuItem";
            this.RecoverMenuItem.Size = new System.Drawing.Size(98, 22);
            this.RecoverMenuItem.Text = "還原";
            this.RecoverMenuItem.Click += new System.EventHandler(this.RecoverMenuItem_Click);
            // 
            // RVIClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 271);
            this.Controls.Add(this.lable1);
            this.Controls.Add(this.btn_cancel_connect);
            this.Controls.Add(this.cb_debug);
            this.Controls.Add(this.btn_Cleartb_log);
            this.Controls.Add(this.tb_DayShift);
            this.Controls.Add(this.lb_dayShift);
            this.Controls.Add(this.btn_exportLog);
            this.Controls.Add(this.btn_disconnect);
            this.Controls.Add(this.btn_takePic);
            this.Controls.Add(this.btn_sendMessage);
            this.Controls.Add(this.lb_UserList);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_message);
            this.Controls.Add(this.tb_log);
            this.Controls.Add(this.btn_connect);
            this.Controls.Add(this.tb_Port);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_UserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_IP);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RVIClient";
            this.Text = "RVIClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RVIClient_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RVIClient_FormClosed);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_IP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_UserName;
        private System.Windows.Forms.TextBox tb_Port;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.TextBox tb_log;
        private System.Windows.Forms.TextBox tb_message;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lb_UserList;
        private System.Windows.Forms.Button btn_sendMessage;
        private System.Windows.Forms.Button btn_takePic;
        private System.Windows.Forms.Button btn_disconnect;
        private System.Windows.Forms.Button btn_exportLog;
        private System.Windows.Forms.Label lb_dayShift;
        private System.Windows.Forms.TextBox tb_DayShift;
        private System.Windows.Forms.Button btn_Cleartb_log;
        private System.Windows.Forms.CheckBox cb_debug;
        private System.Windows.Forms.Button btn_cancel_connect;
        private System.Windows.Forms.Label lable1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RecoverMenuItem;
    }
}

