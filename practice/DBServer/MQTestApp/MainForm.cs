using MessagePack;
using ServerCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQTestApp
{
    public partial class MainForm : Form
    {
        bool IsBackGroundProcessRunning = false;
        System.Windows.Threading.DispatcherTimer dispatcherUITimer;

        ConcurrentQueue<string> LogMsgQueue = new ConcurrentQueue<string>();
        ConcurrentQueue<byte[]> MQMsgQueue = new ConcurrentQueue<byte[]>();

        UInt16 MyIndex = 0;
        ServerCommon.MQSender MQRequest = new ServerCommon.MQSender();
        ServerCommon.MQReceiver MQResponse = new ServerCommon.MQReceiver();
   
        public MainForm()
        {
            InitializeComponent();
        }

        public void WriteLog(string log)
        {
            LogMsgQueue.Enqueue(log);
        }

        void BackGroundProcess(object sender, EventArgs e)
        {
            ProcessLog();

            try
            {
                if(MQMsgQueue.TryDequeue(out var mqData))
                {
                    ProcessPacket(mqData);
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
            }
        }

        private void ProcessLog()
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (IsBackGroundProcessRunning)
            {
                System.Threading.Thread.Sleep(1);

                if (LogMsgQueue.TryDequeue(out var msg))
                {
                    ++logWorkCount;

                    if (listBoxLog.Items.Count > 512)
                    {
                        listBoxLog.Items.Clear();
                    }

                    listBoxLog.Items.Add(msg);
                    listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                }
                else
                {
                    break;
                }

                if (logWorkCount > 8)
                {
                    break;
                }
            }
        }

     
        void InitMQ()
        {
            MQRequest.Init(textBox10.Text);
            MQResponse.Init(textBox10.Text, textBox1.Text, null);
            MQResponse.ReceivedMQData = ReceiveMQ;
        }

        void ReceiveMQ(byte[] mqData)
        {
            MQMsgQueue.Enqueue(mqData);
        }

        void SendMQ(MQPacketHeadInfo header, byte[] mqData)
        {
            header.Write(mqData);
            MQRequest.Send(textBox2.Text, mqData);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IsBackGroundProcessRunning = true;
            dispatcherUITimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
            dispatcherUITimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherUITimer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsBackGroundProcessRunning = false;

        }

        // 로바 서버용 MQ 연결 및 초기화
        private void button1_Click(object sender, EventArgs e)
        {
            MyIndex = textBox3.Text.ToUInt16();
            textBox1.Text += textBox3.Text;
            InitMQ();
            button1.Enabled = false;
        }

       
        // 오목 게임 유저 게임 전적 로딩 요청
        private void button3_Click(object sender, EventArgs e)
        {
            var request = new ServerCommon.MQReqGameRecord();
            request.UserUID = textBox8.Text.ToUInt64();
            var mqData = MessagePackSerializer.Serialize(request);

            MQPacketHeadInfo header = new MQPacketHeadInfo();
            header.Id = (UInt16)MqPacketId.MQ_REQ_GAME_RECORD;
            header.SenderIndex = MyIndex;
            header.UserUniqueId = 11111;

            SendMQ(header, mqData);
        }
    }
}
