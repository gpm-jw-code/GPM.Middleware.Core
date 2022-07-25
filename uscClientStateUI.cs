using GPM.Middleware.Core.Models.Communication;
using GPM.Middleware.Core.Models.Communication.Middleware;
using GPM.Middleware.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPM.Middleware.Core
{
    public partial class uscClientStateUI : UserControl
    {
        clsDataRecorder RecordInstance => StaUtility.ControlMiddleware.tcpClientRecords.ContainsKey(Client) ?
                                        StaUtility.ControlMiddleware.tcpClientRecords[Client] : null;
        public clsServer.Client Client { get; }

        public uscClientStateUI()
        {
            InitializeComponent();
        }


        public uscClientStateUI(clsServer.Client client)
        {
            InitializeComponent();
            Client = client;
            Client.CmdReceieved += Client_CmdReceieved;
            labClientRemoteEndPoint.Text = client.EndPoint;
            labConnectedTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            labclientTypeLab.BackColor = client.clientType == clsServer.Client.CLIENT_TYPE.IR ? Color.Red : Color.Blue;
            labclientTypeLab.Text = client.clientType.ToString();
            timer1.Enabled = true;
            if (client.clientType == clsServer.Client.CLIENT_TYPE.LR)
                btnStopRecord.Visible = btnResumeRecord.Visible = labRecordingIndicator.Visible = false;

        }

        private void Client_CmdReceieved(object sender, string e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                textBox1.Text = e;
            });
        }


        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            RecordInstance.StopRecord();
            btnResumeRecord.Enabled = true;

        }
        private void btnResumeRecord_Click(object sender, EventArgs e)
        {
            btnResumeRecord.Enabled = false;
            if (!RecordInstance.isRecording)
                return;
            RecordInstance.ResumeRecord();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                labIdlingSec.Text = Client.IdlingSec.ToString();
                if (RecordInstance != null)
                {
                    if (Client.clientType == clsServer.Client.CLIENT_TYPE.IR && RecordInstance.videoRecord)
                        btnStopRecord.Enabled = labRecordingIndicator.Visible = RecordInstance.isRecording && !RecordInstance.manualStopRecord;
                    else
                        btnStopRecord.Visible = btnResumeRecord.Visible = labRecordingIndicator.Visible = false;

                    labTask.Text = RecordInstance.isRecording ? $"執行遠端任務中 ({RecordInstance.RecordedDataNum}/{RecordInstance.TotalDataQty})" : "IDLE";
                }
            });
        }

        private void btnKickout_Click(object sender, EventArgs e)
        {
            if (Client.isRecording)
            {
                if (MessageBox.Show("尚在進行遠端作業，確定要剔除?", "Kick your ass", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }

            StaUtility.ControlMiddleware.KickOut(Client);
        }

        private void labClientRemoteEndPoint_Click(object sender, EventArgs e)
        {

        }

        internal void UpdateState(string errMsg)
        {
            BeginInvoke((MethodInvoker)delegate
            {

                labTask.Text = errMsg;
            });
        }

    }
}
