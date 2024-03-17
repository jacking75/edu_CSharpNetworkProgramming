using CloudStructures;
using CloudStructures.Structures;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csharp_test_client
{
    public partial class mainForm : Form
    {
        RedisConnection Connection;


        ClientSimpleTcp Network = new ClientSimpleTcp();

        bool IsNetworkThreadRunning = false;
        bool IsBackGroundProcessRunning = false;

        System.Threading.Thread NetworkReadThread = null;
        System.Threading.Thread NetworkSendThread = null;

        PacketBufferManager PacketBuffer = new PacketBufferManager();
        Queue<byte[]> RecvPacketQueue = new Queue<byte[]>();
        Queue<byte[]> SendPacketQueue = new Queue<byte[]>();

        System.Windows.Threading.DispatcherTimer dispatcherUITimer;

        int CurTurn;
        Painter PanelPainter = new Painter();

        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            PacketBuffer.Init((8096 * 10), MsgPackPacketHeadInfo.HeadSize, 1024);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            IsBackGroundProcessRunning = true;
            dispatcherUITimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
            dispatcherUITimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherUITimer.Start();

            btnDisconnect.Enabled = false;

            SetPacketHandler();
            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsNetworkThreadRunning = false;
            IsBackGroundProcessRunning = false;

            Network.Close();
        }
        void NetworkReadProcess()
        {
            while (IsNetworkThreadRunning)
            {
                if (Network.IsConnected() == false)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                var recvData = Network.Receive();

                if (recvData != null)
                {
                    PacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                    while (true)
                    {
                        var data = PacketBuffer.Read();
                        if (data == null)
                        {
                            break;
                        }

                        lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                        {
                            RecvPacketQueue.Enqueue(data);
                        }
                    }
                    //DevLog.Write($"받은 데이터: {recvData.Item2}", LOG_LEVEL.INFO);
                }
                else
                {
                    Network.Close();
                    SetDisconnectd();
                    DevLog.Write("서버와 접속 종료 !!!", LOG_LEVEL.INFO);
                }
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(1);

                if (Network.IsConnected() == false)
                {
                    continue;
                }

                lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
                {
                    if (SendPacketQueue.Count > 0)
                    {
                        var packet = SendPacketQueue.Dequeue();
                        Network.Send(packet);
                    }
                }
            }
        }


        void BackGroundProcess(object sender, EventArgs e)
        {
            ProcessLog();

            try
            {
                byte[] packet = null;

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (RecvPacketQueue.Count() > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet != null)
                {
                    PacketProcess(packet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("BackGroundProcess. error:{0}", ex.Message));
            }
        }

        private void ProcessLog()
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (IsBackGroundProcessRunning)
            {
                System.Threading.Thread.Sleep(1);

                string msg;

                if (DevLog.GetLog(out msg))
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
        
        void SetDisconnectd()
        {
            if (btnConnect.Enabled == false)
            {
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
            }

            SendPacketQueue.Clear();

            listBoxRoomChatMsg.Items.Clear();
            listBoxRoomUserList.Items.Clear();

            labelStatus.Text = "서버 접속이 끊어짐";
        }

        void PostSendPacket(PACKET_ID packetID, byte[] packetData)
        {
            if (Network.IsConnected() == false)
            {
                DevLog.Write("서버 연결이 되어 있지 않습니다", LOG_LEVEL.ERROR);
                return;
            }

            var header = new MsgPackPacketHeadInfo();
            header.TotalSize = (UInt16)packetData.Length;
            header.Id = (UInt16)packetID;
            header.Type = 0;
            header.Write(packetData);

            SendPacketQueue.Enqueue(packetData);
        }
        void DrawMok()
        {
            var graphic = panel1.CreateGraphics();
            
            SolidBrush myBrush = new SolidBrush(PanelPainter.MokColor);
            graphic.FillEllipse(myBrush, new Rectangle(PanelPainter.PanelPosX, PanelPainter.PanelPosY, Painter.MokSize, Painter.MokSize));
            myBrush.Dispose();
        }

        RedisConnection GetRedisConnection()
        {
            if (Connection == null)
            {
                var config = new RedisConfig("test", textBox1.Text);
                Connection = new RedisConnection(config);
            }

            return Connection;
        }

        #region Room
        void AddRoomUserList(string userID)
        {
            var msg = userID;
            listBoxRoomUserList.Items.Add(msg);
        }

        void RemoveRoomUserList(string userID)
        {
            object removeItem = null;

            foreach(var user in listBoxRoomUserList.Items)
            {
                if(user.ToString() == userID)
                {
                    removeItem = user;
                    break;
                }
            }

            if (removeItem != null)
            {
                listBoxRoomUserList.Items.Remove(removeItem);
            }
        }

        void LeaveRoom()
        {
            listBoxRoomUserList.Items.Clear();
            listBoxRoomChatMsg.Items.Clear();
        }
        
        void AddRoomChatMessageList(string userID, string messsage)
        {
            var msg = $"{userID}: {messsage}";

            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add(msg);
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

        void SetRoomReady(string userID, bool isReady)
        {
            for (var i = 0; i < listBoxRoomUserList.Items.Count; i++)
            {
                var temp = listBoxRoomUserList.Items[i].ToString().Split('_');
                var curUserID = temp[temp.Length - 1];
                
                if (curUserID == userID)
                {
                    listBoxRoomUserList.Items[i] = isReady ? $"READY_{userID}" : userID;
                    break;
                }
            }
        }
        void EndOmok()
        {
            for (var i = 0; i < listBoxRoomUserList.Items.Count; i++)
            {
                var temp = listBoxRoomUserList.Items[i].ToString().Split('_');
                var curUserID = temp[temp.Length - 1];

                // Ready 해제
                listBoxRoomUserList.Items[i] = curUserID;
            }
            panel1.Refresh();
        }
        #endregion

        #region OnClick
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string address = textBoxIP.Text;

            if (checkBoxLocalHostIP.Checked)
            {
                address = "127.0.0.1";
            }

            int port = Convert.ToInt32(textBoxPort.Text);

            if (Network.Connect(address, port))
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;

                DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);
            }
            else
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            SetDisconnectd();
            Network.Close();
        }

        
        // 로그인 요청
        private void btn_Login_Click(object sender, EventArgs e)
        {
            var loginReq = new PKTReqLogin();
            loginReq.UserID = textBoxUserID.Text;
            loginReq.AuthToken = textBoxUserPW.Text;

            var sendPacketData = MessagePackSerializer.Serialize(loginReq);

            PostSendPacket(PACKET_ID.REQ_LOGIN, sendPacketData);            
            DevLog.Write($"로그인 요청: {textBoxUserID.Text}, {textBoxUserPW.Text}");
        }
        
        private void btn_RoomEnter_Click(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqRoomEnter();
            requestPkt.RoomNumber = textBoxRoomNumber.Text.ToInt32();

            var sendPacketData = MessagePackSerializer.Serialize(requestPkt);
            PostSendPacket(PACKET_ID.REQ_ROOM_ENTER, sendPacketData);
            DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
        }

        private void btn_RoomLeave_Click(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqRoomLeave();
            
            var sendPacketData = MessagePackSerializer.Serialize(requestPkt);
            PostSendPacket(PACKET_ID.REQ_ROOM_LEAVE, sendPacketData);
            DevLog.Write($"방 퇴장 요청:  {textBoxRoomNumber.Text} 번");
        }

        private void btnRoomChat_Click(object sender, EventArgs e)
        {
            if(textBoxRoomSendMsg.Text.IsEmpty())
            {
                MessageBox.Show("채팅 메시지를 입력하세요");
                return;
            }

            var requestPkt = new PKTReqRoomChat();
            requestPkt.ChatMessage = textBoxRoomSendMsg.Text;
            
            var sendPacketData = MessagePackSerializer.Serialize(requestPkt);
            PostSendPacket(PACKET_ID.REQ_ROOM_CHAT, sendPacketData);
            DevLog.Write($"방 채팅 요청");
        }
        
        private void btnReady_Click(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqReadyOmok();

            var sendPacketData = MessagePackSerializer.Serialize(requestPkt);
            PostSendPacket(PACKET_ID.REQ_READY_OMOK, sendPacketData);
            DevLog.Write($"오목 준비 요청");
        }
        
        private void btnPut_Click(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqPutMok
            {
                isSkip = false,
                PosX = int.Parse(xPosTextNumber.Text),
                PosY = int.Parse(yPosTextNumber.Text),
                Turn = CurTurn
            };
            
            var sendPacketData = MessagePackSerializer.Serialize(requestPkt);
            PostSendPacket(PACKET_ID.REQ_PUT_MOK, sendPacketData);
            DevLog.Write($"오목 두기 요청");
        }
        #endregion
                
        // 게임 서버 등록
        private async void button1_Click(object sender, EventArgs e)
        {
            var serverGroup = new PvPGameServerGroup();

            if (string.IsNullOrEmpty(textBox10.Text) == false)
            {
                serverGroup.ServerList.Add(new GameServerInfo()
                {
                    Index = textBox10.Text.ToInt32(),
                    IP = textBox4.Text,
                    Port = textBox5.Text.ToUInt16()
                });
            }

            if (string.IsNullOrEmpty(textBox8.Text) == false)
            {
                serverGroup.ServerList.Add(new GameServerInfo()
                {
                    Index = textBox8.Text.ToInt32(),
                    IP = textBox11.Text,
                    Port = textBox9.Text.ToUInt16()
                });
            }

            var key = KeyDefine.PvPGameServerList;
            var redis = new RedisString<PvPGameServerGroup>(GetRedisConnection(), key, null);
            await redis.DeleteAsync();
            await redis.SetAsync(serverGroup);

            await ReadGameServerGroup();
        }
        async Task ReadGameServerGroup()
        {
            listBox1.Items.Clear();

            var key = KeyDefine.PvPGameServerList;
            var redis = new RedisString<PvPGameServerGroup>(GetRedisConnection(), key, null);
            var result = await redis.GetAsync();

            if (result.HasValue == false)
            {
                return;
            }

            foreach (var server in result.Value.ServerList)
            {
                listBox1.Items.Add($"{server.Index} _ {server.IP} _ {server.Port}");
            }
        }

        // 매칭 요청
        private async void btnMatching_Click(object sender, EventArgs e)
        {
            const int msgID = 2;
            var msgData = new RequestPvPMatching
            {
                UserID = textBoxUserID.Text,
                RatingScore = 100
            };
            var msgPacket = MessagePackSerializer.Serialize(msgData);
            MsgPackHeaderInfo.WriteID(msgPacket, msgID);

            var key = KeyDefine.RequesMatchingQueue;
            var redis = new RedisList<byte[]>(GetRedisConnection(), key, null);
            var result = await redis.RightPushAsync(msgPacket);

            if (result > 0)
            {
                DevLog.Write("Redis에 매칭 요청 성공", LOG_LEVEL.INFO);
            }
            else
            {
                DevLog.Write("Redis에 매칭 요청 실패", LOG_LEVEL.ERROR);
            }
        }

        // 매칭 요청 확인
        private async void button3_Click(object sender, EventArgs e)
        {
            var key = KeyDefine.PrefixMatchingResult + textBoxUserID.Text;
            var redis = new RedisString<byte[]>(GetRedisConnection(), key, null);
            var result = await redis.GetAsync();

            if(result.HasValue)
            {
                var matchResult = MessagePackSerializer.Deserialize<PvPMatchingResult>(result.Value);

                textBoxIP.Text = matchResult.IP;
                textBoxPort.Text = matchResult.Port.ToString();
                textBoxRoomNumber.Text = matchResult.RoomNumber.ToString();
                textBoxUserPW.Text = matchResult.Token;

                DevLog.Write("Redis에 매칭 결과 받은. 접속 바람", LOG_LEVEL.INFO);
            }
            else
            {
                DevLog.Write("Redis에서 받은 매칭 결과 없음", LOG_LEVEL.INFO);
            }
        }
    }

    [MessagePackObject]
    public class PvPMatchingResult
    {
        [Key(0)]
        public string IP;
        [Key(1)]
        public UInt16 Port;
        [Key(2)]
        public Int32 RoomNumber;
        [Key(3)]
        public Int32 Index;
        [Key(4)]
        public string Token;
    }


    public class GameServerInfo
    {
        public Int32 Index;
        public string IP;
        public UInt16 Port;
    }
    public class PvPGameServerGroup
    {
        public List<GameServerInfo> ServerList = new List<GameServerInfo>();
    }

    public class KeyDefine
    {
        public const string RequesMatchingQueue = "req_matching";

        public const string PvPGameServerList = "pvp_gameServer_list";

        public const string PrefixGameServerRoomQueue = "gs_room_queue_";

        public const string PrefixMatchingResult = "ret_matching_";

        public const string PrefixMatchingResultToGameServer = "ret_newGmae_";
    }
}
