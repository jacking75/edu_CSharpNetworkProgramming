using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocketServer2
{
    class OutgoingDataPreparer
    {
        
        private DataHolder theDataHolder;

        public OutgoingDataPreparer()
        {            
        }

        internal void PrepareOutgoingData(SocketAsyncEventArgs e, DataHolder handledDataHolder)
        {
            DataHoldingUserToken theUserToken = (DataHoldingUserToken)e.UserToken;
            if (Program.watchProgramFlow == true)   //for testing
            {
                Program.testWriter.WriteLine("Mediator PrepareOutgoingData() " + theUserToken.TokenId);
            }
            
            theDataHolder = handledDataHolder;

            //In this example code, we will send back the receivedTransMissionId,
            // followed by the
            //message that the client sent to the server. And we must
            //prefix it with the length of the message. So we put 3 
            //things into the array.
            // 1) prefix,
            // 2) receivedTransMissionId,
            // 3) the message that we received from the client, which
            // we stored in our DataHolder until we needed it.
            //That is our communication protocol. The client must know the protocol.

            //Convert the receivedTransMissionId to byte array.
            Byte[] idByteArray = BitConverter.GetBytes(theDataHolder.receivedTransMissionId);

            //Determine the length of all the data that we will send back.
            Int32 lengthOfCurrentOutgoingMessage = idByteArray.Length + theDataHolder.dataMessageReceived.Length;

            //So, now we convert the length integer into a byte array.
            //Aren't byte arrays wonderful? Maybe you'll dream about byte arrays tonight!
            Byte[] arrayOfBytesInPrefix = BitConverter.GetBytes(lengthOfCurrentOutgoingMessage);
            
            //Create the byte array to send.
            theUserToken.dataToSend = new Byte[theUserToken.sendPrefixLength + lengthOfCurrentOutgoingMessage];
            
            //Now copy the 3 things to the theUserToken.dataToSend.
            Buffer.BlockCopy(arrayOfBytesInPrefix, 0, theUserToken.dataToSend, 0, theUserToken.sendPrefixLength);
            Buffer.BlockCopy(idByteArray, 0, theUserToken.dataToSend, theUserToken.sendPrefixLength, idByteArray.Length);
            //The message that the client sent is already in a byte array, in DataHolder.
            Buffer.BlockCopy(theDataHolder.dataMessageReceived, 0, theUserToken.dataToSend, theUserToken.sendPrefixLength + idByteArray.Length, theDataHolder.dataMessageReceived.Length);
            
            theUserToken.sendBytesRemainingCount = theUserToken.sendPrefixLength + lengthOfCurrentOutgoingMessage;
            theUserToken.bytesSentAlreadyCount = 0;
        }
    }
}
