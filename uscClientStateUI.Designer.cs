namespace GPM.Middleware.Core
{
    partial class uscClientStateUI
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

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labTask = new System.Windows.Forms.Label();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.labRecordingIndicator = new System.Windows.Forms.Label();
            this.btnResumeRecord = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer();
            this.btnKickout = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labClientRemoteEndPoint = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labConnectedTime = new System.Windows.Forms.Label();
            this.labIdlingSec = new System.Windows.Forms.Label();
            this.labclientTypeLab = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labTask
            // 
            this.labTask.AutoSize = true;
            this.labTask.Dock = System.Windows.Forms.DockStyle.Left;
            this.labTask.Font = new System.Drawing.Font("微軟正黑體", 11F, System.Drawing.FontStyle.Bold);
            this.labTask.Location = new System.Drawing.Point(33, 20);
            this.labTask.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labTask.Name = "labTask";
            this.labTask.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.labTask.Size = new System.Drawing.Size(42, 23);
            this.labTask.TabIndex = 1;
            this.labTask.Text = "IDLE";
            this.labTask.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnStopRecord.Enabled = false;
            this.btnStopRecord.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnStopRecord.Location = new System.Drawing.Point(5, 0);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(59, 30);
            this.btnStopRecord.TabIndex = 21;
            this.btnStopRecord.Text = "停止錄製";
            this.btnStopRecord.UseVisualStyleBackColor = true;
            this.btnStopRecord.Click += new System.EventHandler(this.btnStopRecord_Click);
            // 
            // labRecordingIndicator
            // 
            this.labRecordingIndicator.BackColor = System.Drawing.Color.Red;
            this.labRecordingIndicator.Dock = System.Windows.Forms.DockStyle.Left;
            this.labRecordingIndicator.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.labRecordingIndicator.ForeColor = System.Drawing.Color.White;
            this.labRecordingIndicator.Location = new System.Drawing.Point(123, 0);
            this.labRecordingIndicator.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labRecordingIndicator.Name = "labRecordingIndicator";
            this.labRecordingIndicator.Size = new System.Drawing.Size(89, 30);
            this.labRecordingIndicator.TabIndex = 20;
            this.labRecordingIndicator.Text = "影像錄製中";
            this.labRecordingIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labRecordingIndicator.Visible = false;
            // 
            // btnResumeRecord
            // 
            this.btnResumeRecord.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnResumeRecord.Enabled = false;
            this.btnResumeRecord.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnResumeRecord.Location = new System.Drawing.Point(64, 0);
            this.btnResumeRecord.Name = "btnResumeRecord";
            this.btnResumeRecord.Size = new System.Drawing.Size(59, 30);
            this.btnResumeRecord.TabIndex = 22;
            this.btnResumeRecord.Text = "繼續錄製";
            this.btnResumeRecord.UseVisualStyleBackColor = true;
            this.btnResumeRecord.Click += new System.EventHandler(this.btnResumeRecord_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnKickout
            // 
            this.btnKickout.BackColor = System.Drawing.Color.Red;
            this.btnKickout.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnKickout.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnKickout.Font = new System.Drawing.Font("新細明體-ExtB", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKickout.Location = new System.Drawing.Point(0, 20);
            this.btnKickout.Name = "btnKickout";
            this.btnKickout.Size = new System.Drawing.Size(33, 30);
            this.btnKickout.TabIndex = 24;
            this.btnKickout.Text = "踢除";
            this.btnKickout.UseVisualStyleBackColor = false;
            this.btnKickout.Click += new System.EventHandler(this.btnKickout_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.LightGray;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(75, 20);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            this.splitContainer1.Panel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(0, 3, 5, 0);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitContainer1.Panel2.Controls.Add(this.labRecordingIndicator);
            this.splitContainer1.Panel2.Controls.Add(this.btnResumeRecord);
            this.splitContainer1.Panel2.Controls.Add(this.btnStopRecord);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.splitContainer1.Size = new System.Drawing.Size(561, 30);
            this.splitContainer1.SplitterDistance = 265;
            this.splitContainer1.TabIndex = 25;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(0, 3);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(260, 23);
            this.textBox1.TabIndex = 23;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkCyan;
            this.panel1.Controls.Add(this.labClientRemoteEndPoint);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.labConnectedTime);
            this.panel1.Controls.Add(this.labIdlingSec);
            this.panel1.Controls.Add(this.labclientTypeLab);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.ForeColor = System.Drawing.Color.Black;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(636, 20);
            this.panel1.TabIndex = 26;
            // 
            // labClientRemoteEndPoint
            // 
            this.labClientRemoteEndPoint.BackColor = System.Drawing.Color.DarkCyan;
            this.labClientRemoteEndPoint.Dock = System.Windows.Forms.DockStyle.Top;
            this.labClientRemoteEndPoint.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labClientRemoteEndPoint.ForeColor = System.Drawing.Color.White;
            this.labClientRemoteEndPoint.Location = new System.Drawing.Point(181, 0);
            this.labClientRemoteEndPoint.Name = "labClientRemoteEndPoint";
            this.labClientRemoteEndPoint.Size = new System.Drawing.Size(238, 27);
            this.labClientRemoteEndPoint.TabIndex = 1;
            this.labClientRemoteEndPoint.Text = "label1";
            this.labClientRemoteEndPoint.Click += new System.EventHandler(this.labClientRemoteEndPoint_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(28, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Remote EndPoint : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.DarkCyan;
            this.label2.Dock = System.Windows.Forms.DockStyle.Right;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 8F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(419, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Connect Time : ";
            // 
            // labConnectedTime
            // 
            this.labConnectedTime.AutoSize = true;
            this.labConnectedTime.BackColor = System.Drawing.Color.DarkCyan;
            this.labConnectedTime.Dock = System.Windows.Forms.DockStyle.Right;
            this.labConnectedTime.Font = new System.Drawing.Font("微軟正黑體", 8F);
            this.labConnectedTime.ForeColor = System.Drawing.Color.White;
            this.labConnectedTime.Location = new System.Drawing.Point(506, 0);
            this.labConnectedTime.Name = "labConnectedTime";
            this.labConnectedTime.Size = new System.Drawing.Size(110, 15);
            this.labConnectedTime.TabIndex = 4;
            this.labConnectedTime.Text = "2022/07/12 23:00:00";
            // 
            // labIdlingSec
            // 
            this.labIdlingSec.AutoSize = true;
            this.labIdlingSec.BackColor = System.Drawing.Color.DarkCyan;
            this.labIdlingSec.Dock = System.Windows.Forms.DockStyle.Right;
            this.labIdlingSec.Font = new System.Drawing.Font("微軟正黑體", 8F);
            this.labIdlingSec.ForeColor = System.Drawing.Color.White;
            this.labIdlingSec.Location = new System.Drawing.Point(616, 0);
            this.labIdlingSec.Name = "labIdlingSec";
            this.labIdlingSec.Size = new System.Drawing.Size(20, 15);
            this.labIdlingSec.TabIndex = 5;
            this.labIdlingSec.Text = "(-)";
            // 
            // labclientTypeLab
            // 
            this.labclientTypeLab.AutoSize = true;
            this.labclientTypeLab.BackColor = System.Drawing.Color.Red;
            this.labclientTypeLab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labclientTypeLab.Dock = System.Windows.Forms.DockStyle.Left;
            this.labclientTypeLab.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.labclientTypeLab.ForeColor = System.Drawing.Color.White;
            this.labclientTypeLab.Location = new System.Drawing.Point(0, 0);
            this.labclientTypeLab.Name = "labclientTypeLab";
            this.labclientTypeLab.Size = new System.Drawing.Size(28, 23);
            this.labclientTypeLab.TabIndex = 6;
            this.labclientTypeLab.Text = "IR";
            // 
            // uscClientStateUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.labTask);
            this.Controls.Add(this.btnKickout);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.Name = "uscClientStateUI";
            this.Size = new System.Drawing.Size(636, 50);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labTask;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.Label labRecordingIndicator;
        private System.Windows.Forms.Button btnResumeRecord;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnKickout;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labClientRemoteEndPoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labConnectedTime;
        private System.Windows.Forms.Label labIdlingSec;
        private System.Windows.Forms.Label labclientTypeLab;
    }
}
