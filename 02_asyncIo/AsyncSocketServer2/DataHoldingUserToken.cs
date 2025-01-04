using System;
using System.Net.Sockets;
using System.Threading;


namespace AsyncSocketServer2;

class DataHoldingUserToken
{
    internal Mediator theMediator;
    internal DataHolder theDataHolder;

    internal Int32 socketHandleNumber;

    internal readonly Int32 bufferOffsetReceive;
    internal readonly Int32 permanentReceiveMessageOffset;
    internal readonly Int32 bufferOffsetSend;

    private Int32 idOfThisObject; //테스트용        

    internal Int32 lengthOfCurrentIncomingMessage;

    //receiveMessageOffset는 수신 버퍼에서 메시지가 시작되는 바이트 위치를 표시하는 데 사용됩니다.
    //이 값은 수신된 데이터 스트림의 범위를 벗어날 수도 있습니다. 그러나 범위를 벗어나면
    //코드에서 액세스하지 않습니다.
    internal Int32 receiveMessageOffset;
    internal Byte[] byteArrayForPrefix;
    internal readonly Int32 receivePrefixLength;
    internal Int32 receivedPrefixBytesDoneCount = 0;
    internal Int32 receivedMessageBytesDoneCount = 0;
    //이 변수는 receiveSendToken.receivePrefixBytesDone 변수와 이름이 비슷하지만 사용 방법이 다릅니다.
    //한 상황에서 receiveMessageOffset 변수의 값을 계산하는 데 필요합니다.
    internal Int32 recPrefixBytesDoneThisOp = 0;

    internal Int32 sendBytesRemainingCount;
    internal readonly Int32 sendPrefixLength;
    internal Byte[] dataToSend;
    internal Int32 bytesSentAlreadyCount;

    //세션 ID는 연결된 세션에서 전송된 모든 데이터와 관련이 있습니다.
    //이는 DataHolder의 전송 ID와는 다르며, 하나의 TCP 메시지와 관련이 있습니다.
    //연결된 세션은 앱에서 허용하는 경우 여러 메시지를 가질 수 있습니다.
    private Int32 sessionId;

    public DataHoldingUserToken(SocketAsyncEventArgs e, Int32 rOffset, Int32 sOffset, Int32 receivePrefixLength, Int32 sendPrefixLength, Int32 identifier)
    {
        this.idOfThisObject = identifier;

        //SAEA 개체에 대한 참조를 가진 Mediator를 생성합니다.
        this.theMediator = new Mediator(e);
        this.bufferOffsetReceive = rOffset;
        this.bufferOffsetSend = sOffset;
        this.receivePrefixLength = receivePrefixLength;
        this.sendPrefixLength = sendPrefixLength;
        this.receiveMessageOffset = rOffset + receivePrefixLength;
        this.permanentReceiveMessageOffset = this.receiveMessageOffset;
    }

    //테스트를 위해 이 개체에 대해 ID를 사용해 보겠습니다.
    public Int32 TokenId
    {
        get
        {
            return this.idOfThisObject;
        }
    }

    internal void CreateNewDataHolder()
    {
        theDataHolder = new DataHolder();
    }

    //DataHoldingUserToken에서 sessionId 변수를 생성하는 데 사용됩니다.
    //ProcessAccept()에서 호출됩니다.
    internal void CreateSessionId()
    {
        sessionId = Interlocked.Increment(ref Program.mainSessionId);
    }

    public Int32 SessionId
    {
        get
        {
            return this.sessionId;
        }
    }

    public void Reset()
    {
        this.receivedPrefixBytesDoneCount = 0;
        this.receivedMessageBytesDoneCount = 0;
        this.recPrefixBytesDoneThisOp = 0;
        this.receiveMessageOffset = this.permanentReceiveMessageOffset;
    }
}
