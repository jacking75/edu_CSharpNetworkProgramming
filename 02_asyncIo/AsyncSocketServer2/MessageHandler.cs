using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocketServer2;

class MessageHandler
{

    public bool HandleMessage(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken receiveSendToken, Int32 remainingBytesToProcess)
    {
        bool incomingTcpMessageIsReady = false;

        // 이전 수신 작업에서 생성되지 않은 경우, 완전한 메시지를 저장할 배열을 생성합니다.
        if (receiveSendToken.receivedMessageBytesDoneCount == 0)
        {
            if (Program.watchProgramFlow == true)   // 테스트용
            {
                Program.testWriter.WriteLine("MessageHandler, receive 배열 생성 " + receiveSendToken.TokenId);
            }


            receiveSendToken.theDataHolder.dataMessageReceived = new Byte[receiveSendToken.lengthOfCurrentIncomingMessage];
        }

        // receiveSendToken.receivedPrefixBytesDoneCount 변수를 기억하세요.
        // 이 변수는 접두사를 처리하는 데 도움이 되었으며,
        // 여러 개의 수신 작업이 필요한 경우에도 처리할 수 있도록 해줍니다.
        // 마찬가지로, receiveSendToken.receivedMessageBytesDoneCount 변수도 있습니다.
        // 이 변수는 메시지 데이터를 처리하는 데 도움이 되며,
        // 수신 작업이 한 번이든 여러 번이든 상관없이 처리할 수 있도록 해줍니다.
        if (remainingBytesToProcess + receiveSendToken.receivedMessageBytesDoneCount == receiveSendToken.lengthOfCurrentIncomingMessage)
        {
            // 이 if 문 안에 들어왔다면, 메시지의 끝에 도달했습니다.
            // 즉, 이 메시지에 대해 수신한 총 바이트 수가 접두사에서 얻은 메시지 길이 값과 일치합니다.

            if (Program.watchProgramFlow == true)   // 테스트용
            {
                Program.testWriter.WriteLine("MessageHandler, 길이가 올바름 " + receiveSendToken.TokenId);
            }

            // 수신한 바이트를 DataHolder 개체에 있는 바이트 배열에 기록/추가합니다.
            Buffer.BlockCopy(receiveSendEventArgs.Buffer, receiveSendToken.receiveMessageOffset, receiveSendToken.theDataHolder.dataMessageReceived, receiveSendToken.receivedMessageBytesDoneCount, remainingBytesToProcess);

            incomingTcpMessageIsReady = true;
        }

        else
        {
            // 이 else 문 안에 들어왔다면, 추가 수신 작업이 필요합니다.
            // 아직 메시지를 완전히 받지 못했으며,
            // 수신한 데이터를 모두 검사했습니다.
            // 문제 없습니다. SocketListener.ProcessReceive에서
            // StartReceive를 호출하여 추가 데이터를 수신하기 위해
            // 다른 수신 작업을 수행합니다.

            if (Program.watchProgramFlow == true)   // 테스트용
            {
                Program.testWriter.WriteLine(" MessageHandler, 길이가 부족함 " + receiveSendToken.TokenId);
            }

            Buffer.BlockCopy(receiveSendEventArgs.Buffer, receiveSendToken.receiveMessageOffset, receiveSendToken.theDataHolder.dataMessageReceived, receiveSendToken.receivedMessageBytesDoneCount, remainingBytesToProcess);

            receiveSendToken.receiveMessageOffset = receiveSendToken.receiveMessageOffset - receiveSendToken.recPrefixBytesDoneThisOp;

            receiveSendToken.receivedMessageBytesDoneCount += remainingBytesToProcess;
        }

        return incomingTcpMessageIsReady;
    }
}
